using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)] public float zombieVolume = 0.7f;

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
}