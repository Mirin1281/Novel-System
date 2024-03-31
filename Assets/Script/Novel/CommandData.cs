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

        public async UniTask ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus)
        {
            if(Enabled && command != null)
            {
                await command.ExecuteAsync(flowchart, callStatus);
            }
        }

        /// <summary>
        /// コマンドをキャストして返します
        /// </summary>
        public CommandBase GetCommandBase()
        {
            if (command == null) return null;
            return command as CommandBase;
        }

        /// <summary>
        /// カスタムエディタ用
        /// </summary>
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
        public void SetCommand<T>(string content1 = null, string content2 = null) where T : ICommand, new()
        {
            command = new T();
            GetCommandBase().SetCSVContent1(content1);
            GetCommandBase().SetCSVContent2(content2);
        }

        /// <summary>
        /// CSV用
        /// </summary>
        public void SetCommand(Type type)
        {
            command = Activator.CreateInstance(type) as CommandBase;
        }
    }
}