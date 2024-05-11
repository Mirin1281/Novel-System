using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MessageBoxInput : MonoBehaviour
    {
        [SerializeField] AudioClip inputSE;
        float onCancelKeyTime;
        float seVolume;

        public event Action OnInputed;

        void Update()
        {
            if (Input.GetButtonDown(ConstContainer.SUBMIT_KEYNAME) || Input.GetButtonDown(ConstContainer.CANCEL_KEYNAME))
            {
                OnInputed?.Invoke();
            }

            if (Input.GetButton(ConstContainer.CANCEL_KEYNAME))
            {
                onCancelKeyTime += Time.deltaTime;
            }
            else
            {
                onCancelKeyTime = 0f;
            }

            seVolume = 1f;
            if (onCancelKeyTime > 0.7f)
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
            OnInputed += () => clicked = true;
            if (action != null)
            {
                OnInputed += action;
            }

            await UniTask.WaitUntil(() => clicked, cancellationToken: token);

            if (inputSE != null)
            {
                SEManager.Instance.PlaySE(inputSE, seVolume);
            }
            OnInputed -= () => clicked = true;
            if (action != null)
            {
                OnInputed -= action;
            }
        }
    }
}