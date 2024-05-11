using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
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

        CommandStatus ICommand.GetCommandStatus()
            => new(GetName(this), GetSummary(), GetCommandColor(), GetCommandInfo());

        #region Overrides

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        protected virtual string GetSummary() => null;

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// 説明を記載します
        /// </summary>
        protected virtual string GetCommandInfo() => null;

        
        /// <summary>
        /// CSV出力時の第一表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent1 { get; set; } = string.Empty;

        /// <summary>
        /// CSV出力時の第二表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent2 { get; set; } = string.Empty;

        #endregion

        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        protected string GetName(CommandBase commandBase)
            => commandBase.ToString()
                .Replace($"{nameof(Novel)}.{nameof(Command)}.", string.Empty)
                .Replace(nameof(Command), string.Empty);

        /// <summary>
        /// 警告文を色付きで返します(デフォルトは"Warning!!")
        /// </summary>
        protected string WarningText(string text = "Warning!!")
            => $"<color=#dc143c>{text}</color>";

#if UNITY_EDITOR
        public void SetFlowchart(Flowchart f) => ParentFlowchart = f;
        public void SetIndex(int i) => Index = i;
#endif
    }
}