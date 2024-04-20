using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Sample"), System.Serializable]
    public class SampleCommand : CommandBase
    {
        [SerializeField, DropDownCharacter, Space(10)]
        CharacterData character;

        [SerializeField]
        string summaryText;

        [SerializeField]
        string infoText;

        [SerializeField]
        Color color;

        protected override async UniTask EnterAsync()
        {
            Debug.Log($"キャラクターの名前: {character.CharacterName}");
            await UniTask.CompletedTask;
        }


        #region For EditorWindow

        protected override string GetSummary() => summaryText;

        protected override Color GetCommandColor() => color;

        protected override string GetCommandInfo() => infoText;

        public override string CSVContent1
        {
            get => character == null ? null : character.CharacterName;
            set
            {
                var chara = CharacterData.GetCharacter(value);
                if (chara == null) return;
                character = chara;
            }
        }

        public override string CSVContent2
        {
            get => summaryText;
            set => summaryText = value;
        }

        #endregion
    }
}