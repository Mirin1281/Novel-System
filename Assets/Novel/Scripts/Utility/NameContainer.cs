using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Novel
{
    /// <summary>
    /// 定数で名前を保管する
    /// </summary>
    public static class NameContainer
    {
        public static readonly string NOVEL_PATH = GetNovelPath();
        public static readonly string COMMANDDATA_PATH = NOVEL_PATH + "/Scriptable/Commands";
        public static readonly string CHARACTER_PATH = NOVEL_PATH + "/Scriptable/Characters";

        public static readonly string SUBMIT_KEYNAME = "Submit";
        public static readonly string CANCEL_KEYNAME = "Cancel";

        static string GetNovelPath()
        {
            string selfFileName = $"{nameof(NameContainer)}.cs";
            string selfPath = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories)
                .FirstOrDefault(p => Path.GetFileName(p) == selfFileName)
                .Replace("\\", "/")
                .Replace($"/selfFileName", "");
                
            string[] directories = selfPath.Split('/');
            // このスクリプトの場所は不変としてNovelフォルダのパスを求めている
            // Novelフォルダの場所を変えたり、名前を変えても大丈夫です
            int depth = directories.Length - 3;
            string path = null;
            for(int i = 0; i < depth; i++)
            {
                path += directories[i];
                if (i == depth - 1) break;
                path += "/";
            }
            return path;
        }
    }
}