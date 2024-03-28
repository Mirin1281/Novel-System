using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
#pragma warning disable 0414 // value is never used の警告を消すため

namespace Novel
{
    [CreateAssetMenu(
        fileName = "FlowchartData",
        menuName = "ScriptableObject/FlowchartData")
    ]
    public class FlowchartData : ScriptableObject, IFlowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        // シリアライズはする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();

        IEnumerable<CommandBase> IFlowchart.GetCommandBaseList()
            => commandDataList.Select(data => data.GetCommandBase());

        IEnumerable<CommandData> IFlowchart.GetCommandDataList() => commandDataList;
        void IFlowchart.SetCommandDataList(IEnumerable<CommandData> list)
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

        void IFlowchart.Stop(FlowchartStopType stopType)
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
    }
}