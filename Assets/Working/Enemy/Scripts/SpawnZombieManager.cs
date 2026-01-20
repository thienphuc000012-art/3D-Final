using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnZombieManager : MonoBehaviour
{
    [Header("Zombie Prefabs Walk/Run")]
    public GameObject[] zombieWalkPrefabs; // nhiều loại Walk
    public GameObject[] zombieRunPrefabs;  // nhiều loại Run
    public GameObject zombieBossPrefab;

    [Header("Spawn Lanes")]
    public Transform[] lanes; // 4 lane spawn

    [Header("Wave Settings")]
    public float spawnInterval = 2.5f;
    private int currentWave = 1;
    private int zombiesPerWave = 5;

    private bool spawning = false;
    private bool waveEnded = false;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    void Update()
    {
        if (waveEnded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            waveEnded = false;
            currentWave++;
            zombiesPerWave += 5;
            StartCoroutine(StartWave());
        }
    }

    IEnumerator StartWave()
    {
        spawning = true;
        int zombiesToSpawn = zombiesPerWave;

        Debug.Log("=== Bắt đầu Wave " + currentWave + " với " + zombiesToSpawn + " zombie ===");

        bool spawnBoss = (currentWave % 5 == 0);

        int walkCount = Mathf.RoundToInt(zombiesToSpawn * 0.6f);
        int runCount = zombiesToSpawn - walkCount;

        List<GameObject> spawnList = new List<GameObject>();

        if (spawnBoss)
        {
            spawnList.Add(zombieBossPrefab);
            zombiesToSpawn--;
            walkCount = Mathf.RoundToInt(zombiesToSpawn * 0.6f);
            runCount = zombiesToSpawn - walkCount;
        }

        // Thêm Walk
        for (int i = 0; i < walkCount; i++)
        {
            // chọn ngẫu nhiên prefab Walk
            GameObject prefab = zombieWalkPrefabs[Random.Range(0, zombieWalkPrefabs.Length)];
            spawnList.Add(prefab);
        }

        // Thêm Run
        for (int i = 0; i < runCount; i++)
        {
            // chọn ngẫu nhiên prefab Run
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

            Instantiate(prefab, lanes[laneIndex].position, Quaternion.identity);
            Debug.Log("Spawn zombie " + prefab.name + " tại lane " + laneIndex);

            yield return new WaitForSeconds(spawnInterval);

            if (usedLanes.Count >= lanes.Length)
                usedLanes.Clear();
        }

        spawning = false;
        waveEnded = true;

        Debug.Log("=== Kết thúc Wave " + currentWave + " ===");
    }
}