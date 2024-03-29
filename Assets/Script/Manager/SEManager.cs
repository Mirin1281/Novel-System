using UnityEngine;

namespace Novel
{
    public class SEManager : SingletonMonoBehaviour<SEManager>
    {
        [SerializeField] AudioSource audioSource;
        /*[SerializeField] SETypeData seData;

        /// <summary>
        /// SEを鳴らします
        /// </summary>
        /// <param name="type">SEType型で指定</param>
        /// <param name="volumeRate"></param>
        public void PlaySE(SEType type, float volumeRate = 1f)
        {
            var (se, vol) = seData.GetSE(type);
            float volume = volumeRate * vol * GameManager.Instance.SEVolume * MyStatic.SEMasterVolume;
            audioSource.PlayOneShot(se, volume);
        }*/

        public void PlaySE(AudioClip se, float volumeRate = 1f)
        {
            float volume = volumeRate * MyStatic.SEMasterVolume;
            audioSource.PlayOneShot(se, volume);
        }
    }
}
