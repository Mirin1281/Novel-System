using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MsgBoxInput : MonoBehaviour
{
    public event Action OnInputed;
    public event Action OnCancelStay;

    void Update()
    {
        if (Input.GetButtonDown(NameContainer.SUBMIT_KEYNAME))
        {
            OnInputed?.Invoke();
        }
        else if (Input.GetButton(NameContainer.CANCEL_KEYNAME))
        {
            OnCancelStay?.Invoke();
        }
    }

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
        if(action != null)
        {
            OnInputed += action;
        }
        await UniTask.WaitUntil(() => clicked, cancellationToken: token);
        OnInputed -= () => clicked = true;
        if (action != null)
        {
            OnInputed -= action;
        }
    }
}
