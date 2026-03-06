using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SpawnZombieManager : MonoBehaviour
{
    [Header("Zombie Prefabs Walk/Run")]
    public GameObject[] zombieWalkPrefabs;
    public GameObject[] zombieRunPrefabs;

    [Header("Zombie Prefabs Boss")]
    public GameObject[] zombieBossPrefabs;

    [Header("Spawn Lanes")]
    public Transform[] lanes;
    public Transform[] laneTargets;

    [Header("Wave Settings")]
    public float spawnInterval = 2.5f;
    public int currentWave = 1;
    public int zombiesPerWave = 5;

    [Header("Zombie Counts ")]
    public int walkCount;
    public int runCount;
    public int bossCount;

    [Header("Spawn Settings")]
    public float horizontalOffsetRange = 1.5f;

    [Header("Test Settings")]
    public bool overrideCounts = false;
    public int testWalkCount = 0;
    public int testRunCount = 0;
    public int testBossCount = 0;

    private bool spawning = false;
    private bool waveEnded = false;
    private int healthBonus = 0;

    [Header("Player Settings")]
    public PlayerHealth sharedPlayerHealth;

    private int aliveZombies = 0;
    private int phase = 1;

    // quản lý số lượng spawn
    private int zombiesToSpawnThisPhase;
    private int zombiesSpawned;
    private int zombiesPerPhase1; // số lượng zombie phase 1

    [Header("UI Settings")]
    public WaveMessageUI waveMessageUI;
    public ZombieUIManager zombieUIManager;

    void Start()
    {
        UpdateZombieCountByWave();
        StartCoroutine(StartWavePhase(spawnInterval));
    }

    void Update()
    {
        if (waveEnded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            waveEnded = false;
            currentWave++;
            UpdateZombieCountByWave();

            if (currentWave % 5 == 0)
            {
                healthBonus += 100;
                Debug.Log("Tăng máu cho tất cả enemy thêm 100. Tổng bonus: " + healthBonus);
            }

            StartCoroutine(StartWavePhase(spawnInterval));
        }
    }

    IEnumerator StartWavePhase(float interval)
    {
        spawning = true;

        int zombiesToSpawn;

        // phase 1 lưu số lượng, phase 2 dùng lại
        if (phase == 1)
        {
            zombiesToSpawn = zombiesPerWave;
            zombiesPerPhase1 = zombiesToSpawn;
        }
        else
        {
            zombiesToSpawn = zombiesPerPhase1;
        }

        Debug.Log("=== Bắt đầu Wave " + currentWave + " - Phase " + phase + " với " + zombiesToSpawn + " zombie ===");

        bool spawnBoss = (currentWave % 5 == 0);

        if (overrideCounts)
        {
            walkCount = testWalkCount;
            runCount = testRunCount;
            bossCount = testBossCount;
        }
        else
        {
            walkCount = Mathf.RoundToInt(zombiesToSpawn * 0.6f);
            runCount = zombiesToSpawn - walkCount;
            bossCount = spawnBoss ? 1 : 0;
        }

        List<GameObject> spawnList = new List<GameObject>();

        for (int i = 0; i < bossCount; i++)
        {
            if (zombieBossPrefabs.Length > 0)
            {
                GameObject bossPrefab = zombieBossPrefabs[Random.Range(0, zombieBossPrefabs.Length)];
                spawnList.Add(bossPrefab);
            }
        }

        for (int i = 0; i < walkCount; i++)
        {
            GameObject prefab = zombieWalkPrefabs[Random.Range(0, zombieWalkPrefabs.Length)];
            spawnList.Add(prefab);
        }

        for (int i = 0; i < runCount; i++)
        {
            GameObject prefab = zombieRunPrefabs[Random.Range(0, zombieRunPrefabs.Length)];
            spawnList.Add(prefab);
        }

        zombiesToSpawnThisPhase = spawnList.Count;
        zombiesSpawned = 0;

        List<int> usedLanes = new List<int>();

        foreach (GameObject prefab in spawnList)
        {
            int laneIndex;
            do
            {
                laneIndex = Random.Range(0, lanes.Length);
            } while (usedLanes.Contains(laneIndex));

            usedLanes.Add(laneIndex);

            Vector3 basePos = lanes[laneIndex].position;
            float offset = Random.Range(-horizontalOffsetRange, horizontalOffsetRange);
            Vector3 spawnPos = basePos + new Vector3(0f, 0f, offset);

            GameObject zombie = Instantiate(prefab, spawnPos, Quaternion.identity);

            ZombieMovementWithAnim zm = zombie.GetComponent<ZombieMovementWithAnim>();
            if (zm != null && laneTargets.Length > laneIndex)
            {
                zm.target = laneTargets[laneIndex];
                zm.maxHealth += healthBonus;
                zm.startHealth += healthBonus;
                zm.sharedPlayerHealth = sharedPlayerHealth;

                zm.GetComponent<Health>().OnDeath += () =>
                {
                    aliveZombies--;
                    Debug.Log("Zombie chết, còn lại: " + aliveZombies);

                    if (aliveZombies <= 0 && zombiesSpawned >= zombiesToSpawnThisPhase)
                    {
                        OnPhaseEnd();
                    }
                };
                zombieUIManager.AddZombieIcon(zm.zombieData);
            }

            aliveZombies++;
            zombiesSpawned++;

            yield return new WaitForSeconds(interval);

            if (usedLanes.Count >= lanes.Length)
                usedLanes.Clear();
        }

        spawning = false;

        if (aliveZombies <= 0)
        {
            OnPhaseEnd();
        }
    }

    private void OnPhaseEnd()
    {
        if (phase == 1)
        {
            waveMessageUI?.ShowMessage("A huge wave of zombie is approaching!");
            phase = 2;
            StartCoroutine(StartPhase2Delay()); // gọi coroutine delay
        }
        else
        {
            waveEnded = true;
            phase = 1;
            waveMessageUI?.ShowMessage("=== Kết thúc Wave " + currentWave + " ===");
        }
    }

    private void UpdateZombieCountByWave()
    {
        zombiesPerWave = currentWave * 5;
    }
    private IEnumerator StartPhase2Delay()
    {
        // đợi 3 giây cho chữ hiển thị
        yield return new WaitForSeconds(3f);

        waveMessageUI?.ShowMessage("Final wave!");

        // đợi thêm 2 giây nữa cho chữ "Final wave!" hiển thị rõ
        yield return new WaitForSeconds(2f);

        // sau khi chữ đã hiển thị xong thì mới spawn zombie
        StartCoroutine(StartWavePhase(spawnInterval * 0.5f));
    }
}