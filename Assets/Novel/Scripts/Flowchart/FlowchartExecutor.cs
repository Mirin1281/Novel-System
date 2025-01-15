using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class FlowchartExecutor : MonoBehaviour
    {
        [SerializeField] bool executeOnStart;
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        void Start()
        {
            if (executeOnStart)
                Execute();
        }
        void OnDestroy()
        {
            flowchart = null;
        }

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
            if (token == default)
            {
                token = this.GetCancellationTokenOnDestroy();
            }
            return Flowchart.ExecuteAsync(index, token);
        }

        /// <summary>
        /// 呼び出しているフローチャートを停止します。その際に表示されているUIはフェードアウトされます
        /// </summary>
        public void Stop()
        {
            flowchart.Stop(Flowchart.StopType.IncludeParent, isClearUI: true);
        }
    }
}