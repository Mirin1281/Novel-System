using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

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

    public async UniTask WaitInput(Action action = null)
    {
        bool clicked = false;
        OnInputed += () => clicked = true;
        if(action != null)
        {
            OnInputed += action;
        }
        await UniTask.WaitUntil(() => clicked, cancellationToken: MyStatic.Token);
        OnInputed -= () => clicked = true;
        if (action != null)
        {
            OnInputed -= action;
        }
    }
}
