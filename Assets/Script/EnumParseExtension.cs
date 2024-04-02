using System;
public static class EnumParseExtension
{
    public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
        where TEnum : struct, IComparable, IFormattable, IConvertible   // コンパイル時にできるだけ制約
    {
        if (typeof(TEnum).IsEnum)
        { return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm); }
        else
        //実行時チェックでenumじゃなかった場合
        { enm = default(TEnum); return false; }
    }
}
