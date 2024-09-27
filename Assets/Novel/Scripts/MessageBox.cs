using UnityEngine;

namespace Novel
{
    [RequireComponent(typeof(CanvasGroup), typeof(Writer), typeof(MessageBoxInput))]
    public class MessageBox : FadableMonoBehaviour
    {
        [SerializeField] BoxType type;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Writer writer;
        [SerializeField] MessageBoxInput input;

        public Writer Writer => writer;
        public MessageBoxInput Input => input;

        public bool IsTypeEqual(BoxType type) => this.type == type;

        protected override float GetAlpha() => canvasGroup.alpha;

        protected override void SetAlpha(float a)
        {
            canvasGroup.alpha = a;
        }

        void Awake()
        {
            if(canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if(writer == null)
            {
                writer = GetComponent<Writer>();
            }
            if (input == null)
            {
                input = GetComponent<MessageBoxInput>();
            }
        }
    }
}