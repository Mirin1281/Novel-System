using TMPro;

public static class RubyTmproExtension
{
    public static void SetUneditedText(this RubyTextMeshProUGUI self, string text)
    {
        self.uneditedText = text;
    }
}
