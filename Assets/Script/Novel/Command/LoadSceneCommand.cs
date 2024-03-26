using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("LoadScene"), System.Serializable]
    public class LoadSceneCommand : CommandBase
    {
        [SerializeField, SceneChanger] string sceneName;
        [SerializeField] float fadeTime;

        protected override async UniTask EnterAsync()
        {
            FadeLoadSceneManager.Instance.LoadScene(fadeTime, sceneName);
            await MyStatic.WaitSeconds(5f);
        }

        protected override string GetSummary()
        {
            return sceneName;
        }
    }
}