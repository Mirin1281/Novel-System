using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu(nameof(FlowJump)), System.Serializable]
    public class FlowJump : CommandBase
    {
        enum JumpType
        {
            [InspectorName("絶対パス")] Absolute,
            [InspectorName("このコマンドよりN個上")] UpRelative,
            [InspectorName("このコマンドよりN個下")] DownRelative,
        }

        [SerializeField] int jumpIndex;
        [SerializeField] JumpType jumpType;
        [SerializeField, Tooltip("エディタでのみ動作")]
        bool editorOnly;

        protected override async UniTask EnterAsync()
        {
            if (editorOnly)
            {
#if UNITY_EDITOR
#else
                return;
#endif
            }
            int index = jumpType switch
            {
                JumpType.Absolute => jumpIndex,
                JumpType.UpRelative => Index - jumpIndex,
                JumpType.DownRelative => Index + jumpIndex,
                _ => throw new System.Exception()
            };
            ParentFlowchart.SetIndex(index, true);
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            int index = jumpType switch
            {
                JumpType.Absolute => jumpIndex,
                JumpType.UpRelative => Index - jumpIndex,
                JumpType.DownRelative => Index + jumpIndex,
                _ => throw new System.Exception()
            };

            var cmdDataList = ParentFlowchart.GetReadOnlyCommandDataList();
            if (index < 0 || index >= cmdDataList.Count || index == Index) return WarningText();
            var cmd = cmdDataList[index].GetCommandBase();
            if (cmd == null || cmdDataList[index].Enabled == false) return WarningText();
            if (Index < index)
            {
                return $"<color=#000080>▽ [ {GetName(cmd)} ] ▽</color>";
            }
            else
            {
                return $"<color=#000080>△ [ {GetName(cmd)} ] △</color>";
            }
        }
    }
}
