using UnityEngine;

namespace Novel
{
    [CreateAssetMenu(
        fileName = nameof(MessageBoxesData),
        menuName = ConstContainer.DATA_CREATE_PATH + "MessageBoxes",
        order = 2)
    ]
    public class MessageBoxesData : Enum2ObjectListDataBase<BoxType, MessageBox>
    {
        [Header("true : シーン切り替え時に全メッセージボックスを都度生成します\n" +
                "false : 受注生産方式でキャッシュします")]
        [SerializeField, Space(10)] bool createOnSceneChanged;
        public bool CreateOnSceneChanged => createOnSceneChanged;
    }

    public enum BoxType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("下")] Type1,
        [InspectorName("上")] Type2,
    }
}