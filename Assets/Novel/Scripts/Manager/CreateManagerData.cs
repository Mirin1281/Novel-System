using UnityEngine;
using System;

namespace Novel
{
    [CreateAssetMenu(
        fileName = "CreateManagerData",
        menuName = "ScriptableObject/CreateManagerData",
        order = 1)
    ]
    public class CreateManagerData : ScriptableObject
    {
        [SerializeField] CreateManagerParam[] managerParams;

        // この属性によりAwakeより前に処理が走る(ここでしか呼ばないほうが吉)
        // ScriptableObjectからマネージャーを生成する
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeAwakeInit()
        {
            var createManagerData = Resources.Load<CreateManagerData>(nameof(CreateManagerData));
            if (createManagerData == null)
            {
                Debug.LogWarning($"{nameof(CreateManagerData)}がありません！");
                return;
            }
            createManagerData.InitCreate();
        }

        public void InitCreate()
        {
            foreach (var param in managerParams)
            {
                var obj = Instantiate(param.ManagerPrefab);
                obj.name = param.ManagerPrefab.name;
                DontDestroyOnLoad(obj);
                if (param.IsInactiveOnAwake)
                {
                    obj.SetActive(false);
                }
            }
        }

        [Serializable]
        class CreateManagerParam
        {
            [field: SerializeField]
            public GameObject ManagerPrefab { get; private set; }

            [field: SerializeField, Tooltip("生成時に非アクティブにします")]
            public bool IsInactiveOnAwake { get; private set; }
        }
    }
}