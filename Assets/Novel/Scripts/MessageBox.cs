using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MessageBox : MonoBehaviour, IFadable
    {
        [SerializeField] BoxType type;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image boxImage;
        [SerializeField] Writer writer;
        [SerializeField] MsgBoxInput input;

        public BoxType Type => type;
        public Writer Writer => writer; 
        public MsgBoxInput Input => input;

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

        void SetAlpha(float a) => canvasGroup.alpha = a;

        async UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
        {
            if (time == 0f)
            {
                SetAlpha(toAlpha);
                return;
            }
            var outQuad = new OutQuad(toAlpha, time, canvasGroup.alpha);
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