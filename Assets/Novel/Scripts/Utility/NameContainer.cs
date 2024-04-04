using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Novel
{
    /// <summary>
    /// íËêîÇ≈ñºëOÇï€ä«Ç∑ÇÈ
    /// </summary>
    public static class NameContainer
    {
        public static readonly string NOVEL_PATH = PathGetter.GetNovelFolderPath();
        public static readonly string COMMANDDATA_PATH = NOVEL_PATH + "/Scriptable/Commands";
        public static readonly string CHARACTER_PATH = NOVEL_PATH + "/Scriptable/Characters";

        public static readonly string SUBMIT_KEYNAME = "Submit";
        public static readonly string CANCEL_KEYNAME = "Cancel";
    }
}