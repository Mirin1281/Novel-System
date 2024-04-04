using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// このスクリプトはNovelフォルダ直下に置くこと
/// Novelフォルダの場所を変えたり、名前を変えても大丈夫です
/// </summary>
public static class PathGetter
{
    public static string GetNovelFolderPath()
    {
        string selfFileName = $"{nameof(PathGetter)}.cs";
        string path = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories)
            .FirstOrDefault(p => Path.GetFileName(p) == selfFileName)
            .Replace("\\", "/")
            .Replace($"/{selfFileName}", "");
        return path;
    }
}
