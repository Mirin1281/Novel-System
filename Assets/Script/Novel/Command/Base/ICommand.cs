using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novel.Command
{
    public interface ICommand
    {
        UniTask CallCommandAsync(IFlowchart flowchart);
        CommandStatus GetCommandStatus();
    }

    public interface IFlowchart
    {
        UniTask ExecuteAsync(int index = 0, bool isNest = false);
        void Stop();
    }
}