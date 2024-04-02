using Cysharp.Threading.Tasks;
using System.Threading;

public interface IFadable
{
    UniTask ShowFadeAsync(float time = MyStatic.DefaultFadeTime, CancellationToken token = default);
    UniTask ClearFadeAsync(float time = MyStatic.DefaultFadeTime, CancellationToken token = default);
}
