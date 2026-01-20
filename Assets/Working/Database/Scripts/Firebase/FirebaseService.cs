using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using UnityEngine;

public class FirebaseService
{
    private readonly FirebaseClient _client;

    public FirebaseService(string url)
    {
        _client = new FirebaseClient(url);
    }

    // ================= SIGN UP =================
    // Ch? c?n data, không c?n key
    public async Task<List<AccountData>> GetAccounts()
    {
        return (await _client
                .Child("Accounts")
                .OnceAsync<AccountData>())
            .Select(a => a.Object)
            .ToList();
    }

    public async Task CreateAccount(AccountData account)
    {
        await _client
            .Child("Accounts")
            .Child($"Acc_{account.Timecreate}")
            .PutAsync(account);
    }

    // ================= SIGN IN =================
    // C?n c? key + data
    public async Task<List<FirebaseObject<AccountData>>> GetAccountSnapshots()
    {
        return (List<FirebaseObject<AccountData>>)await _client
            .Child("Accounts")
            .OnceAsync<AccountData>();
    }

    // ================= SAVE PLAYER =================
    public async Task SavePlayer(string accountKey, PlayerData player)
    {
        if (string.IsNullOrEmpty(accountKey))
        {
            Debug.LogError("SavePlayer failed: accountKey is null");
            return;
        }

        await _client
            .Child("Accounts")
            .Child(accountKey)
            .Child("Player")
            .PutAsync(player);
    }
}
