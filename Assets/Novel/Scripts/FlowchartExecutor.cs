using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    // FlowchartはMonoBehaviour型とScriptableObject型がある
    // MonoBehaviourはシーン内で参照が取れるためできることが多い
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効く

    public class FlowchartExecutor : MonoBehaviour, IFlowchartObject
    {
        [SerializeField] bool isStartExecute;
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        public string Name => name;

        void Start()
        {
            if (isStartExecute) ExecuteAsync().Forget();
        }

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            await Flowchart.ExecuteAsync(index, callStatus);
        }
    }

    public interface IFlowchartObject
    {
        public Flowchart Flowchart { get; }
        public UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null);
        public string Name { get; }
    }
}