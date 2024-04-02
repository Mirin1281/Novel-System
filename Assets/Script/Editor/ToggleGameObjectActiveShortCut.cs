using UnityEngine;
using UnityEditor;
using System.Reflection;

// https://qiita.com/r-ngtm/items/ca5d4106e8c559275862
[InitializeOnLoad]
public class ToggleGameObjectActiveShortCut
{
    static ToggleGameObjectActiveShortCut()
    {
        bool keyDown = false;
        EditorApplication.CallbackFunction function = () =>
        {
            if (!keyDown && Event.current.type == EventType.KeyDown)
            {
                keyDown = true;

                // "," が入力されたらHierarchyで選択しているオブジェクトのアクティブ状態を反転させる
                if (Event.current.keyCode == KeyCode.Comma && Selection.activeGameObject != null)
                {
                    foreach (var go in Selection.gameObjects)
                    {
                        Undo.RecordObject(go, go.name + ".activeSelf");
                        go.SetActive(!go.activeSelf);
                    }
                }
            }

            if (keyDown && Event.current.type == EventType.KeyUp)
            {
                keyDown = false;
            }
        };

        FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
        functions += function;
        info.SetValue(null, functions);
    }
}
