namespace Novel.Command
{
    /// <summary>
    /// このインターフェースがついたコマンドは、それよりも下で発火された際にCallZone()が呼び出されます
    /// </summary>
    public interface IZoneCommand
    {
        void CallZone();
    }

    /* Zoneコマンドについて
    これは主にコマンド中にセーブ、ロードをする際に利用することを想定した機能です
    IZoneCommandインターフェイスを継承するコマンドは、「そのコマンドを通ったら発火する」に加え、
    「そのコマンドより下からExecuteした場合を検知する」という振るまいが追加されます

    具体的な使用例として、BGMの変更が挙げられます
    従来のコマンドだと、ロードなどにより、BGMを再生するコマンドより下からExecuteされた場合に、
    BGMの再生がされない状態ですが、Zoneコマンドを使えばそれを検知でき、簡単に正しい挙動を実現できます

    なお、コマンドのクラス名は○○ZoneCommandを推奨します
    Flowchart.isCheckZoneをfalseにすることで、Zoneコマンドを使用しない場合は処理をカットできます
    */
}