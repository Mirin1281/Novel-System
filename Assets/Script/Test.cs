using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    [SerializeField] MessageBoxManager msgBoxManagerPrefab;
    [SerializeField] FlowchartExecutor flowchartExecutor;
    [SerializeField] FlagKey_Int intFlag;
    [SerializeField] FlagKey_String stringFlag;
    [SerializeField] SaveLoadButtonGroup saveButtonGroup;
    [SerializeField] SceneChanger sceneChanger;

    async UniTask Start()
    {
        
        //sceneChanger = sceneChanger.FindComponent();
        //sceneChanger.LoadScene();
        //await saveButtonGroup.ShowAndWaitButtonClick(SaveLoadType.Save, default);

        /*await Wait(1f);
        Destroy(MessageBoxManager.Instance.gameObject);
        await Wait(1f);
        DontDestroyOnLoad(Instantiate(msgBoxManagerPrefab));
        await Wait(1f);
        flowchartExecutor.ExecuteAsync().Forget();*/
        //FlagManager.SetFlagValue(intFlag, 256);
        //FlagManager.SetFlagValue(stringFlag, "‚¨‚ñ‚¨‚ñ");
        await UniTask.CompletedTask;
    }

}
