using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Label"), System.Serializable]
    public class Label : CommandBase
    {
        [SerializeField] string labelName;
        [ColorUsage(true, true), SerializeField] Color commandColor = Color.white;

        protected override async UniTask EnterAsync()
        {
            await UniTask.CompletedTask;
            return;
        }

        protected override string GetSummary()
        {
            return labelName;
        }

        protected override Color GetCommandColor()
        {
            return commandColor;
        }

        protected override string GetCommandInfo() => "�f�o�b�O�p�ł��B���̃R�}���h�͉����N����܂���";

        public override string CSVContent1
        {
            get => labelName;
            set => labelName = value;
        }
    }
}