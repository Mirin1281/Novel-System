using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

namespace Novel
{
    public static class StaticToken
    {
        static CancellationTokenSource cts;
        public static CancellationToken TokenOnSceneChange;

        public static void Init()
        {
            SceneManager.activeSceneChanged += (_, _) => ResetToken();
        }

        public static void ResetToken()
        {
            cts?.Cancel();
            cts = new();
            TokenOnSceneChange = cts.Token;
        }
    }
}