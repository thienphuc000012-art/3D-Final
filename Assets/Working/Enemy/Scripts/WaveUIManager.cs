using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI waveText;
    public Slider waveProgressSlider; 

    private int totalZombiesInWave;
    private int zombiesSpawned;
    private Coroutine fillCoroutine;

    public void InitWave(int waveNumber, int totalZombies)
    {
        waveText.text = "Wave " + waveNumber;
        totalZombiesInWave = totalZombies;
        zombiesSpawned = 0;
        waveProgressSlider.value = 0; 
    }

    public void OnZombieSpawned()
    {
        zombiesSpawned++;
        float targetProgress = (float)zombiesSpawned / totalZombiesInWave;

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(SmoothFill(targetProgress));
    }

    private IEnumerator SmoothFill(float target)
    {
        float start = waveProgressSlider.value;
        float duration = 7f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            waveProgressSlider.value = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        waveProgressSlider.value = target;
    }
}