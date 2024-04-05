using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Label"), System.Serializable]
    public class LabelCommand : CommandBase
    {
        [SerializeField] string labelName;

        protected override async UniTask EnterAsync()
        {
            await UniTask.CompletedTask;
            return;
        }

        protected override string GetSummary()
        {
            return labelName;
        }

        protected override string GetCommandInfo() => "デバッグ用です。このコマンドは何も起こりません";

        public override string CSVContent1
        {
            get => labelName;
            set => labelName = value;
        }
    }
}