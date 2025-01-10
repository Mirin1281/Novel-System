using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    //[AddTypeMenu(nameof(CommandDelay)), System.Serializable]
    public class CommandDelay : CommandBase
    {
        [SerializeField] float time = 0f;
        [SerializeField, SerializeReference, SubclassSelector, Space(10)]
        ICommand command;

        protected override async UniTask EnterAsync()
        {
            await AsyncUtility.Seconds(time, Token);
            command.ExecuteAsync(ParentFlowchart).Forget();
        }

        protected override string GetSummary()
        {
            if (command == null) return WarningText();
            return $"  {time}s: {command.GetName()}";
        }
    }
}
