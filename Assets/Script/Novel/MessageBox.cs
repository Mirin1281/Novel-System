using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class MessageBox : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image boxImage;
        [SerializeField] Writer writer;
        [SerializeField] MsgBoxInput input;
        [SerializeField] AudioClip nextSE;

        public Writer Writer => writer; 
        public MsgBoxInput Input => input;

        public async UniTask ShowAsync(float time = MyStatic.DefaultFadeTime)
        {
            if (gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
                SetAlpha(0f);
                await FadeAlphaAsync(1f, time);
            }
        }        
        
        void SetAlpha(float a) => canvasGroup.alpha = a;

        async UniTask FadeAlphaAsync(float toAlpha, float time)
        {
            var outQuad = new OutQuad(toAlpha, time, canvasGroup.alpha);
            var t = 0f;
            while (t < time)
            {
                SetAlpha(outQuad.Ease(t));
                t += Time.deltaTime;
                await MyStatic.Yield();
            }
            SetAlpha(toAlpha);
        }

        public async UniTask FadeOutAsync(float time = MyStatic.DefaultFadeTime)
        {
            if(time == 0f)
            {
                SetAlpha(0f);
                gameObject.SetActive(false);
                return;
            }
            await FadeAlphaAsync(0f, time);
            gameObject.SetActive(false);
        }
    }
}