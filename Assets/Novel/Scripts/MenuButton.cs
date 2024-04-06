using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class MenuButton : FadableMonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] TMP_Text tmpro;
        [SerializeField] Button button;
        public UniTask OnClickAsync(CancellationToken token) => button.OnClickAsync(token);

        public void SetText(string s)
        {
            tmpro.SetText(s);
        }

        protected override float GetAlpha() => image.color.a;

        protected override void SetAlpha(float a)
        {
            image.SetAlpha(a);
        }
    }
}