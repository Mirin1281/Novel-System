using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    public interface ICommand
    {
        UniTask CallCommandAsync(IFlowchart flowchart);
        CommandStatus GetCommandStatus();
    }

    [Serializable]
    public abstract class CommandBase : ICommand
    {
        protected IFlowchart CalledFlowchart { get; private set; }

        protected abstract UniTask EnterAsync();
        async UniTask ICommand.CallCommandAsync(IFlowchart flowchart)
        {
            CalledFlowchart = flowchart;
            await EnterAsync();
        }

        CommandStatus ICommand.GetCommandStatus()
            => new CommandStatus(GetName(), GetSummary(), GetCommandColor(), GetCommandInfo());

        protected virtual string GetSummary() => string.Empty;
        protected virtual Color GetCommandColor() => Color.white;
        protected virtual string GetCommandInfo() => null;

        string GetName()
        {
            return ToString()
                .Replace("Novel.Command.", string.Empty)
                .Replace("Command", string.Empty);
        }

        protected string WarningColorText(string text = "Warning!!")
            => $"<color=#dc143c>{text}</color>";
    }
}