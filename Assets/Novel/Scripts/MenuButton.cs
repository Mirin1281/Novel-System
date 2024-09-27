using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    [RequireComponent(typeof(Image), typeof(Button))]
    public class MenuButton : FadableMonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] Button button;
        [SerializeField] TMP_Text tmpro;
        
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