using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MenuManager : SingletonMonoBehaviour<MenuManager>
    {
        [SerializeField] MenuButtonCreator buttonCreator;

        public async UniTask<int> ShowAndWaitButtonClick(CancellationToken token, params string[] texts)
        {
            gameObject.SetActive(true);
            var buttons = buttonCreator.CreateShowButtons(texts);
            var tasks = new UniTask[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                tasks[i] = buttons[i].Button.OnClickAsync(token);
            }
            int clickIndex = await UniTask.WhenAny(tasks);
            buttonCreator.AllClearFadeAsync(0.1f, token).Forget();
            return clickIndex;
        }

        /*public async UniTask<int> ShowAndDisposeAsync(CancellationToken token, params string[] texts)
        {
            int index = await ShowAndWaitButtonClick(token, texts);
            Destroy(gameObject, 0.2f);
            return index;
        }*/
    }
}