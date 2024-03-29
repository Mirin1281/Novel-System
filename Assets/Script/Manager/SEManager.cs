using UnityEngine;

namespace Novel
{
    public class SEManager : SingletonMonoBehaviour<SEManager>
    {
        [SerializeField] AudioSource audioSource;
        /*[SerializeField] SETypeData seData;

        /// <summary>
        /// SE‚ð–Â‚ç‚µ‚Ü‚·
        /// </summary>
        /// <param name="type">SETypeŒ^‚ÅŽw’è</param>
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
