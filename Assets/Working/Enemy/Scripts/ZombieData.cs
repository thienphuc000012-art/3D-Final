using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Game/Zombie Data")]
public class ZombieData : ScriptableObject
{
    public string zombieName;
    public Sprite zombieSprite;
    public int maxHealth;
    public float moveSpeed;
    public int damage;
    [TextArea]
    public string description;
}