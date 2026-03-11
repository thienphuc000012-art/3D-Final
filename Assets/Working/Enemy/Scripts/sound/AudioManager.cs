using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)] public float zombieVolume = 0.7f;

    // Danh sách tất cả AudioSource của zombie
    private List<AudioSource> zombieSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetZombieVolume(float value)
    {
        zombieVolume = value;
    }

    // ✅ Đăng ký AudioSource zombie
    public void RegisterZombieSource(AudioSource source)
    {
        if (source != null && !zombieSources.Contains(source))
            zombieSources.Add(source);
    }

    // ✅ Hủy đăng ký khi zombie chết
    public void UnregisterZombieSource(AudioSource source)
    {
        if (source != null && zombieSources.Contains(source))
            zombieSources.Remove(source);
    }

    // ✅ Tắt tất cả âm thanh zombie
    public void StopAllZombieSounds()
    {
        foreach (var src in zombieSources)
        {
            if (src != null && src.isPlaying)
                src.Stop();
        }
    }
}