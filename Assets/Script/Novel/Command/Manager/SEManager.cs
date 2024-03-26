using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SEManager : SingletonMonoBehaviour<SEManager>
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] SEData seData;

    /// <summary>
    /// SEを鳴らします
    /// </summary>
    /// <param name="type">SEType型で指定</param>
    /// <param name="volumeRate"></param>
    public void PlaySE(SEType type, float volumeRate = 1f)
    {
        var (se, vol) = seData.GetSEAndVolume(type);
        float volume = volumeRate * vol * GameManager.Instance.SEVolume * MyStatic.SEMasterVolume;
        audioSource.PlayOneShot(se, volume);
    }
}
