using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Sample"), System.Serializable]
    public class Sample : CommandBase
    {
        [SerializeField, DropDownCharacter(nameof(character)), Space(10)]
        CharacterData character;

        [SerializeField]
        string summaryText = "Summary";

        [SerializeField]
        string infoText = "Info";

        [SerializeField]
        Color color = Color.white;

        // フローチャート上で呼ばれた際に呼ばれます
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
            get => character == null ? "Null" : character.CharacterName;
            set
            {
                if (value == "Null")
                {
                    character = null;
                    return;
                }
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