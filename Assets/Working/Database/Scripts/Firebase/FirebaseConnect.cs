using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseConnect : MonoBehaviour
{

    private const string firebaseUrl =
        "https://last-tower-3d-game-project-default-rtdb.asia-southeast1.firebasedatabase.app/";

    private FirebaseService _firebase;

    private void Awake()
    {
        _firebase = new FirebaseService(firebaseUrl);
    }

    // ================= UI BUTTON =================

    public GameObject loginPanel;
    public GameObject gamePanel;

    public async void OnSignUpButtonClick()
    {
        await SignUp();
    }

    public async void OnSignInButtonClick()
    {
        await SignIn();
    }

    public void StartGameClick()
    {
        SceneManager.LoadScene("LoadScene");
    }

    public void ExitGameClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ================= CORE LOGIC =================

    private async Task SignUp()
    {
        string username = GetUsername();
        string password = GetPassword();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Username or password is empty");
            return;
        }

        var accounts = await _firebase.GetAccounts();

        if (accounts.Any(a => a.Username == username))
        {
            ShowMessage("Account already exists");
            ClearInput();
            return;
        }

        AccountData account = AccountFactory.Create(username, password);
        await _firebase.CreateAccount(account);

        ShowMessage("Sign up success");
        //ClearInput();
        GetInputField("InputPassword").text = "";

        await Task.Delay(2000);

        ShowMessage("");
    }

    private async Task SignIn()
    {
        string username = GetUsername();
        string password = GetPassword();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Username or password is empty");
            return;
        }

        var accounts = await _firebase.GetAccountSnapshots();

        var snapshot = accounts.FirstOrDefault(a =>
            a.Object.Username == username &&
            a.Object.Password == password);

        if (snapshot == null)
        {
            ShowMessage("Username or password incorrect");
            return;
        }

        // ✅ GÁN ACCOUNT KEY
        PlayerRuntime.Instance.AccountKey = snapshot.Key;

        // ✅ LOAD PLAYER
        PlayerRuntime.Instance.Player.LoadFromData(snapshot.Object.Player);

        ShowMessage("Sign in successful");

        await Task.Delay(1000);        

        ShowMessage("");

        loginPanel.SetActive(false);

        gamePanel.SetActive(true);

        //SceneManager.LoadScene("GameScene");
    }


    // ================= HELPERS =================

    private string GetUsername()
    {
        return GameObject.Find("Canvas")
            .transform.Find("LoginPanel")
            .transform.Find("InputUsername")
            .GetComponent<TMP_InputField>().text.Trim();
    }

    private string GetPassword()
    {
        return GameObject.Find("Canvas")
            .transform.Find("LoginPanel")
            .transform.Find("InputPassword")
            .GetComponent<TMP_InputField>().text.Trim();
    }

    private void ClearInput()
    {
        GetInputField("InputUsername").text = "";
        GetInputField("InputPassword").text = "";
    }

    private TMP_InputField GetInputField(string name)
    {
        return GameObject.Find("Canvas")
            .transform.Find("LoginPanel")
            .transform.Find(name)
            .GetComponent<TMP_InputField>();
    }

    private void ShowMessage(string message)
    {
        GameObject.Find("Canvas")
            .transform.Find("Text")
            .GetComponent<TextMeshProUGUI>().text = message;
    }

}

