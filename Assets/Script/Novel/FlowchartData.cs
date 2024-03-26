using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;
#pragma warning disable 0414 // value is never used の警告を消すため

namespace Novel
{
    [CreateAssetMenu(
        fileName = "FlowchartData",
        menuName = "ScriptableObject/FlowchartData")
    ]
    public class FlowchartData : ScriptableObject, IFlowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand[] commands;

        bool isStopped;

        public void Execute(int index = 0)
        {
            ExecuteAsync(index).Forget();
        }

        public async UniTask ExecuteAsync(int index = 0, bool isNest = false)
        {
            while (commands.Length > index && isStopped == false)
            {
                var cmdData = commands[index];
                if(cmdData != null)
                {
                    await cmdData.CallCommandAsync(null);
                }
                index++;
            }
            if (isStopped == false && isNest == false)
            {
                MessageBoxManager.Instance.AllFadeOutAsync(0.3f).Forget();
            }
            isStopped = false;
        }

        public void Stop()
        {
            isStopped = true;
        }
    }
}