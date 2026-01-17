using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string ID;
    public string Name;
    public int Wave;    
    public int Exp;
    public int Lv;        
    public void LoadFromData(PlayerData data)
    {
        ID = data.ID;
        Name = data.Name;      
        Wave = data.Wave;
        Exp = data.Exp;
        Lv = data.Lv;                 
    }
}

