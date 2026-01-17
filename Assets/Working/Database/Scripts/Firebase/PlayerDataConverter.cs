using System.Collections.Generic;
using UnityEngine;

public static class PlayerDataConverter
{
    public static PlayerData ToData(Player player)
    {
        PlayerData data = new PlayerData
        {
            ID = player.ID,
            Name = player.Name,            
            Exp = player.Exp,
            Lv = player.Lv,    
            Wave = player.Wave,
        };        

        return data;
    }
}
