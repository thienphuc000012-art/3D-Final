using UnityEngine;

public class AudioSon : MonoBehaviour
{
    public AudioSon Instance;

    [SerializeField] AudioSource bg;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        bg.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerRuntime.Instance.sceneIndex == 0)
        {
            if (!bg.isPlaying)
                bg.Play();
        }
        else
        {
            if (bg.isPlaying)
                bg.Stop();
        }
    }
}
