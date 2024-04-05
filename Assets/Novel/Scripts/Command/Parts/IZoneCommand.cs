namespace Novel.Command
{
    /// <summary>
    /// このインターフェースがついたコマンドは、それよりも下で発火された際にCallZone()が呼び出されます
    /// </summary>
    public interface IZoneCommand
    {
        void CallZone();
    }
}