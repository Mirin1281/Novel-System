using UnityEngine;
using System;

[CreateAssetMenu(
    fileName = "CreateManagerData",
    menuName = "ScriptableObject/CreateManagerData")
]
public class CreateManagerData : ScriptableObject
{
    [SerializeField] CreateManagerParam[] managerParams;

    // この属性によりAwakeより前に処理が走る
    // ScriptableObjectからマネージャーを生成する
    // ここでしか呼ばないほうが吉
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeAwakeInit()
    {
        var createManagerData = Resources.Load<CreateManagerData>(nameof(CreateManagerData));
        if(createManagerData == null)
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
            var initializable = obj.GetComponent<IInitializableManager>();
            initializable?.Init();
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
