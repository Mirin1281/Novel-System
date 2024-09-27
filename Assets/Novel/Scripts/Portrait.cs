using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public class Portrait : FadableMonoBehaviour
    {
        public enum PortraitPositionType
        {
            Left,
            Center,
            Right,
            Custom,
        }

        [SerializeField] PortraitType type;
        [SerializeField] Image portraitImage;
        [SerializeField] Vector2 leftPosition = new(-400, -100);
        [SerializeField] Vector2 centerPosition = new(0, -100);
        [SerializeField] Vector2 rightPosition = new(400, -100);

        readonly Color hideColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        public bool IsTypeEqual(PortraitType type) => this.type == type;

        public Transform PortraitImageTs => portraitImage.transform;

        public async UniTask TurnAsync(float time, CancellationToken token = default)
        {
            var startScaleX = PortraitImageTs.localScale.x;
            var startScaleY = PortraitImageTs.localScale.y;
            var endScaleX = -startScaleX;

            if (time == 0f)
            {
                SetScaleX(endScaleX);
                return;
            }
            var outQuad = new Easing(startScaleX, endScaleX, time, EaseType.OutQuad);
            var t = 0f;
            while (t < time)
            {
                SetScaleX(outQuad.Ease(t));
                t += Time.deltaTime;
                await UniTask.Yield(token == default ? this.GetCancellationTokenOnDestroy() : token);
            }
            SetScaleX(endScaleX);


            void SetScaleX(float x)
            {
                PortraitImageTs.localScale = new Vector3(x, startScaleY);
            }
        }

        public void SetPos(PortraitPositionType posType)
        {
            var pos = posType switch
            {
                PortraitPositionType.Left => leftPosition,
                PortraitPositionType.Center => centerPosition,
                PortraitPositionType.Right => rightPosition,
                _ => throw new System.Exception()
            };
            PortraitImageTs.localPosition = pos;
        }
        public void SetPos(Vector2 pos)
        {
            PortraitImageTs.localPosition = pos;
        }

        public void SetSprite(Sprite sprite)
        {
            portraitImage.sprite = sprite;
        }

        /// <summary>
        /// ハイド(暗くなった状態)を切り替えます
        /// </summary>
        /// <param name="enable"></param>
        public void SetHide(bool enable)
        {
            portraitImage.color = enable ? hideColor : Color.white;
        }

        protected override float GetAlpha() => portraitImage.color.a;

        protected override void SetAlpha(float a)
        {
            portraitImage.SetAlpha(a);
        }
    }
}