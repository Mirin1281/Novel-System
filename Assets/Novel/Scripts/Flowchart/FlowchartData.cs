using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;
using System.Threading;

namespace Novel
{
    [CreateAssetMenu(
        fileName = "Flow",
        menuName = ConstContainer.DATA_CREATE_PATH + "Flowchart",
        order = 1)
    ]
    public class FlowchartData : ScriptableObject
    {
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">コマンドの初期インデックス</param>
        /// <param name="token">キャンセル用のトークン(通常はStop()があるので不要)</param>
        public void Execute(int index = 0, CancellationToken token = default)
        {
            ExecuteAsync(index, token);
        }
        public UniTask ExecuteAsync(int index = 0, CancellationToken token = default)
        {
            return flowchart.ExecuteAsync(index, token);
        }

        /// <summary>
        /// 呼び出しているフローチャートを停止します。その際に表示されているUIはフェードアウトされます
        /// </summary>
        public void Stop()
        {
            flowchart.Stop(Flowchart.StopType.IncludeParent, isClearUI: true);
        }

#if UNITY_EDITOR
        /// <summary>
        /// リストの中に特定のCommandDataがあるか調べます
        /// </summary>
        public bool IsUsed(CommandData targetData)
        {
            foreach (var cmdData in flowchart.GetReadOnlyCommandDataList())
            {
                if (cmdData == targetData) return true;
            }
            return false;
        }
#endif
    }
}