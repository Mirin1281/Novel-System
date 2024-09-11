using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;

namespace Novel
{
    // 【説明】
    // FlowchartはMonoBehaviour型とScriptableObject型があります
    // MonoBehaviourはシーン内で参照が取れるためできることが多いです
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効きます

    // Flowchartは中身が多いですが、実際に使用するのはExecuteAsync()とStop()がほとんどです
    
    [Serializable]
    public class Flowchart
    {
        public enum StopType
        {
            [InspectorName("このフローチャートのみ")] Single,
            [InspectorName("入れ子の親も含む全て")] All,
        }

        [SerializeField, TextArea]
        string description = "説明";

        [SerializeField, Tooltip("Zoneコマンドを使用する場合のみtrueにしてください")]
        bool isCheckZone;

        // シリアライズする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        bool isSingleStopped;
        bool isCalling;
        int callIndex;

        public FlowchartCallStatus CallStatus { get; set; }
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">リストの何番目から発火するか</param>
        /// <param name="token">キャンセル用のトークン</param>
        public UniTask ExecuteAsync(int index = 0, CancellationToken token = default)
        {
            SetStatus(token);
            return PrecessAsync(index);
        }

        // 通常、こちらは外部から呼び出しません
        public UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            CallStatus = callStatus;
            return PrecessAsync(index);
        }

        async UniTask PrecessAsync(int index)
        {
            isCalling = true;
            SetIndex(index, false);
            if (isCheckZone) ApplyZone(commandDataList, callIndex);

            while (commandDataList.Count > callIndex && isSingleStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this);
                callIndex++;
            }

            if (isSingleStopped == false && CallStatus.IsNestCalled == false)
            {
                NovelManager.Instance.ClearAllUI();
            }
            isSingleStopped = false;
            isCalling = false;
        }

        /// <summary>
        /// フローチャートを停止します
        /// </summary>
        public void Stop(StopType stopType = StopType.All, bool isClearUI = false)
        {
            if (stopType == StopType.All)
            {
                CallStatus.Cts?.Cancel();
                if(isCalling && isClearUI)
                {
                    NovelManager.Instance.ClearAllUI();
                }
            }
            else if (stopType == StopType.Single)
            {
                isSingleStopped = true;
            }
        }

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

        void SetStatus(CancellationToken token)
        {
            CancellationTokenSource cts = new();
            if (token != default)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token);
            }
            CallStatus = new(cts.Token, cts, false);
        }    

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
        UniTask ExecuteAsync(int index = 0, CancellationToken token = default);
    }

    /// <summary>
    /// Token, TokenSourceと"入れ子で呼ばれたか"の3つの情報を保持します
    /// </summary>
    public class FlowchartCallStatus
    {
        public readonly CancellationToken Token;
        public readonly CancellationTokenSource Cts;
        public readonly bool IsNestCalled;

        public FlowchartCallStatus(CancellationToken token, CancellationTokenSource cts, bool isNestCalled)
        {
            Token = token;
            Cts = cts;
            IsNestCalled = isNestCalled;
        }
    }
}