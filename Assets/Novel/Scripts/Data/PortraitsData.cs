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
        [Header("true : シーン切り替え時に全ポートレートを都度生成します\n" +
                "false : 受注生産方式でキャッシュします")]
        [SerializeField, Space(10)] bool createOnSceneChanged;
        public bool CreateOnSceneChanged => createOnSceneChanged;
    }

    public enum PortraitType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("真白ノベル")] Type1,
        [InspectorName("河野修二")] Type2,
    }
}