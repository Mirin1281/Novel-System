﻿using UnityEngine;
using UnityEngine.UI;

namespace Novel
{
    public class MessageBox : FadableMonoBehaviour
    {
        [SerializeField] BoxType type;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Writer writer;
        [SerializeField] MessageBoxInput input;

        public Writer Writer => writer; 
        public MessageBoxInput Input => input;

        public bool IsMeetType(BoxType type) => this.type == type;

        protected override float GetAlpha() => canvasGroup.alpha;

        protected override void SetAlpha(float a)
        {
            canvasGroup.alpha = a;
        }
    }
}