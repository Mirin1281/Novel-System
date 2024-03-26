using UnityEngine;
using System;

[CreateAssetMenu(
    fileName = "CreateManagerData",
    menuName = "ScriptableObject/CreateManagerData")
]
public class CreateManagerData : ScriptableObject
{
    [SerializeField] CreateManagerParam[] managerParams;

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
        [field: SerializeField] public GameObject ManagerPrefab { get; private set; }
        [field: SerializeField] public bool IsInactiveOnAwake { get; private set; }
    }
}
