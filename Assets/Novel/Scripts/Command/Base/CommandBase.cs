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
        [field: SerializeField, HideInInspector]
        protected Flowchart ParentFlowchart { get; private set; }

        [field: SerializeField, HideInInspector]
        protected int Index { get; private set; }

        protected FlowchartCallStatus CallStatus { get; private set; }
        

        protected abstract UniTask EnterAsync();
        async UniTask ICommand.ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus)
        {
            ParentFlowchart = flowchart;
            CallStatus = callStatus;
            await EnterAsync();
        }

        #region For Editor

        /// <summary>
        /// 名前、色、説明、状態を表す
        /// </summary>
        public readonly struct CommandStatus
        {
            readonly public string Name;
            readonly public string Summary;
            readonly public Color Color;
            readonly public string Info;

            public CommandStatus(
                string name, string summary = null, Color color = default, string info = null)
            {
                Name = name;
                Summary = summary ?? string.Empty;
                Color = color == default ? new Color(0.8f, 0.8f, 0.8f, 1f) : color;
                Info = info ?? string.Empty;
            }
        }

        CommandStatus ICommand.GetCommandStatus()
            => new(
                GetName(),
                GetSummary(),
                GetCommandColor(),
                GetCommandInfo());

        /// <summary>
        /// コマンド名を定義します
        /// </summary>
        protected virtual string GetName() => GetName(this);

        /// <summary>
        /// エディタのコマンドに状態を記述します(オーバーライド)
        /// </summary>
        protected virtual string GetSummary() => null;

        /// <summary>
        /// コマンドの色を設定します(オーバーライド)
        /// </summary>
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// 説明を記載します(オーバーライド)
        /// </summary>
        protected virtual string GetCommandInfo() => null;

        /// <summary>
        /// コマンドのクラス名を取得します
        /// </summary>
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
        /// <summary>
        /// CSV出力時の第一表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent1 { get; set; } = string.Empty;

        /// <summary>
        /// CSV出力時の第二表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent2 { get; set; } = string.Empty;

        public void SetFlowchart(Flowchart f) => ParentFlowchart = f;
        public void SetIndex(int i) => Index = i;
#endif
    }

    #endregion
}