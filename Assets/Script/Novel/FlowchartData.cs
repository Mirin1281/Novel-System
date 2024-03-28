using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    using Novel.Command;

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

        /// <summary>
        /// カスタムエディタ用。リストの中にあるか調べます
        /// </summary>
        public bool IsUsed(CommandData targetData)
        {
            foreach(var cmd in flowchart.GetCommandDataList())
            {
                if (cmd == targetData) return true;
            }
            return false;
        }
    }
}