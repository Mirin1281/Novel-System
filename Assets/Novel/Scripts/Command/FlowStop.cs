using UnityEngine;
using Cysharp.Threading.Tasks;
using StopType = Novel.Flowchart.StopType;

namespace Novel.Command
{
    [AddTypeMenu(nameof(FlowStop)), System.Serializable]
    public class FlowStop : CommandBase
    {
        [SerializeField] StopType stopType;
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

            ParentFlowchart.Stop(stopType);

            try
            {
                Token.ThrowIfCancellationRequested();
            }
            catch
            {
                NovelManager.Instance.ClearAllUI();
                throw;
            }

            if (ParentFlowchart.CallStatus.ExistWaitOthers == false)
            {
                NovelManager.Instance.ClearAllUI();
            }
            await UniTask.CompletedTask;
        }

        protected override string GetSummary()
        {
            return stopType.ToString();
        }
    }
}
