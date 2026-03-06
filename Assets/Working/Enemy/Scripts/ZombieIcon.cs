using UnityEngine;
using UnityEngine.EventSystems;

public class ZombieIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ZombieData zombieData;      
    public ZombieUIManager uiManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.ShowZombieInfoData(zombieData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.HideZombieInfo();
    }
}