using UnityEngine;

namespace Novel
{
    // メニューや会話時のクリック音を鳴らします
    // 各自のSEManagerに置き換えることを推奨します
    public class SEManager : SingletonMonoBehaviour<SEManager>
    {
        [SerializeField] AudioSource audioSource;

        public void PlaySE(AudioClip se, float volumeRate = 1f)
        {
            float volume = volumeRate;
            audioSource.PlayOneShot(se, volume);
        }
    }
}
