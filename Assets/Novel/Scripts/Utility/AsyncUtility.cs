using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Novel
{
    public static class AsyncUtility
    {
        public static UniTask Seconds(float waitTime, CancellationToken token)
        {
            if (waitTime > 0)
            {
                return UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
            }
            else
            {
                return UniTask.CompletedTask;
            }
        }
    }
}