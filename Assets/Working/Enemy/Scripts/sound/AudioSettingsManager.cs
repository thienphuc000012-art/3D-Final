using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider zombieSlider; 
    public Slider bgmSlider;    

    [Header("Audio Sources")]
    public AudioSource bgmSource; 

    void Start()
    {

        if (zombieSlider != null)
        {
            zombieSlider.value = AudioManager.Instance.zombieVolume;
            zombieSlider.onValueChanged.AddListener(SetZombieVolume);
        }

        if (bgmSource != null && bgmSlider != null)
        {
            bgmSlider.value = bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
    }

    public void SetZombieVolume(float value)
    {
        AudioManager.Instance.SetZombieVolume(value);
    }

    public void SetBGMVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }
}