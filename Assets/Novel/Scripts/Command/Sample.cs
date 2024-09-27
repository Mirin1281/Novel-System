using UnityEngine;
using Cysharp.Threading.Tasks;

// 自作したり、カスタマイズしたりする際はこちらを参照してください
// このコマンドはエディタ拡張を使用していません

// ◆コマンドを非表示にしたい
// 　いくつか(Sample)～～というコマンド名のものがありますが、これらはサンプルなので非表示にすることを推奨します
// 　このスクリプトでは26行目の[AddTypeMenu("(Sample)")～　をコメントアウトするのが安全です
// 　どうしても削除したい場合は、プロジェクト全体で使用箇所が無いことを確かめてから削除してください
//
// ◆名前を変更したい
// 　AddTypeMenu()の引数はインスペクタ上の表示名を設定できます。そのまま書き換えて構いません
//   クラス名本体を変更したい場合、25行目のコメントアウトを外してからクラス名を変更した後にコンパイルしてください
//
// ◆DropDownCharacter属性について
// 　下部にCharacterDataのフィールドを設定できますが、その際に[DropDownCharacter]を用いることで
// 　ドロップダウン方式でCharacterDataを選択することができるため便利です。ご活用ください
// 　(なおCharacterDataはScriptableObjectです)

// このコマンドは説明のために冗長な書き方をしているので、より単純なものはWaitコマンドなどを参照してください

namespace Novel.Command
{
    //[UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "(Sample)")]
    [AddTypeMenu("(Sample)"), System.Serializable]
    public class Sample : CommandBase
    {
        #region Field

        [Header("左側のリストからこのコマンドを選択した状態で右クリックし\n\"Edit Script\"からスクリプトを閲覧することができます")]

        [SerializeField, DropDownCharacter, Space(10)]
        CharacterData character;

        [SerializeField]
        string summaryText = "Summary";

        [SerializeField]
        Color color = Color.white;

        #endregion


        // フローチャート上でコマンドが通過した際に実際に呼ばれます
        protected override async UniTask EnterAsync()
        {
            if(character != null)
            {
                Debug.Log($"キャラクターの名前: {character.CharacterName}");
            }
            await UniTask.CompletedTask;
        }


        #region For EditorWindow

        // サマリー(コマンドリスト上の情報)を表示
        protected override string GetSummary() => summaryText;

        // コマンドリスト上の色を定義
        protected override Color GetCommandColor() => color;

        // CSV出力をした際の情報(1)の入出力
        // getが表示するテキストです
        // setは受け取ったテキストからフィールドへ変換します
        public override string CSVContent1
        {
            get => character == null ? string.Empty : character.CharacterName;
            set
            {
                if (value == string.Empty)
                {
                    character = null;
                    return;
                }
                var chara = CharacterData.GetCharacter(value);
                if (chara == null) return;
                character = chara;
            }
        }

        // CSV出力をした際の情報(2)の入出力
        public override string CSVContent2
        {
            get => summaryText;
            set => summaryText = value;
        }

        #endregion
    }
}