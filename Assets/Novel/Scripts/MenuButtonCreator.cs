using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Novel
{
    public class MenuButtonCreator : MonoBehaviour
    {
        [SerializeField] MenuButton buttonPrefab;
        List<MenuButton> createButtons;

        void Awake()
        {
            createButtons = new(transform.childCount);
            foreach(var btn in GetComponentsInChildren<MenuButton>())
            {
                btn.gameObject.SetActive(false);
                createButtons.Add(btn);
            }
            SceneManager.activeSceneChanged += AllClear;
        }
        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= AllClear;
        }

        void AllClear(Scene _, Scene __)
        {
            AllClearFadeAsync(0).Forget();
        }

        public IReadOnlyList<MenuButton> CreateShowButtons(params string[] texts)
        {
            createButtons ??= new(texts.Length);
            int currentCount = createButtons.Count;

            int createCount = texts.Length;
            if (createCount == currentCount) // 今ある子にいるボタンと必要なボタンの数が同じとき
            {
                AllShowFadeAsync(createButtons, 0f).Forget();
                SetNames(createButtons, texts);
                return createButtons;
            }
            else if (createCount > currentCount) // 子のボタンより必要なボタンの数の方が多いとき
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
                foreach(var button in createButtons)
                {
                    buttons.Add(button);
                    button.ShowFadeAsync(0f).Forget();
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

        async UniTask AllShowFadeAsync(List<MenuButton> buttons, float time = ConstContainer.DefaultFadeTime)
        {
            foreach (var button in buttons)
            {
                button.ShowFadeAsync(time).Forget();
            }
            await AsyncUtility.Seconds(time, this.GetCancellationTokenOnDestroy());
        }

        public async UniTask AllClearFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            foreach(var button in createButtons)
            {
                if(button.gameObject.activeInHierarchy)
                {
                    button.ClearFadeAsync(time, token).Forget();
                }
            }
            await AsyncUtility.Seconds(time, token == default ? this.GetCancellationTokenOnDestroy() : token);
        }
    }
}