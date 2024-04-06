using UnityEngine;
using UnityEngine.UI;

namespace Novel
{
    public class MessageBox : FadableMonoBehaviour
    {
        [SerializeField] BoxType type;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image boxImage;
        [SerializeField] Writer writer;
        [SerializeField] MsgBoxInput input;

        public Writer Writer => writer; 
        public MsgBoxInput Input => input;

        public bool IsMeetType(BoxType type) => this.type == type;

        protected override float GetAlpha() => canvasGroup.alpha;

        protected override void SetAlpha(float a)
        {
            canvasGroup.alpha = a;
        }
    }
}