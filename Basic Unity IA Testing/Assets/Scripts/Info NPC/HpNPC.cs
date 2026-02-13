using UnityEngine;

public class HpNPC : MonoBehaviour
{
    [Header("HP")]
    public float maxHp = 100f;
    [SerializeField] private float hp = 100f;

    public float getHP() => hp;
    public float getMaxHP() => maxHp;

    public void reduceHp(float damage)
    {
        hp -= damage;
        if (hp < 0f) hp = 0f;
    }
}
