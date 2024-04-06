using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public enum PortraitPosType
    {
        Left,
        Center,
        Right,
        Custom,
    }

    public class Portrait : FadableMonoBehaviour
    {
        [SerializeField] PortraitType type;
        [SerializeField] Image portraitImage;
        [SerializeField] Vector2 leftPosition = new(-400, -100);
        [SerializeField] Vector2 centerPosition = new(0, -100);
        [SerializeField] Vector2 rightPosition = new(400, -100);

        readonly Color hideColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);


        public bool IsMeetType(PortraitType type) => this.type == type;

        public Transform PortraitImageTs => portraitImage.transform;

        public async UniTask TurnAsync(float time, CancellationToken token = default)
        {
            var startScaleX = portraitImage.transform.localScale.x;
            var startScaleY = portraitImage.transform.localScale.y;
            var endScaleX = -startScaleX;

            if (time == 0f)
            {
                SetScaleX(endScaleX);
                return;
            }
            var outQuad = new OutQuad(endScaleX, time, startScaleX);
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
                portraitImage.transform.localScale = new Vector3(x, startScaleY);
            }
        }

        public void SetPos(PortraitPosType posType)
        {
            var pos = posType switch
            {
                PortraitPosType.Left => leftPosition,
                PortraitPosType.Center => centerPosition,
                PortraitPosType.Right => rightPosition,
                _ => throw new System.Exception()
            };
            portraitImage.transform.localPosition = pos;
        }
        public void SetPos(Vector2 pos)
        {
            portraitImage.transform.localPosition = pos;
        }

        public void SetSprite(Sprite sprite)
        {
            portraitImage.sprite = sprite;
        }

        public void HideOn()
        {
            portraitImage.color = hideColor;
        }

        public void HideOff()
        {
            portraitImage.color = Color.white;
        }

        protected override float GetAlpha() => portraitImage.color.a;

        protected override void SetAlpha(float a)
        {
            portraitImage.SetAlpha(a); ;
        }
    }
}