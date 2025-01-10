using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel.Command
{
    public interface ICommand
    {
        UniTask ExecuteAsync(Flowchart parentFlowchart);
        string GetName();
        string GetSummary();
        Color GetCommandColor();
    }

    [Serializable]
    public abstract class CommandBase : ICommand
    {
        [field: SerializeField, HideInInspector]
        protected Flowchart ParentFlowchart { get; private set; }

        [field: SerializeField, HideInInspector]
        protected int Index { get; private set; }

        protected CancellationToken Token => ParentFlowchart.CallStatus.Token;


        protected abstract UniTask EnterAsync();
        UniTask ICommand.ExecuteAsync(Flowchart parentFlowchart)
        {
            ParentFlowchart = parentFlowchart;
            return EnterAsync();
        }

        string ICommand.GetSummary() => GetSummary();
        Color ICommand.GetCommandColor() => GetCommandColor();
        string ICommand.GetName() => GetName(this);


        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        protected string GetName(CommandBase commandBase)
        {
            var tmpArray = commandBase.ToString().Split('.');
            return tmpArray[tmpArray.Length - 1];
        }

        /// <summary>
        /// 警告文を色付きで返します(デフォルトは"Warning!!")
        /// </summary>
        protected string WarningText(string text = "Warning!!")
            => $"<color=#dc143c>{text}</color>";


        #region ///// Overrides 継承するコマンドがオーバーライドして使います /////

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        protected virtual string GetSummary() => null;

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// CSV出力時の第一表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent1 { get; set; } = string.Empty;

        /// <summary>
        /// CSV出力時の第二表示(getは書き出し、setは読み込み)
        /// </summary>
        public virtual string CSVContent2 { get; set; } = string.Empty;

        #endregion

#if UNITY_EDITOR
        public void SetFlowchart(Flowchart f) => ParentFlowchart = f;
        public void SetIndex(int i) => Index = i;
#endif
    }
}