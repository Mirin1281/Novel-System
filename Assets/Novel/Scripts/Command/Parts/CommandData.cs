using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [Serializable]
    public class CommandData : ScriptableObject
    {
        [field: SerializeField]
        public bool Enabled { get; private set; } = true;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        public async UniTask ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus)
        {
            if(Enabled && command != null)
            {
                await command.ExecuteAsync(flowchart, callStatus);
            }
        }

        /// <summary>
        /// CommandBaseをキャストして返します
        /// </summary>
        public CommandBase GetCommandBase()
        {
            if (command == null) return null;
            return command as CommandBase;
        }

#if UNITY_EDITOR
        public CommandStatus GetCommandStatus()
        {
            if(command == null)
            {
                return new CommandStatus("<Null>");
            }
            return command.GetCommandStatus();
        }

        /// <summary>
        /// CSV用
        /// </summary>
        public void SetCommand(Type type)
        {
            if (type == null)
            {
                command = null;
                return;
            }
            command = Activator.CreateInstance(type) as CommandBase;
        }
#endif
    }
}