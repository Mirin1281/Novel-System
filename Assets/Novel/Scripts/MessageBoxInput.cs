using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MessageBoxInput : MonoBehaviour
    {
        [SerializeField] AudioClip inputSE;
        float seVolume;

        public event Action OnInputed;

        void OnDestroy()
        {
            OnInputed = null;
        }

        void Update()
        {
            if (Input.GetButtonDown(ConstContainer.SUBMIT_KEYNAME) || Input.GetButtonDown(ConstContainer.CANCEL_KEYNAME))
            {
                OnInputed?.Invoke();
            }

            if (Input.GetButton(ConstContainer.CANCEL_KEYNAME))
            {
                NovelManager.Instance.OnCancelKeyDown = true;
            }
            else
            {
                NovelManager.Instance.OnCancelKeyDown = false;
            }

            seVolume = 1f;
            if (NovelManager.Instance.OnSkip)
            {
                OnInputed?.Invoke();
                seVolume = 0.2f;
            }
        }

        /// <summary>
        /// EventTriggerで発火
        /// </summary>
        public void OnScreenClicked()
        {
            OnInputed?.Invoke();
        }

        /// <summary>
        /// 入力があるまで待ちます
        /// </summary>
        /// <param name="action">コールバック</param>
        /// <returns></returns>
        public async UniTask WaitInput(Action action = null, CancellationToken token = default)
        {
            bool clicked = false;
            OnInputed += Click;
            if (action != null)
            {
                OnInputed += action;
            }

            await UniTask.WaitUntil(() => clicked, cancellationToken: token);

            if (inputSE != null)
            {
                NovelManager.Instance.PlayOneShot(inputSE, seVolume);
            }
            OnInputed -= Click;
            if (action != null)
            {
                OnInputed -= action;
            }


            void Click() => clicked = true;
        }
    }
}