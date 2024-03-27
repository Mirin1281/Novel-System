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

        public async UniTask CallAsync(IFlowchart flowchart)
        {
            if(Enabled && command != null)
            {
                await command.CallCommandAsync(flowchart);
            }
        }

        public CommandStatus GetCommandStatus()
        {
            if(command == null)
            {
                return new CommandStatus("<color=#dc143c><Null></color>", string.Empty, Color.white, null);
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
            string name, string summary, Color color, string info)
        {
            Name = name;
            Summary = summary;
            Color = color;
            Info = info;
        }
    }
}