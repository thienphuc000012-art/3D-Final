using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnZombieManager : MonoBehaviour
{
    [Header("Zombie Prefabs Walk/Run")]
    public GameObject[] zombieWalkPrefabs;
    public GameObject[] zombieRunPrefabs;
    public GameObject zombieBossPrefab;

    [Header("Spawn Lanes")]
    public Transform[] lanes;          // vị trí spawn
    public Transform[] laneTargets;    // target cho từng lane

    [Header("Wave Settings")]
    public float spawnInterval = 2.5f;
    public int currentWave = 1;        // hiển thị và chỉnh sửa được trong Inspector
    public int zombiesPerWave = 5;     // hiển thị và chỉnh sửa được trong Inspector

    private bool spawning = false;
    private bool waveEnded = false;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    void Update()
    {
        // Khi wave kết thúc, nhấn Space để bắt đầu wave mới
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
            GameObject prefab = zombieWalkPrefabs[Random.Range(0, zombieWalkPrefabs.Length)];
            spawnList.Add(prefab);
        }

        // Thêm Run
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

            GameObject zombie = Instantiate(prefab, lanes[laneIndex].position, Quaternion.identity);

            // Gán target theo lane
            ZombieMovementWithAnim zm = zombie.GetComponent<ZombieMovementWithAnim>();
            if (zm != null && laneTargets.Length > laneIndex)
            {
                zm.target = laneTargets[laneIndex];
            }

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