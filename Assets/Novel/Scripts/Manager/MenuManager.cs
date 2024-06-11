using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MenuManager : SingletonMonoBehaviour<MenuManager>
    {
        [SerializeField] MenuButtonCreator buttonCreator;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip selectSE;
        [SerializeField] float seVolume = 1f;

        protected override void Awake()
        {
            base.Awake();
            if(audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// ボタングループを表示して押されるまで待機します
        /// </summary>
        /// <param name="token"></param>
        /// <param name="texts">ボタンに表示するテキスト</param>
        /// <returns>押されたボタンのインデックス</returns>
        public async UniTask<int> ShowAndWaitButtonClick(
            CancellationToken token, params string[] texts)
        {
            gameObject.SetActive(true);
            var buttons = buttonCreator.CreateShowButtons(texts);
            var tasks = new UniTask[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                tasks[i] = buttons[i].OnClickAsync(token);
            }
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            int clickIndex = await UniTask.WhenAny(tasks);
            buttonCreator.AllClearFadeAsync(0.1f, token).Forget();
            if(selectSE != null)
            {
                audioSource.PlayOneShot(selectSE, seVolume);
            }
            
            return clickIndex;
        }

        /// <summary>
        /// ボタンごとの決定効果音も設定できます
        /// </summary>
        public async UniTask<int> ShowAndWaitButtonClick(
            CancellationToken token, params (string text, AudioClip se)[] textAndSEs)
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
                audioSource.PlayOneShot(se, seVolume);
            }
            else if (selectSE != null)
            {
                audioSource.PlayOneShot(selectSE, seVolume);
            }
            return clickIndex;
        }
    }
}