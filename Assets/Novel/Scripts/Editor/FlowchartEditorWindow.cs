using UnityEngine;
using UnityEditor;

namespace Novel.Editor
{
    public class FlowchartEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Novel System/Open Flowchart Window")]
        public static void OpenWindow()
        {
            GetWindow<FlowchartEditorWindow>("Flowchart Editor", typeof(SceneView));
        }


        [SerializeField] FlowchartEditorHelper helper;

        void OnEnable()
        {
            helper ??= new(this);
            helper.Init();
        }

        void OnDestroy()
        {
            helper.SetEvents(false);
            helper = null;
        }

        // 選択しているオブジェクトが変更された時に呼ばれる 
        void OnSelectionChange()
        {
            helper.UpdateFlowchartObjectAndWindow();
        }

        void OnGUI()
        {
            helper.OnGUI();
        }
    }
}