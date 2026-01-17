using System;
using System.Collections.Generic;
using UnityEngine;

public static class AccountFactory
{
    public static AccountData Create(string username, string password)
    {
        return new AccountData
        {
            Username = username,
            Password = password,
            Timecreate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            Player = CreateNewPlayer()
        };
    }

    private static PlayerData CreateNewPlayer()
    {
        PlayerData player = new PlayerData
        {
            ID = Guid.NewGuid().ToString(),
            Name = "NewPlayer",            
            Exp = 0,
            Lv = 1,
            Wave = 1,
            IsOnline = false,
            LastLogin = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),            
        };        

        return player;
    }
}

