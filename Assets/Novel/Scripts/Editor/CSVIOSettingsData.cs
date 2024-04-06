using UnityEngine;

namespace Novel.Editor
{
    [CreateAssetMenu(
        fileName = "CSVIOSettings",
        menuName = "ScriptableObject/CSVIOSettings")
    ]
    public class CSVIOSettingsData : ScriptableObject
    {
        [field: Header("CSVファイルの出力先パス"), SerializeField]
        public string CSVFolderPath { get; private set; } = "Assets";

        [field: Header("デフォルトのファイル名\n\"(シーン名)_(このファイル名)\"という名前で出力されます"), SerializeField]
        public string ExportFileName { get; private set; } = "FlowchartSheet";

        [field: Header("書き出し時、非アクティブのゲームオブジェクトを含むか"), SerializeField]
        public FindObjectsInactive FlowchartFindMode { get; private set; } = FindObjectsInactive.Include;

        [field: Header("書き出し時、1つのフローチャートに対してとる列の数"), SerializeField]
        public int RowCountPerOne { get; private set; } = 3;

        [field: Header("読み込み時、CSVとフローチャートでコマンド名が異なる際に書き換える"), SerializeField]
        public bool IsChangeIfDifferentCmdName { get; private set; } = true;
    }
}