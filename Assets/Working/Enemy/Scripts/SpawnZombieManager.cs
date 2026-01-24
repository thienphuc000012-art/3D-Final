using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [Tooltip("Số lượng zombie Walk trong wave hiện tại")]
    public int walkCount;
    [Tooltip("Số lượng zombie Run trong wave hiện tại")]
    public int runCount;
    [Tooltip("Số lượng zombie Boss trong wave hiện tại")]
    public int bossCount;

    [Header("Spawn Settings")]
    [Tooltip("Khoảng lệch ngang tối đa khi spawn để tránh chồng lên nhau")]
    public float horizontalOffsetRange = 1.5f;

    [Header("Test Settings")]
    public bool overrideCounts = false;   // nếu bật thì dùng số lượng nhập tay
    public int testWalkCount = 0;
    public int testRunCount = 0;
    public int testBossCount = 0;


    private bool spawning = false;
    private bool waveEnded = false;
    private int healthBonus = 0;

    [Header("Player Settings")]
    public PlayerHealth sharedPlayerHealth; // kéo thả Player vào đây
    void Start()
    {
        UpdateZombieCountByWave();
        StartCoroutine(StartWave());
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

            StartCoroutine(StartWave());
        }
    }

    IEnumerator StartWave()
    {
        spawning = true;
        int zombiesToSpawn = zombiesPerWave;

        Debug.Log("=== Bắt đầu Wave " + currentWave + " với " + zombiesToSpawn + " zombie ===");

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

            }

            Debug.Log("Spawn zombie " + prefab.name + " tại lane " + laneIndex + " offset Z: " + offset);

            yield return new WaitForSeconds(spawnInterval);

            if (usedLanes.Count >= lanes.Length)
                usedLanes.Clear();
        }

        spawning = false;
        waveEnded = true;

        Debug.Log("=== Kết thúc Wave " + currentWave + " ===");
    }
    private void UpdateZombieCountByWave()
    {
        // mỗi wave tăng thêm 5 zombie
        zombiesPerWave = currentWave * 5;
    }
}