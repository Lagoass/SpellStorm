using UnityEngine;

public enum EnemyType { Melee, Ranged } // O Enum

[CreateAssetMenu(menuName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Identidade")]
    public string enemyName;
    public EnemyType enemyType = EnemyType.Melee; // Define o comportamento
    public int spawnCost = 10;
    
    [Header("Base Stats")]
    [Range(0, 1000)] public float baseHealth = 100f;
    [Range(0f, 20f)] public float baseMoveSpeed;
    [Range(0f, 20f)] public float baseDamage; 
    [Range(1f, 50f)] public float baseSize;
    [Range(0f, 100f)] public float baseAttackRate; 
    [Range(0f, 100f)] public int baseMass;

    [Header("Ranged Stats (Apenas Ranged)")]
    public string projectileType; 
    public float attackRange = 8f;
    public float keepDistance = 5f;
    public float chargeTime = 1.0f;
    public float projectileSpeed = 6f;
    public GameObject preAttackVisual; 

    [Header("Scaling")]
    public float healthPerLevel = 10f;
    public float damagePerLevel = 2f;

    [Header("Runtime")]
    [HideInInspector] public float health; 
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;
    [HideInInspector] public int mass;

    public void InitializeForWave(int waveLevel)
    {
        health = baseHealth + (healthPerLevel * (waveLevel - 1));
        damage = baseDamage + (damagePerLevel * (waveLevel - 1));
        
        moveSpeed = baseMoveSpeed;
        size = baseSize;
        attackRate = baseAttackRate;
        mass = baseMass;
    }
}