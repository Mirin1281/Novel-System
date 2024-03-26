using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class FadeLoadSceneManager : SingletonMonoBehaviour<FadeLoadSceneManager>
{
    [SerializeField] Image fadeImage;

    /// <summary>
    /// フェードしながらシーンを遷移します
    /// </summary>
    /// <param name="fadeInterval">暗転、明転の秒数</param>
    /// <param name="sceneName">遷移先のシーン名</param>
    public void LoadScene(float fadeInterval, string sceneName)
    {
        LoadSceneAsync(fadeInterval, sceneName).Forget();
    }

    // こっちはawaitできる
    public async UniTask LoadSceneAsync(float fadeInterval, string sceneName)
    {
        if(fadeInterval != 0f)
        {
            await FadeIn(fadeInterval);
        }
            
        await SceneManager.LoadSceneAsync(sceneName);

        if(fadeInterval != 0f)
        {
            await FadeOut(fadeInterval);
        }
    }

    public async UniTask FadeIn(float interval, Color? fadeColor = null)
    {
        gameObject.SetActive(true);
        var time = 0f;
        while (time <= interval)
        {
            fadeImage.color = Color.Lerp(Color.clear, fadeColor ?? Color.black, time);
            time += Time.deltaTime;
            await UniTask.Yield();
        }
        fadeImage.color = fadeColor ?? Color.black;
    }

    public async UniTask FadeOut(float interval, Color? fadeColor = null)
    {
        var time = 0f;
        while (time <= interval)
        {
            fadeImage.color = Color.Lerp(fadeColor ?? Color.black, Color.clear, time);
            time += Time.deltaTime;
            await UniTask.Yield();
        }
        gameObject.SetActive(false);
    }
}