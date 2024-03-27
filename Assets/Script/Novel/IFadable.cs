using Cysharp.Threading.Tasks;

public interface IFadable
{
    UniTask ShowFadeAsync(float time = MyStatic.DefaultFadeTime);

    UniTask ClearFadeAsync(float time = MyStatic.DefaultFadeTime);
}
