using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    public interface ICommand
    {
        UniTask ExecuteAsync(Flowchart flowchart, FlowchartCallStatus callStatus);
        CommandStatus GetCommandStatus();
    }
}
