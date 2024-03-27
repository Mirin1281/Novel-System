using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class MenuButtonCreator : MonoBehaviour
    {
        [SerializeField] MenuButton buttonPrefab;
        List<MenuButton> createButtons = new();

        public IReadOnlyList<MenuButton> CreateShowButtons(params string[] texts)
        {
            int createCount = texts.Length;
            int currentCount = 0;
            if (createButtons == null)
            {
                createButtons = new();
            }
            else
            {
                currentCount = createButtons.Count;
            }

            if (createCount == currentCount)
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
            await MyStatic.WaitSeconds(time);
        }

        public async UniTask AllClearFadeAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach(var button in createButtons)
            {
                if(button.gameObject.activeInHierarchy)
                {
                    button.ClearFadeAsync(time).Forget();
                }
            }
            await MyStatic.WaitSeconds(time);
        }
    }
}