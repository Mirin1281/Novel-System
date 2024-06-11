
namespace Novel
{
    /// <summary>
    /// ’è”‚Å–¼‘O‚ğ•ÛŠÇ‚·‚é
    /// </summary>
    public static class ConstContainer
    {
        public static readonly string NOVEL_PATH = DirectoryPathGetter.GetNovelFolderPath();
        public static readonly string COMMANDDATA_PATH = NOVEL_PATH + "/Scriptables/Commands";

        public const string SUBMIT_KEYNAME = "Submit";
        public const string CANCEL_KEYNAME = "Cancel";

        public const float DefaultFadeTime = 0.3f;
    }
}