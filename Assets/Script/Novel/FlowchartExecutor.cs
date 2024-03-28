using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
#pragma warning disable 0414 // value is never used の警告を消すため

namespace Novel
{
    // FlowchartはMonoBehaviour型とScriptableObject型がある
    // MonoBehaviourはシーン内で参照が取れるためできることが多い
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効く
    public class FlowchartExecutor : MonoBehaviour
    {
        [SerializeField] bool isStartExecute;
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        void Start()
        {
            if (isStartExecute) flowchart.ExecuteAsync().Forget();
        }

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            await flowchart.ExecuteAsync(index, callStatus);
        }
    }

    [Serializable]
    public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        // シリアライズはする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();

        public IEnumerable<CommandBase> GetCommandBaseList()
            => commandDataList.Select(data => data.GetCommandBase());

        public IEnumerable<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(IEnumerable<CommandData> list)
        {
            commandDataList = list as List<CommandData>;
        }

        bool isStopped;
        CancellationTokenSource cts;

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            var status = SetStatus(callStatus);

            while (commandDataList.Count > index && isStopped == false)
            {
                var cmdData = commandDataList[index];
                await cmdData.CallAsync(this, index, status);
                index++;
            }
            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;
        }

        public void Stop(FlowchartStopType stopType)
        {
            if (stopType == FlowchartStopType.IncludeParent)
            {
                cts.Cancel();
            }
            else if (stopType == FlowchartStopType.Single)
            {
                isStopped = true;
            }
        }

        /// <summary>
        /// FlowchartCallStatusをctsに反映します。
        /// statusがnullだった場合は初期化をします
        /// </summary>
        FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus)
        {
            if (callStatus == null)
            {
                cts = new CancellationTokenSource();
                return new FlowchartCallStatus(cts.Token, cts, false);
            }
            else
            {
                cts = callStatus.Cts;
                return callStatus;
            }
        }
    }

    public enum FlowchartStopType
    {
        [InspectorName("このフローチャートのみ")] Single,
        [InspectorName("入れ子の親も含む")] IncludeParent,
    }

    public class FlowchartCallStatus
    {
        public readonly CancellationToken Token;
        public readonly CancellationTokenSource Cts;
        public readonly bool IsNestCalled;

        public FlowchartCallStatus(
            CancellationToken token, CancellationTokenSource cts, bool isNestCalled)
        {
            Token = token;
            Cts = cts;
            IsNestCalled = isNestCalled;
        }
    }
}