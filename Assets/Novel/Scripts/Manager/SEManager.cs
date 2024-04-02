using UnityEngine;

namespace Novel
{
    // ���j���[���b���̃N���b�N����炵�܂�
    // �e����SEManager�ɒu�������邱�Ƃ𐄏����܂�
    public class SEManager : SingletonMonoBehaviour<SEManager>
    {
        [SerializeField] AudioSource audioSource;
        /*[SerializeField] SETypeData seData;

        /// <summary>
        /// SE��炵�܂�
        /// </summary>
        /// <param name="type">SEType�^�Ŏw��</param>
        /// <param name="volumeRate"></param>
        public void PlaySE(SEType type, float volumeRate = 1f)
        {
            var (se, vol) = seData.GetSE(type);
            float volume = volumeRate * vol * GameManager.Instance.SEVolume * MyStatic.SEMasterVolume;
            audioSource.PlayOneShot(se, volume);
        }*/

        public void PlaySE(AudioClip se, float volumeRate = 1f)
        {
            float volume = volumeRate;
            audioSource.PlayOneShot(se, volume);
        }
    }
}
