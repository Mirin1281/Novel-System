using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    using CommandStatus = CommandBase.CommandStatus;

    public interface ICommand
    {
        UniTask CallCommandAsync(int index, FlowchartCallStatus callStatus);
        CommandStatus GetCommandStatus();
    }

    [Serializable]
    public abstract class CommandBase : ICommand
    {
        protected IFlowchart ParentFlowchart { get; private set; }
        protected FlowchartCallStatus CallStatus { get; private set; }
        protected int Index { get; private set; }

        protected abstract UniTask EnterAsync();
        async UniTask ICommand.CallCommandAsync(int index, FlowchartCallStatus callStatus)
        {
            CallStatus = callStatus;
            await EnterAsync();
        }

        #region For Editor

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

        CommandStatus ICommand.GetCommandStatus()
            => new CommandStatus(GetName(), GetSummary(), GetCommandColor(), GetCommandInfo());

        protected virtual string GetSummary() => string.Empty;
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);
        protected virtual string GetCommandInfo() => null;

        string GetName() => ShapeCommandName(this);

        protected string ShapeCommandName(CommandBase commandBase)
            => commandBase.ToString()
                .Replace("Novel.Command.", string.Empty)
                .Replace("Command", string.Empty);

        protected string WarningColorText(string text = "Warning!!")
            => $"<color=#dc143c>{text}</color>";

        public void SetFlowchart(IFlowchart f) => ParentFlowchart = f;
        public void SetIndex(int i) => Index = i;
    }

    #endregion
}