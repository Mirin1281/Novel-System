using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Novel
{
    public class MenuButtonCreator : MonoBehaviour
    {
        [SerializeField] MenuButton buttonPrefab;
        List<MenuButton> createButtons = new();

        void Awake()
        {
            createButtons = GetComponentsInChildren<MenuButton>().ToList();
            createButtons.ForEach(button => button.gameObject.SetActive(false));
            SceneManager.activeSceneChanged += (_, __) => AllClearFadeAsync(0).Forget();
        }
        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= (_, __) => AllClearFadeAsync(0).Forget();
        }

        public IReadOnlyList<MenuButton> CreateShowButtons(params string[] texts)
        {
            int currentCount = 0;
            if (createButtons == null)
            {
                createButtons = new();
            }
            else
            {
                currentCount = createButtons.Count;
            }

            int createCount = texts.Length;
            if (createCount == currentCount) // 今ある子にいるボタンと必要なボタンの数が同じとき
            {
                AllShowFadeAsync(createButtons, 0f).Forget();
                SetNames(createButtons, texts);
                return createButtons;
            }
            else if (createCount > currentCount)
            {
                for (int i = 0; i < createCount; i++)
                {
                    if (i >= currentCount)
                    {
                        createButtons.Add(Instantiate(buttonPrefab, transform));
                    }
                }
                AllShowFadeAsync(createButtons, 0f).Forget();
                SetNames(createButtons, texts);
                return createButtons;
            }
            else
            {
                var buttons = new List<MenuButton>(createCount);
                for (int i = 0; i < createCount; i++)
                {
                    buttons.Add(createButtons[i]);
                    createButtons[i].ShowFadeAsync(0f).Forget();
                }
                AllShowFadeAsync(buttons, 0f).Forget();
                SetNames(buttons, texts);
                return buttons;
            }
        }

        void SetNames(List<MenuButton> buttons, string[] texts)
        {
            for(int i = 0; i < texts.Length; i++)
            {
                buttons[i].SetText(texts[i]);
            }
        }

        async UniTask AllShowFadeAsync(List<MenuButton> buttons, float time = MyStatic.DefaultFadeTime)
        {
            foreach (var button in buttons)
            {
                button.ShowFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time, destroyCancellationToken);
        }

        public async UniTask AllClearFadeAsync(float time = MyStatic.DefaultFadeTime, CancellationToken token = default)
        {
            foreach(var button in createButtons)
            {
                if(button.gameObject.activeInHierarchy)
                {
                    button.ClearFadeAsync(time, token).Forget();
                }
            }
            await MyStatic.WaitSeconds(time, token == default ? this.GetCancellationTokenOnDestroy() : token);
        }
    }
}