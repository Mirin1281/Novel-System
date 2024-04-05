using Cysharp.Threading.Tasks;
using System.Threading;

namespace Novel
{
    public interface IFadable
    {
        UniTask ShowFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default);
        UniTask ClearFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default);
    }
}