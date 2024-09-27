using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel;
using System.Collections.Generic;

using System.IO;


#if UNITY_EDITOR
using UnityEditor;
using Novel.Editor;
#endif

public class Test : MonoBehaviour
{
    [SerializeField] MessageBoxManager msgBoxManagerPrefab;
    [SerializeField] FlowchartExecutor flowchartExecutor;
    [SerializeField] FlagKey_Int intFlag;
    [SerializeField] FlagKey_String stringFlag;
    [SerializeField] SaveLoadButtonGroup saveButtonGroup;
    [SerializeField] SceneChanger sceneChanger;
    [SerializeField]
    int[] ints = new int[] { 1, 0 };

#if UNITY_EDITOR
    [ContextMenu("DestroyObj")]
    void DestroyObj()
    {
        Undo.RegisterCompleteObjectUndo(intFlag, "Destroy Obj");
        //DestroyImmediate(intFlag, true);
        DestroyScritableObject(intFlag);
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        //Undo.DestroyObjectImmediate(intFlag);
    }

    static void DestroyScritableObject(ScriptableObject obj)
    {
        var path = FlowchartEditorUtility.GetExistFolderPath(obj);
        var deleteName = obj.name;
        Object.DestroyImmediate(obj, true);
        AssetDatabase.MoveAssetToTrash($"{path}/{deleteName}.asset");
        AssetDatabase.MoveAssetToTrash($"{path}/{deleteName}.asset.meta");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
#endif

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
