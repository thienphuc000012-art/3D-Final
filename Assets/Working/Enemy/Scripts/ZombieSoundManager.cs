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

    [Range(0f, 1f)] public float volume = 0.7f;

    void Awake()
    {
        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.spatialBlend = 1f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 1f;
        }
    }

    public void PlayWalk()
    {
        if (walkClip != null)
        {
            loopSource.clip = walkClip;
            loopSource.volume = volume;
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
            sfxSource.PlayOneShot(attackClip, volume);
    }

    public void PlayHit()
    {
        StopWalk();

        if (hitClip != null)
            sfxSource.PlayOneShot(hitClip, volume);
    }

    public void PlayDeath()
    {
        StopWalk();
        sfxSource.Stop(); 

        if (deathClip != null)
            sfxSource.PlayOneShot(deathClip, volume);
    }
}