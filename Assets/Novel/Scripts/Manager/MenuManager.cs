using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MenuManager : SingletonMonoBehaviour<MenuManager>
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] MenuButtonCreator buttonCreator;
        [SerializeField] AudioClip selectSE;
        [SerializeField] float seVolume = 1f;

        /// <summary>
        /// ボタングループを表示して押されるまで待機します
        /// </summary>
        /// <param name="token">キャンセル用のトークン</param>
        /// <param name="texts">ボタンに表示するテキスト</param>
        /// <returns>押されたボタンのインデックス</returns>
        public async UniTask<int> ShowAndWaitButtonClick(CancellationToken token, params string[] texts)
        {
            gameObject.SetActive(true);
            CancellationTokenSource cts = new();
            CancellationToken linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token).Token;
            var buttons = buttonCreator.CreateShowButtons(texts);
            var tasks = new UniTask[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                tasks[i] = buttons[i].OnClickAsync(linkedToken);
            }
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            int clickIndex = await UniTask.WhenAny(tasks); // 選ばれなかった方のタスクはキャンセルしないと残るので注意
            cts.Cancel();
            cts.Dispose();

            buttonCreator.AllClearFadeAsync(0.1f, token).Forget();
            if (selectSE != null)
            {
                NovelManager.Instance.PlayOneShot(selectSE, seVolume);
            }
            return clickIndex;
        }

        /// <summary>
        /// テキストに加え、ボタンごとの決定効果音も設定できます
        /// </summary>
        public async UniTask<int> ShowAndWaitButtonClick(CancellationToken token, params (string text, AudioClip se)[] textAndSEs)
        {
            var texts = new string[textAndSEs.Length];
            var ses = new AudioClip[textAndSEs.Length];
            for (int i = 0; i < textAndSEs.Length; i++)
            {
                (texts[i], ses[i]) = textAndSEs[i];
            }

            int clickIndex = await ShowAndWaitButtonClick(token, texts);

            var se = ses[clickIndex];
            if (se != null)
            {
                NovelManager.Instance.PlayOneShot(se, seVolume);
            }
            else if (selectSE != null)
            {
                NovelManager.Instance.PlayOneShot(selectSE, seVolume);
            }
            return clickIndex;
        }

        void SetAlpha(float a)
        {
            canvasGroup.alpha = a;
        }
        float GetAlpha() => canvasGroup.alpha;

        /// <summary>
        /// アクティブにしてからフェードインします
        /// </summary>
        public async UniTask ShowFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            gameObject.SetActive(true);
            await FadeAlphaAsync(0f, 1f, time, token);
        }

        /// <summary>
        /// フェードアウトしてから非アクティブにします
        /// </summary>
        public async UniTask ClearFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            await FadeAlphaAsync(GetAlpha(), 0f, time, token);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 指定した透明度まで連続的に変化させます
        /// </summary>
        async UniTask FadeAlphaAsync(float startAlpha, float toAlpha, float time, CancellationToken token)
        {
            var t = 0f;
            while (t < time)
            {
                SetAlpha(t.Ease(startAlpha, toAlpha, time, EaseType.OutQuad));
                t += Time.deltaTime;
                await UniTask.Yield(token == default ? this.GetCancellationTokenOnDestroy() : token);
            }
            SetAlpha(toAlpha);
        }
        async UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
        {
            var t = 0f;
            while (t < time)
            {
                SetAlpha(t.Ease(GetAlpha(), toAlpha, time, EaseType.OutQuad));
                t += Time.deltaTime;
                await UniTask.Yield(token == default ? this.GetCancellationTokenOnDestroy() : token);
            }
            SetAlpha(toAlpha);
        }
    }
}