using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZombieManager : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject zombieWalkPrefab;
    public GameObject zombieRunPrefab;
    public GameObject zombieBossPrefab;

    [Header("Spawn Lanes")]
    public Transform[] lanes; // 4 lane spawn

    [Header("Wave Settings")]
    public float spawnInterval = 2.5f;
    private int currentWave = 1;
    private int zombiesPerWave = 5;

    private bool spawning = false;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        spawning = true;
        int zombiesToSpawn = zombiesPerWave;

        Debug.Log("=== Bắt đầu Wave " + currentWave + " với " + zombiesToSpawn + " zombie ===");

        // Nếu wave là bội số của 5 thì thêm boss
        bool spawnBoss = (currentWave % 5 == 0);

        // Tính số lượng Walk/Run
        int walkCount = Mathf.RoundToInt(zombiesToSpawn * 0.6f); // 60% đi bộ
        int runCount = zombiesToSpawn - walkCount;               // còn lại chạy

        // Tạo danh sách spawn
        List<GameObject> spawnList = new List<GameObject>();

        // Nếu wave có boss thì thêm boss đầu tiên
        if (spawnBoss)
        {
            spawnList.Add(zombieBossPrefab);
            zombiesToSpawn--; // boss chiếm 1 slot
            walkCount = Mathf.RoundToInt(zombiesToSpawn * 0.6f);
            runCount = zombiesToSpawn - walkCount;
        }

        // Thêm Walk trước
        for (int i = 0; i < walkCount; i++)
            spawnList.Add(zombieWalkPrefab);

        // Thêm Run sau
        for (int i = 0; i < runCount; i++)
            spawnList.Add(zombieRunPrefab);

        // Danh sách lane đã dùng
        List<int> usedLanes = new List<int>();

        // Spawn lần lượt theo danh sách
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

        Debug.Log("=== Kết thúc Wave " + currentWave + " ===");

        currentWave++;
        zombiesPerWave += 5;

        yield return new WaitForSeconds(5f);
        StartCoroutine(StartWave());
    }
}