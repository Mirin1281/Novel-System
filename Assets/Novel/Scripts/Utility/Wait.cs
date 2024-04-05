using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Novel
{
    public static class Wait
    {
        public static UniTask Seconds(float waitTime, CancellationToken token)
        {
            if (waitTime > 0)
            {
                return UniTask.Delay(TimeSpan.FromSeconds(waitTime),
                    cancellationToken: token == default ? StaticToken.TokenOnSceneChange : token);
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }

        public static UniTask Yield(CancellationToken token)
            => UniTask.Yield(token == default ? StaticToken.TokenOnSceneChange : token);
    }
}