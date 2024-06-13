using UnityEngine;
using Cysharp.Threading.Tasks;

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
            if (executeOnStart) Execute();
        }

        public void Execute(int index = 0)
        {
            Flowchart.ExecuteAsync(index).Forget();
        }
        public UniTask ExecuteAsync(int index = 0)
        {
            return Flowchart.ExecuteAsync(index);
        }
    }
}