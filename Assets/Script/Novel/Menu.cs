using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] MenuButtonCreator buttonCreator;

        public async UniTask<int> WaitButtonClick(params string[] text)
        {
            var buttons = buttonCreator.CreateShowButtons(text);
            var tasks = new UniTask[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                tasks[i] = buttons[i].Button.OnClickAsync();
            }
            int clickIndex = await UniTask.WhenAny(tasks);
            buttonCreator.AllFadeOutAsync(0.1f).Forget();
            return clickIndex;
        }
    }
}