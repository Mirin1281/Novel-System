using System.IO;
using System.Linq;

namespace Novel
{
    /// <summary>
    /// ���̃X�N���v�g��Novel�t�H���_�����ɒu������
    /// Novel�t�H���_�̏ꏊ��ς�����A���O��ς��Ă����v�ł�
    /// </summary>
    public static class DirectoryPathGetter
    {
        public static string GetNovelFolderPath()
        {
            string selfFileName = $"{nameof(DirectoryPathGetter)}.cs";
            string path = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories)
                .FirstOrDefault(p => Path.GetFileName(p) == selfFileName)
                .Replace("\\", "/")
                .Replace($"/{selfFileName}", "");
            return path;
        }
    }
}