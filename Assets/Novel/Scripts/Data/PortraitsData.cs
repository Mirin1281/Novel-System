using UnityEngine;

namespace Novel
{
    [CreateAssetMenu(
        fileName = nameof(PortraitsData),
        menuName = ConstContainer.DATA_CREATE_PATH + "Portraits",
        order = 2)
    ]
    public class PortraitsData : Enum2ObjectListDataBase<PortraitType, Portrait>
    {
        [field: SerializeField, Tooltip(
                "true時はシーン切り替え時に全ポートレートを生成します\n" +
                "false時は受注生産方式でキャッシュします")]
        public bool CreateOnSceneChanged { get; private set; }
    }

    public enum PortraitType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("真白ノベル")] Type1,
        [InspectorName("河野修二")] Type2,
    }
}