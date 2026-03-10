using UnityEngine;

public class ZombieSoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource loopSource;   
    public AudioSource sfxSource;    

    [Header("Zombie Sounds")]
    public AudioClip walkClip;
    public AudioClip attackClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    void Awake()
    {
        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.spatialBlend = 1f;
            AudioManager.Instance.RegisterZombieSource(loopSource);

        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 1f;
            AudioManager.Instance.RegisterZombieSource(sfxSource);

        }
    }
    void OnDestroy()
    {
        if (loopSource != null) AudioManager.Instance.UnregisterZombieSource(loopSource);
        if (sfxSource != null) AudioManager.Instance.UnregisterZombieSource(sfxSource);
    }
    void Update()
    {
        if (loopSource != null)
            loopSource.volume = AudioManager.Instance.zombieVolume;

        if (sfxSource != null)
            sfxSource.volume = AudioManager.Instance.zombieVolume;
    }

    public void PlayWalk()
    {
        if (walkClip != null)
        {
            loopSource.clip = walkClip;
            loopSource.volume = AudioManager.Instance.zombieVolume;
            loopSource.Play();
        }
    }

    public void StopWalk()
    {
        loopSource.Stop();
    }

    public void PlayAttack()
    {
        StopWalk();
        if (attackClip != null)
            sfxSource.PlayOneShot(attackClip, AudioManager.Instance.zombieVolume);
    }

    public void PlayHit()
    {
        StopWalk();
        if (hitClip != null)
            sfxSource.PlayOneShot(hitClip, AudioManager.Instance.zombieVolume);
    }

    public void PlayDeath()
    {
        StopWalk();
        sfxSource.Stop(); 
        if (deathClip != null)
            sfxSource.PlayOneShot(deathClip, AudioManager.Instance.zombieVolume);
    }
}