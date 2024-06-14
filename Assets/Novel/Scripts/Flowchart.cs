using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;

namespace Novel
{
    // FlowchartはMonoBehaviour型とScriptableObject型がある
    // MonoBehaviourはシーン内で参照が取れるためできることが多い
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効く
    
    [Serializable]
    public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        [SerializeField, Tooltip("Zoneコマンドを使用する場合のみtrueにしてください")]
        bool isCheckZone;

        // シリアライズする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        bool isStopped;

        CancellationTokenSource cts;

        int callIndex;

        /// <summary>
        /// 現在呼ばれているフローチャートのリスト
        /// </summary>
        public static List<Flowchart> CurrentExecutingFlowcharts;

        // コマンド内で呼ばれた際は抜ける際にindexを+1するので、その分予め引いておく
        public void SetIndex(int index, bool calledInCommand)
        {
            if(calledInCommand)
            {
                callIndex = index - 1;
            }
            else
            {
                callIndex = index;
            }
        }

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">リストの何番目から発火するか</param>
        /// <param name="callStatus">他のフローチャートから呼び出された時に渡される情報</param>
        public async UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus = null)
        {
            if(CurrentExecutingFlowcharts == null)
            {
                CurrentExecutingFlowcharts = new(3);
            }
            CurrentExecutingFlowcharts.Add(this);
            SetIndex(index, false);
            if (isCheckZone) ApplyZone(commandDataList, callIndex);
            var status = SetStatus(callStatus, ref cts);

            while (commandDataList.Count > callIndex && isStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this, status);
                callIndex++;
            }

            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;
            CurrentExecutingFlowcharts.Remove(this);


            // 呼び出し時のインデックスを見て、それよりも上に存在する
            // IZoneCommandのついたコマンドを発火します(詳しくはIZoneCommand参照)
            static void ApplyZone(IList<CommandData> commandDataList, int currentIndex)
            {
                for (int i = 0; i < commandDataList.Count; i++)
                {
                    if (commandDataList[i].GetCommandBase() is IZoneCommand zoneCommand)
                    {
                        if(i < currentIndex)
                        {
                            zoneCommand.CallZone();
                        }
                    }
                }
            }

            // FlowchartCallStatusをctsに反映します。
            // statusがnullだった場合は初期化をします
            static FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus, ref CancellationTokenSource cts)
            {
                if (callStatus == null)
                {
                    cts = new();
                    return new FlowchartCallStatus(cts.Token, cts, false);
                }
                else
                {
                    cts = callStatus.Cts;
                    return callStatus;
                }
            }
        }

        public enum StopType
        {
            [InspectorName("このフローチャートのみ")] Single,
            [InspectorName("入れ子の親も含む全て")] All,
        }

        /// <summary>
        /// コマンドリストの切れ目でフローチャートを停止します
        /// </summary>
        public void Stop(StopType stopType)
        {
            if (stopType == StopType.All)
            {
                cts?.Cancel();
            }
            else if (stopType == StopType.Single)
            {
                isStopped = true;
            }
        }

#if UNITY_EDITOR
        public string Description => description;
        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(List<CommandData> list)
        {
            commandDataList = list;
        }
#endif
    }

    public interface IFlowchartObject
    {
        string Name { get; }
        Flowchart Flowchart { get; }
        UniTask ExecuteAsync(int index = 0);
    }

    /// <summary>
    /// Token, TokenSourceと"入れ子で呼ばれたか"の3つの情報を保持します
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