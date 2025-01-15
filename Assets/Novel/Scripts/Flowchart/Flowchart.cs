using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
using System.Linq;

namespace Novel
{
    [Serializable]
    public class Flowchart
    {
        public enum StopType
        {
            [InspectorName("このフローチャートのみ")] Single,
            [InspectorName("await中の親も含む")] IncludeParent,
        }

        // シリアライズする
        [SerializeField]
        List<CommandData> commandDataList = new();

        /// <summary>
        /// 現在呼び出されているか
        /// </summary>
        bool isCalling;

        /// <summary>
        /// 呼び出しているコマンドのインデックス
        /// </summary>
        int callIndex;

        /// <summary>
        /// 停止させる際のフラグ
        /// </summary>
        bool isSingleStopped;

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

        // 通常、こちらはユーザーから呼び出しません
        public UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            SetStatus(callStatus);
            return PrecessAsync(index);
        }

        async UniTask PrecessAsync(int index)
        {
            isCalling = true;
            SetIndex(index, false);

            while (commandDataList.Count > callIndex && isSingleStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this);
                callIndex++;
            }

            if (CallStatus.ExistWaitOthers == false && isSingleStopped == false)
            {
                NovelManager.Instance.ClearAllUI();
            }
            isSingleStopped = false;
            isCalling = false;
            callIndex = 0;
        }

        /// <summary>
        /// フローチャートを停止します
        /// </summary>
        public void Stop(StopType stopType = StopType.IncludeParent, bool isClearUI = false)
        {
            if (stopType == StopType.IncludeParent)
            {
                CallStatus.Cts?.Cancel();
                if (isCalling && isClearUI)
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
            if (calledInCommand)
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
            CallStatus = new(cts, false);
        }
        void SetStatus(FlowchartCallStatus status)
        {
            CallStatus = status;
        }

#if UNITY_EDITOR
        [SerializeField, TextArea, Tooltip("エディタでのみ使用されます")]
        string description;
        public string Description => description;

        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(IEnumerable<CommandData> commands)
        {
            commandDataList = commands.ToList();
            for (int i = 0; i < commandDataList.Count; i++)
            {
                var cmd = commandDataList[i].GetCommandBase();
                if (cmd == null) continue;
                cmd.SetFlowchart(this);
                cmd.SetIndex(i);
            }
        }

        public bool EqualsCommands(Flowchart other)
        {
            if (other == null) return false;
            var myList = GetReadOnlyCommandDataList();
            var otherList = other.GetReadOnlyCommandDataList();
            if (myList == null && otherList == null) return true;
            if (myList == null || otherList == null || myList.Count != otherList.Count) return false;

            bool isEqual = true;
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i] != otherList[i])
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }
#endif
    }

    /// <summary>
    /// Token, TokenSourceと"他のフローチャートが終了を待機しているか"の3つの情報を保持します
    /// </summary>
    public class FlowchartCallStatus
    {
        public readonly CancellationTokenSource Cts;
        public readonly CancellationToken Token;

        /// <summary>
        /// 待機している別のフローチャートが存在するか
        /// </summary>
        public readonly bool ExistWaitOthers;

        public FlowchartCallStatus(CancellationTokenSource cts, bool existWaitOthers)
        {
            Token = cts.Token;
            Cts = cts;
            ExistWaitOthers = existWaitOthers;
        }
    }
}