using UnityEngine;

namespace Novel
{
    [CreateAssetMenu(
        fileName = nameof(MessageBoxesData),
        menuName = ConstContainer.DATA_CREATE_PATH + "MessageBoxesData",
        order = 1)
    ]
    public class MessageBoxesData : Enum2ObjectListDataBase<BoxType, MessageBox>
    {
        [field: SerializeField, Tooltip(
                "true時はシーン切り替え時に全メッセージボックスを生成します\n" +
                "false時は受注生産方式でキャッシュします")]
        public bool CreateOnSceneChanged { get; private set; }
    }

    public enum BoxType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("下")] Type1,
        [InspectorName("上")] Type2,
    }
}