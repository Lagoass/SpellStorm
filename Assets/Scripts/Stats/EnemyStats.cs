using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats (Configurable in Inspector)")]
    public string enemyName;
    [Tooltip("Quanto este inimigo 'custa' para o orçamento (saldo) da onda.")]
    public int spawnCost = 10;
    
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
    [Range(0f, 100f)]
    public int baseMass;

    [Header("Scaling Per Level (Stats por Nível)")]
    [Tooltip("Vida extra que o inimigo ganha por nível de onda (além do Nível 1).")]
    public float healthPerLevel = 10f;
    [Tooltip("Dano extra que o inimigo ganha por nível de onda.")]
    public float damagePerLevel = 2f;

    [Header("Runtime Stats (Usado pelo jogo, não mexa)")]
    [HideInInspector] public float health; 
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;
    [HideInInspector] public int mass;

    // Esta função substitui a ResetStats() e calcula o escalonamento
    public void InitializeForWave(int waveLevel)
    {
        // Nível 1 não recebe bônus (waveLevel - 1 = 0)
        health = baseHealth + (healthPerLevel * (waveLevel - 1));
        damage = baseDamage + (damagePerLevel * (waveLevel - 1));
        
        // Reseta o resto dos stats para o padrão
        moveSpeed = baseMoveSpeed;
        size = baseSize;
        attackRate = baseAttackRate;
        mass = baseMass;
    }
}