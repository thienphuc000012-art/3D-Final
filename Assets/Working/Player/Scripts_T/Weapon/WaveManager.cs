using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public int currentWave = 1;
    public int killsPerWave = 5;
    public int currentKills = 0;

    void Awake()
    {
        Instance = this;
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

        // ❌ KHÔNG buff gun ở đây nữa
        // Wave chỉ dùng để test / spawn / difficulty
    }

    public void ResetWaves()
    {
        currentWave = 1;
        currentKills = 0;
    }
}
