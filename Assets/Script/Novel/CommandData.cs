using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    using CommandStatus = CommandBase.CommandStatus;

    [Serializable]
    public class CommandData : ScriptableObject
    {
        [field: SerializeField]
        public bool Enabled { get; private set; } = true;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        public async UniTask CallAsync(Flowchart flowchart, int index, FlowchartCallStatus callStatus)
        {
            if(Enabled && command != null)
            {
                await command.CallCommandAsync(flowchart, index, callStatus);
            }
        }

        #region For Editor

        public CommandBase GetCommandBase()
        {
            if (command == null) return null;
            return command as CommandBase;
        }

        public CommandStatus GetCommandStatus()
        {
            if(command == null)
            {
                return new CommandStatus(
                    "<color=#dc143c><Null></color>",
                    string.Empty,
                    new Color(0.9f, 0.9f, 0.9f, 1f),
                    null);
            }
            return command.GetCommandStatus();
        }

        #endregion
    }
}