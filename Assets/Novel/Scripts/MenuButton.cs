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
            float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            gameObject.SetActive(true);
            image.SetAlpha(0f);
            await FadeAlphaAsync(1f, time, token);
        }

        public async UniTask ClearFadeAsync(
            float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            await FadeAlphaAsync(0f, time, token);
            gameObject.SetActive(false);
        }

        async UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
        {
            if (time == 0f)
            {
                image.SetAlpha(toAlpha);
                return;
            }
            var outQuad = new OutQuad(toAlpha, time, image.color.a);
            var t = 0f;
            while (t < time)
            {
                image.SetAlpha(outQuad.Ease(t));
                t += Time.deltaTime;
                await Wait.Yield(token == default ? this.GetCancellationTokenOnDestroy() : token);
            }
            image.SetAlpha(toAlpha);
        }
    }
}