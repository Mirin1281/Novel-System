using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Novel
{
    public abstract class FadableMonoBehaviour : MonoBehaviour
    {
        protected abstract void SetAlpha(float a);
        protected abstract float GetAlpha();

        /// <summary>
        /// アクティブにしてからフェードインします
        /// </summary>
        public async UniTask ShowFadeAsync(
                float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            gameObject.SetActive(true);
            SetAlpha(0f);
            await FadeAlphaAsync(1f, time, token);
        }

        /// <summary>
        /// フェードアウトしてから非アクティブにします
        /// </summary>
        public async UniTask ClearFadeAsync(
            float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            await FadeAlphaAsync(0f, time, token);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 指定した透明度まで連続的に変化させます
        /// </summary>
        async UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
        {
            if (time == 0f)
            {
                SetAlpha(toAlpha);
                return;
            }
            var outQuad = new OutQuad(toAlpha, time, GetAlpha());
            var t = 0f;
            CancellationToken tkn = token == default ? this.GetCancellationTokenOnDestroy() : token;
            while (t < time)
            {
                SetAlpha(outQuad.Ease(t));
                t += Time.deltaTime;
                await UniTask.Yield(tkn);
            }
            SetAlpha(toAlpha);
        }
    }
}