using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("CallScript"), System.Serializable]
    public class CallScriptCommand : CommandBase
    {
        [SerializeField] UnityEvent unityEvent;

        protected override async UniTask EnterAsync()
        {
            unityEvent?.Invoke();
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            if(unityEvent == null ||
               unityEvent.GetPersistentEventCount() == 0 ||
               unityEvent.GetPersistentTarget(0) == null ||
               string.IsNullOrEmpty(unityEvent.GetPersistentMethodName(0)))
            {
                return WarningColorText();
            }
            return unityEvent.GetPersistentMethodName(0);
        }
    }
}
