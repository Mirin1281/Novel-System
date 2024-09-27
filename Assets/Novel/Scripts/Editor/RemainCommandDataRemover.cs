using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Novel.Editor
{
    // CommandDataは普通に削除しても空のassetファイルが残ります
    // これはUndo操作に対応するためですが、再起動などでも消えないため明示的に削除する必要があります
    //
    // このクラスではエディタの終了時とビルドの直前に掃除をします
    // またFlowchartDataの"未使用のCommandDataを削除する"でも掃除ができます
    public class RemainCommandDataRemover : IPreprocessBuildWithReport
    {
        [InitializeOnLoadMethod]
        static void StartRemoveTrash()
        {
            EditorApplication.quitting -= RemoveUnusedCommandData;
            EditorApplication.quitting += RemoveUnusedCommandData;
        }

        static void RemoveUnusedCommandData()
        {
            FlowchartEditorUtility.RemoveUnusedCommandData();
        }

        public int callbackOrder => 0;

        // Console > Clear > "Clear on Recompile" 
        // のチェックを外さないと、この中で呼び出されたログが表示されません
        public void OnPreprocessBuild(BuildReport report)
        {
            FlowchartEditorUtility.RemoveUnusedCommandData();
        }
    }
}