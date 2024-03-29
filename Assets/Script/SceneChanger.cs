using UnityEngine;
using UnityEngine.EventSystems;

namespace Novel
{
    /// <summary>
    /// 概要: シーンを移動します。ボタンのイベントに設定したりして使います
    /// 　　  また、キーを設定すると、そのキーが押された際にシーンを移動します
    /// </summary>
    public class SceneChanger : MonoBehaviour
    {
        [SerializeField] string sceneName = "TitleScene";
        [SerializeField] float fadeTime = 0.5f;

        public void LoadScene()
        {
            EventSystem.current.SetSelectedGameObject(null);
            FadeLoadSceneManager.Instance.LoadScene(fadeTime, sceneName);
        }
    }
}