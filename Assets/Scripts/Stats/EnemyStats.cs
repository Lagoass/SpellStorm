using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats (Configurable in Inspector)")]
    public string enemyName;
    [Range(0, 1000)]
    public float baseStartingHealth = 100f;
    [Range(0, 1000)]
    public float baseHealth = 100f;
    [Range(0f, 20f)]
    public float baseMoveSpeed;
    [Range(0f, 20f)]
    public float baseDamage;
    [Range(1f, 50f)]
    public float baseSize;
    [Range(0f, 100f)]
    public float baseAttackRate;
    [Range(0f, 50f)]
    public float baseValue;
    [Range(0f, 100f)]
    public int baseMass;

    [Header("Runtime Stats (Usado pelo jogo, não mexa)")]
    [HideInInspector] public float startingHealth;
    [HideInInspector] public float health; // Era 'health', agora está oculto
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;
    [HideInInspector] public float value;
    [HideInInspector] public int mass;
    public void ResetStats()
    {
        startingHealth = baseStartingHealth;
        health = baseHealth;
        moveSpeed = baseMoveSpeed;
        damage = baseDamage;
        size = baseSize;
        attackRate = baseAttackRate;
        value = baseValue;
        mass = baseMass;
    }
}