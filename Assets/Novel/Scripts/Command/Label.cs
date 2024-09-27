using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(Label)), System.Serializable]
    public class Label : CommandBase
    {
        [SerializeField] string labelName;
        [SerializeField] Color commandColor = Color.white;

        protected override async UniTask EnterAsync()
        {
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            return labelName;
        }

        protected override Color GetCommandColor()
        {
            return commandColor;
        }

        public override string CSVContent1
        {
            get => labelName;
            set => labelName = value;
        }
    }
}