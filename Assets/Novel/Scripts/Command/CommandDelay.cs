using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("CommandDelay"), System.Serializable]
    public class CommandDelay : CommandBase
    {
        [SerializeField] float time = 0f;
        [SerializeField, SerializeReference, SubclassSelector, Space(10)]
        ICommand command;

        protected override async UniTask EnterAsync()
        {
            DelayExecuteCommand().Forget();
            await UniTask.CompletedTask;
        }

        async UniTask DelayExecuteCommand()
        {
            await Novel.Wait.Seconds(time, CallStatus.Token);
            command.ExecuteAsync(ParentFlowchart, CallStatus).Forget();
        }


        protected override string GetSummary()
        {
            if (command == null) return WarningText();
            return $"  {time}s: {command.GetName()}";
        }
    }
}
