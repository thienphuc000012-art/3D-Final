using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public int currentWave = 1;
    public int killsPerWave = 5;
    public int currentKills = 0;

    public Gun playerGun;

    void Awake()
    {
        Instance = this;
        if (playerGun == null)
            playerGun = FindObjectOfType<Gun>();
    }

    public void OnEnemyKilled()
    {
        currentKills++;
        Debug.Log($"Kill: {currentKills}/{killsPerWave}");

        if (currentKills >= killsPerWave)
            NextWave();
    }

    void NextWave()
    {
        currentWave++;
        currentKills = 0;
        Debug.Log($"=== NEXT WAVE: {currentWave} ===");

        if (playerGun != null)
            playerGun.ApplyWaveUpgrade(currentWave);
    }

    public void ResetWaves()
    {
        currentWave = 1;
        currentKills = 0;
        if (playerGun != null)
            playerGun.ResetGunStats();
    }
}
