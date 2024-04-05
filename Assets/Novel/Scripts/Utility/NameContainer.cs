
namespace Novel
{
    /// <summary>
    /// ’è”‚Å–¼‘O‚ğ•ÛŠÇ‚·‚é
    /// </summary>
    public static class NameContainer
    {
        public static readonly string NOVEL_PATH = PathGetter.GetNovelFolderPath();
        public static readonly string COMMANDDATA_PATH = NOVEL_PATH + "/Scriptable/Commands";

        public static readonly string SUBMIT_KEYNAME = "Submit";
        public static readonly string CANCEL_KEYNAME = "Cancel";
    }
}