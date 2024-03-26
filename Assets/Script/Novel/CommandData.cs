using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [Serializable]
    public class CommandData : ScriptableObject
    {
        [SerializeField] bool enabled = true;
        public bool Enabled => enabled;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        public async UniTask CallAsync(IFlowchart flowchart)
        {
            if(enabled && command != null)
            {
                await command.CallCommandAsync(flowchart);
            }
        }

        public CommandStatus GetCommandStatus()
        {
            if(command == null)
            {
                return new CommandStatus("<Null>", string.Empty, Color.white);
            }
            return command.GetCommandStatus();
        }
    }

    public readonly struct CommandStatus
    {
        readonly public string Name;
        readonly public string Summary;
        readonly public Color Color;
        readonly public string Info;

        public CommandStatus(
            string name, string summary, Color color, string info = null)
        {
            Name = name;
            Summary = summary;
            Color = color;
            Info = info;
        }
    }
}