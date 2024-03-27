using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] MenuButtonCreator buttonCreator;

        void Awake()
        {
            gameObject.SetActive(false);
        }

        public async UniTask<int> ShowAndWaitButtonClick(params string[] text)
        {
            gameObject.SetActive(true);
            var buttons = buttonCreator.CreateShowButtons(text);
            var tasks = new UniTask[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                tasks[i] = buttons[i].Button.OnClickAsync();
            }
            int clickIndex = await UniTask.WhenAny(tasks);
            buttonCreator.AllClearFadeAsync(0.1f).Forget();
            return clickIndex;
        }
    }
}