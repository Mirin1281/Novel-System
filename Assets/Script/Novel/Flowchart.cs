using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
#pragma warning disable 0414 // descriptionの"value is never used"警告を消すため

namespace Novel
{
    [Serializable]
    public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        // シリアライズする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();

        public List<CommandData> GetCommandDataList() => commandDataList;

        public List<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        /// <summary>
        /// カスタムエディタ用
        /// </summary>
        public void SetCommandDataList(List<CommandData> list)
        {
            commandDataList = list;
        }

        bool isStopped;
        CancellationTokenSource cts;

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">リストの何番目から発火するか</param>
        /// <param name="callStatus">他のフローチャートから呼び出された時の情報</param>
        public async UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            var status = SetStatus(callStatus);

            while (commandDataList.Count > index && isStopped == false)
            {
                var cmdData = commandDataList[index];
                await cmdData.ExecuteAsync(this, status);
                index++;
            }
            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;


            /// <summary>
            /// FlowchartCallStatusをctsに反映します。
            /// statusがnullだった場合は初期化をします
            /// </summary>
            FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus)
            {
                if (callStatus == null)
                {
                    cts = new CancellationTokenSource();
                    cts = CancellationTokenSource.CreateLinkedTokenSource(
                            MyStatic.TokenOnSceneChange, cts.Token);
                    return new FlowchartCallStatus(cts.Token, cts, false);
                }
                else
                {
                    cts = callStatus.Cts;
                    return callStatus;
                }
            }
        }

        /// <summary>
        /// コマンドリストの切れ目でフローチャートを停止します
        /// </summary>
        public void Stop(FlowchartStopType stopType)
        {
            if (stopType == FlowchartStopType.All)
            {
                cts?.Cancel();
            }
            else if (stopType == FlowchartStopType.Single)
            {
                isStopped = true;
            }
        }
    }

    public enum FlowchartStopType
    {
        [InspectorName("このフローチャートのみ")] Single,
        [InspectorName("入れ子の親も含む全て")] All,
    }

    /// <summary>
    /// Token, TokenSourceと入れ子で呼ばれたかの3つの情報を格納します
    /// </summary>
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