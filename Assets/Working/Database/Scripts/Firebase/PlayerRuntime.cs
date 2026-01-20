using UnityEngine;
using System.Threading.Tasks;

public class PlayerRuntime : MonoBehaviour
{
    public static PlayerRuntime Instance;
    public Player Player;
    public string AccountKey;

    private FirebaseService _firebase;

    private float _saveTimer = 0f;
    private const float SAVE_INTERVAL = 1f;

    private bool _isSaving = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Player = new Player();

            _firebase = new FirebaseService(
                "https://last-tower-3d-game-project-default-rtdb.asia-southeast1.firebasedatabase.app/");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        _saveTimer += Time.deltaTime;

        if (_saveTimer >= SAVE_INTERVAL)
        {
            _saveTimer = 0f;
            _ = SavePlayer(); // fire & forget
        }
    }

    private async void OnApplicationQuit()
    {
        await SavePlayer();
    }

    private async void OnDestroy()
    {
        if (Instance == this)
            await SavePlayer();
    }

    public async Task SavePlayer()
    {
        if (Player == null) return;
        if (string.IsNullOrEmpty(AccountKey)) return;
        if (_isSaving) return; // ? CH?N SAVE CH?NG

        _isSaving = true;

        PlayerData data = PlayerDataConverter.ToData(Player);
        await _firebase.SavePlayer(AccountKey, data);

        _isSaving = false;
    }
}
