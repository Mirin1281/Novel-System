using System;

namespace Novel
{
    public static class EnumParseExtension
    {
        /// <summary>
        /// 文字列をenumに変換します
        /// </summary>
        /// <typeparam name="TEnum">enumの型</typeparam>
        /// <param name="s">変換する文字列</param>
        /// <param name="enm">outの結果</param>
        /// <returns>変換に成功したか</returns>
        public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
            where TEnum : struct, Enum, IComparable, IFormattable, IConvertible   // コンパイル時にできるだけ制約
        {
            if (typeof(TEnum).IsEnum)
            {
                return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm);
            }
            else //実行時チェックでenumじゃなかった場合
            {
                enm = default;
                return false;
            }
        }
    }
}