using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;

namespace Novel
{
    [CreateAssetMenu(
        fileName = "FlowchartData",
        menuName = "ScriptableObject/FlowchartData")
    ]
    public class FlowchartData : ScriptableObject, IFlowchartObject
    {
        [SerializeField] Flowchart flowchart;
        public Flowchart Flowchart => flowchart;

        public string Name => name;

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            await Flowchart.ExecuteAsync(index, callStatus);
        }

        /// <summary>
        /// カスタムエディタ用。リストの中にあるか調べます
        /// </summary>
        public bool IsUsed(CommandData targetData)
        {
            foreach(var cmd in Flowchart.GetReadOnlyCommandDataList())
            {
                if (cmd == targetData) return true;
            }
            return false;
        }
    }
}