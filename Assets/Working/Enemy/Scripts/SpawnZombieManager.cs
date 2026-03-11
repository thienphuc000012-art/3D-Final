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
    private int zombiesPerPhase1;

    [Header("UI Settings")]
    public WaveMessageUI waveMessageUI;
    public ZombieUIManager zombieUIManager;

    [Header("Wave UI")]
    public WaveUIManager waveUIManager;

    [Header("Wave Control UI")]
    public GameObject nextWavePanel; 

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip zombieComingClip;

    public AudioSource musicSource;
    public AudioClip backgroundMusicClipPhase1; 
    public AudioClip backgroundMusicClipPhase2;

    
    
    
    //-----------------------------------
    private void Awake()
    {
        currentWave = PlayerRuntime.Instance.Player.Wave;
    }
    //-----------------------------------


    void Start()
    {
        UpdateZombieCountByWave();
        StartCoroutine(StartWavePhase(spawnInterval));
    }


    //-----------------------------------
    void Update()
    {
        PlayerRuntime.Instance.Player.Wave = currentWave;
    }
    //-----------------------------------

    public void StartNextWave()
    {
        if (nextWavePanel != null)
            nextWavePanel.SetActive(false);

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

    IEnumerator StartWavePhase(float interval)
    {
       if (phase == 1 && audioSource != null && zombieComingClip != null)
    {
        audioSource.PlayOneShot(zombieComingClip);
    }



        if (phase == 1 && musicSource != null && backgroundMusicClipPhase1 != null)
        {
            musicSource.clip = backgroundMusicClipPhase1;
            musicSource.loop = true;
            musicSource.Play();
        }
        else if (phase == 2 && musicSource != null && backgroundMusicClipPhase2 != null)
        {
            musicSource.clip = backgroundMusicClipPhase2;
            musicSource.loop = true;
            musicSource.Play();
        }

        spawning = true;

        int zombiesToSpawn;


        if (phase == 1)
        {
            zombiesToSpawn = zombiesPerWave;
            zombiesPerPhase1 = zombiesToSpawn;
            int totalZombiesInWave = zombiesPerPhase1 * 2;

            waveUIManager.InitWave(currentWave, totalZombiesInWave);

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
            waveUIManager.OnZombieSpawned();


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
            if (audioSource != null && zombieComingClip != null)
            {
                audioSource.PlayOneShot(zombieComingClip);
            }

            StartCoroutine(StartPhase2Delay()); 
        }
        else
        {
            waveEnded = true;
            phase = 1;
            waveMessageUI?.ShowMessage("=== Kết thúc Wave " + currentWave + " ===");
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
            if (nextWavePanel != null)
                nextWavePanel.SetActive(true);


        }
    }

    private void UpdateZombieCountByWave()
    {
        zombiesPerWave = currentWave * 5;
    }
    private IEnumerator StartPhase2Delay()
    {

        yield return new WaitForSeconds(3f);

        waveMessageUI?.ShowMessage("Final wave!");

        yield return new WaitForSeconds(2f);

        StartCoroutine(StartWavePhase(spawnInterval * 0.5f));
    }
}