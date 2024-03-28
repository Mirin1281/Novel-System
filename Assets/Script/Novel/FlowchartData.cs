using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;
using System.Threading;
using System.Collections.Generic;
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

        IEnumerable<CommandBase> IFlowchart.GetCommandBaseList()
        {
            var cmds = new CommandBase[commands.Length];
            for(int i = 0; i < commands.Length; i++)
            {
                cmds[i] = commands[i] as CommandBase;
            }
            return cmds;
        }

        bool isStopped;
        CancellationTokenSource cts;

        public async UniTask ExecuteAsync(int index = 0, FlowchartCallStatus callStatus = null)
        {
            var status = SetStatus(callStatus);

            while (commands.Length > index && isStopped == false)
            {
                var cmdData = commands[index];
                if(cmdData != null)
                {
                    await cmdData.CallCommandAsync(this, index, status);
                }
                index++;
            }
            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;
        }

        /// <summary>
        /// FlowchartCallStatusをctsに反映します。
        /// statusがnullだった場合は初期化をします
        /// </summary>
        FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus)
        {
            if (callStatus == null)
            {
                cts = new CancellationTokenSource();
                return new FlowchartCallStatus(cts.Token, cts, false);
            }
            else
            {
                cts = callStatus.Cts;
                return callStatus;
            }
        }

        void IFlowchart.Stop(FlowchartStopType stopType)
        {
            if (stopType == FlowchartStopType.IncludeParent)
            {
                cts.Cancel();
            }
            else if (stopType == FlowchartStopType.Single)
            {
                isStopped = true;
            }
        }
    }
}