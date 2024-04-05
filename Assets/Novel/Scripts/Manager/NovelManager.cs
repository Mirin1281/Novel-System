using UnityEngine;

namespace Novel
{
    public class NovelManager : SingletonMonoBehaviour<NovelManager>
    {
        protected override void Awake()
        {
            base.Awake();
            StaticToken.Init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StaticToken.ResetToken();
        }

        public float DefaultWriteSpeed { get; private set; } = 2;
    }
}