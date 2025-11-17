using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Base Player Stats (Configurado no Inspector)")]
    public float baseStartingHealth = 100f;
    public float baseHealth = 100f;
    public float baseHealthRegen = 1f;
    public float baseMoveSpeed = 2f;
    public float baseSize = 2.5f;
    public float baseLuck = 1f;
    public float baseMass = 750f;
    public float basePickupRadius = 3f;
    public float baseXpGains = 1f;

    [Header("Runtime Stats (Usados pelo Jogo)")]
    [HideInInspector] public float startingHealth;
    [HideInInspector] public float health;
    [HideInInspector] public float healthRegen;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float size;
    [HideInInspector] public float luck;
    [HideInInspector] public float mass;
    [HideInInspector] public float pickupRadius;
    [HideInInspector] public float XpGains;

    [Header("Runtime Experience (Usados pelo Jogo)")]
    public float experience = 0f;
    public int level = 1;

    public void ResetStats()
    {
        startingHealth = baseStartingHealth; 
        health = baseHealth;         
        healthRegen = baseHealthRegen;
        moveSpeed = baseMoveSpeed;
        size = baseSize;
        luck = baseLuck;
        mass = baseMass;
        pickupRadius = basePickupRadius;
        XpGains = baseXpGains;

        // Reseta os contadores
        experience = 0f;
        level = 1;
    }
}