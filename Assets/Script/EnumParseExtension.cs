using System;
public static class EnumParseExtension
{
    public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
        where TEnum : struct, IComparable, IFormattable, IConvertible   // �R���p�C�����ɂł��邾������
    {
        if (typeof(TEnum).IsEnum)
        { return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm); }
        else
        //���s���`�F�b�N��enum����Ȃ������ꍇ
        { enm = default(TEnum); return false; }
    }
}
