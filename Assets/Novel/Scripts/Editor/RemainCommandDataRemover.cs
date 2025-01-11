using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Novel.Editor
{
    // まれに不要となったCommandDataがフォルダ内に残ることがあります
    // このクラスでは"エディタの終了時"と"ビルドの直前"にそのコマンドを削除します
    // 上部メニューのTools/Novel System/Clear Trash Commandsから手動で行うこともできます
    public class RemainCommandDataRemover : IPreprocessBuildWithReport
    {
        [InitializeOnLoadMethod]
        static void StartRemoveTrash()
        {
            EditorApplication.quitting -= RemoveUnusedCommandData;
            EditorApplication.quitting += RemoveUnusedCommandData;
        }

        [MenuItem("Tools/Novel System/Clear Trash Commands")]
        static void RemoveUnusedCommandData()
        {
            FlowchartEditorUtility.DestroyAllUnusedCommandData();
        }

        public int callbackOrder => 0;

        // Console > Clear > "Clear on Recompile" 
        // のチェックを外さないと、この中で呼び出されたログが表示されません
        public void OnPreprocessBuild(BuildReport report)
        {
            RemoveUnusedCommandData();
        }
    }
}