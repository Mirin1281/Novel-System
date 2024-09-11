using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Novel.Command
{
	[AddTypeMenu("LoadScene"), System.Serializable]
	public class LoadScene : CommandBase
	{
		[SerializeField] string sceneName;
		[SerializeField] float waitSeconds = 0.2f;

		protected override async UniTask EnterAsync()
		{
			await Novel.Wait.Seconds(waitSeconds, CallStatus.Token);
			ParentFlowchart.Stop(Flowchart.StopType.All);
			await SceneManager.LoadSceneAsync(sceneName);
		}

		protected override string GetSummary()
		{
			if (string.IsNullOrEmpty(sceneName)) return WarningText();
			if(Index < ParentFlowchart.GetReadOnlyCommandDataList().Count - 1)
            {
				return $"To {sceneName} {WarningText()}";
			}
			return $"To {sceneName}";
		}
	}
}