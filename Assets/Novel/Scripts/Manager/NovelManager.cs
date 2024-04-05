using UnityEngine;

namespace Novel
{
    public class NovelManager : SingletonMonoBehaviour<NovelManager>
    {
        protected override void Awake()
        {
            base.Awake();
            MyStatic.Init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MyStatic.ResetToken();
        }

        public float DefaultWriteSpeed { get; private set; } = 2;
    }
}