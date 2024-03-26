using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class MenuButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text tmpro;
    [SerializeField] Button button;
    public Button Button => button;

    public void SetText(string s)
    {
        tmpro.SetText(s);
    }

    public async UniTask ShowFadeAsync(float time = MyStatic.DefaultFadeTime)
    {
        gameObject.SetActive(true);
        SetAlpha(0f);
        await FadeAlphaAsync(1f, time);
    }

    public async UniTask FadeOutAsync(float time = MyStatic.DefaultFadeTime)
    {
        await FadeAlphaAsync(0f, time);
        gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        var currentColor = image.color;
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, a);
    }

    async UniTask FadeAlphaAsync(float toAlpha, float time)
    {
        if (time == 0f)
        {
            SetAlpha(toAlpha);
            return;
        }
        var outQuad = new OutQuad(toAlpha, time, image.color.a);
        var t = 0f;
        while (t < time)
        {
            SetAlpha(outQuad.Ease(t));
            t += Time.deltaTime;
            await MyStatic.Yield();
        }
        SetAlpha(toAlpha);
    }
}
