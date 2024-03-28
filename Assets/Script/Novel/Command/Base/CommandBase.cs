using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    using CommandStatus = CommandBase.CommandStatus;

    public interface ICommand
    {
        UniTask ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus);
        CommandStatus GetCommandStatus();
    }

    [Serializable]
    public abstract class CommandBase : ICommand
    {
        protected Flowchart ParentFlowchart { get; private set; }
        protected FlowchartCallStatus CallStatus { get; private set; }
        protected int Index { get; private set; }

        protected abstract UniTask EnterAsync();
        async UniTask ICommand.ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus)
        {
            ParentFlowchart = flowchart;
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
            => new　(GetName(this), GetSummary(), GetCommandColor(), GetCommandInfo());

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        protected virtual string GetSummary() => string.Empty;

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// 説明を記載します
        /// </summary>
        protected virtual string GetCommandInfo() => null;

        protected string GetName(CommandBase commandBase)
            => commandBase.ToString()
                .Replace("Novel.Command.", string.Empty)
                .Replace("Command", string.Empty);

        /// <summary>
        /// 警告文を色付きで返します
        /// </summary>
        protected string WarningColorText(string text = "Warning!!")
            => $"<color=#dc143c>{text}</color>";

#if UNITY_EDITOR
        public void SetFlowchart(Flowchart f) => ParentFlowchart = f;
        public void SetIndex(int i) => Index = i;
#endif
    }

    #endregion
}