using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class FlowchartExecutor : MonoBehaviour, IFlowchartObject
    {
        [SerializeField] bool isStartExecute;
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        public string Name => name;

        void Start()
        {
            if (isStartExecute) Execute();
        }

        public void Execute(int index = 0)
        {
            Flowchart.ExecuteAsync(index).Forget();
        }
        public async UniTask ExecuteAsync(int index = 0)
        {
            await Flowchart.ExecuteAsync(index);
        }
    }

    public interface IFlowchartObject
    {
        string Name { get; }
        Flowchart Flowchart { get; }
        UniTask ExecuteAsync(int index = 0);
    }
}