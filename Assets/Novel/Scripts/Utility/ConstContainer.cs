
namespace Novel
{
    /// <summary>
    /// íËêîÇ≈ñºëOÇï€ä«Ç∑ÇÈ
    /// </summary>
    public static class ConstContainer
    {
        public static readonly string NOVEL_PATH = DirectoryPathGetter.GetNovelFolderPath();
        public static readonly string COMMANDDATA_PATH = NOVEL_PATH + "/Scriptables/Commands";

        public const string SUBMIT_KEYNAME = "Submit";
        public const string CANCEL_KEYNAME = "Cancel";

        public const string FLAGKEY_CREATE_PATH = "Novel-FlagKey/";
        public const string DATA_CREATE_PATH = "Novel-Data/";

        public const float DefaultFadeTime = 0.3f;
    }
}