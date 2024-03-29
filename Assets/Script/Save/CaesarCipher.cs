using System.Text;

/// <summary>
/// 簡単なシーザー暗号化クラス
/// </summary>
public static class CaesarCipher
{
    static string Process(string src, int delta)
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < src.Length; i++)
        {
            stringBuilder.Append((char)(src[i] + delta));
        }
        return stringBuilder.ToString();
    }

    public static string Encrypt(string src, int deltaKey)
    {
        return Process(src, deltaKey);
    }

    public static string Decrypt(string src, int deltaKey)
    {
        return Process(src, -deltaKey);
    }
}