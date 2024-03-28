using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    [CreateAssetMenu(
        fileName = "FlowchartData",
        menuName = "ScriptableObject/FlowchartData")
    ]
    public class FlowchartData : ScriptableObject
    {
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            await flowchart.ExecuteAsync(index, callStatus);
        }
    }
}