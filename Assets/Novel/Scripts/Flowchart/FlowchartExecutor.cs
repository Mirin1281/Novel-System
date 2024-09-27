using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class FlowchartExecutor : MonoBehaviour, IFlowchartObject
    {
        [SerializeField] bool executeOnStart;
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;
        public string Name => name;

        void Start()
        {
            if (executeOnStart)
                Execute();
        }

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">コマンドの初期インデックス</param>
        /// <param name="token">キャンセル用のトークン(通常はStop()があるので不要)</param>
        public void Execute(int index = 0, CancellationToken token = default)
        {
            ExecuteAsync(index, token).Forget();
        }
        public UniTask ExecuteAsync(int index = 0, CancellationToken token = default)
        {
            if(token == default)
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
            flowchart.Stop(Flowchart.StopType.All, isClearUI: true);
        }
    }
}