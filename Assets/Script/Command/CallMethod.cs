using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    //[AddTypeMenu("CallMethod"), System.Serializable]
    public class CallMethodCommand : CommandBase
    {
        [SerializeField] UnityEvent methodEvent;

        protected override async UniTask EnterAsync()
        {
            methodEvent?.Invoke();
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            var eventCount = methodEvent.GetPersistentEventCount();
            if (eventCount == 0) return WarningText();
            var firstEventName = $"{methodEvent.GetPersistentTarget(0)} ▷ {methodEvent.GetPersistentMethodName(0)}";
            return eventCount switch
            {
                1 => firstEventName,
                > 1 => firstEventName + " など",
                _ => WarningText()
            };
        }
    }
}