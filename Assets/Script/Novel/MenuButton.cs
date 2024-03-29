using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Threading;

namespace Novel
{
    public class MenuButton : MonoBehaviour, IFadable
    {
        [SerializeField] Image image;
        [SerializeField] TMP_Text tmpro;
        [SerializeField] Button button;
        public Button Button => button;

        public void SetText(string s)
        {
            tmpro.SetText(s);
        }

        public async UniTask ShowFadeAsync(
            float time = MyStatic.DefaultFadeTime, CancellationToken token = default)
        {
            gameObject.SetActive(true);
            SetAlpha(0f);
            await FadeAlphaAsync(1f, time, token);
        }

        public async UniTask ClearFadeAsync(
            float time = MyStatic.DefaultFadeTime, CancellationToken token = default)
        {
            await FadeAlphaAsync(0f, time, token);
            gameObject.SetActive(false);
        }

        void SetAlpha(float a)
        {
            image.SetAlpha(a);
        }

        async UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
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
                await MyStatic.Yield(token == default ? destroyCancellationToken : token);
            }
            SetAlpha(toAlpha);
        }
    }
}