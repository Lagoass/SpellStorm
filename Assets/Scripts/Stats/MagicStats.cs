using UnityEngine;
using System;

// ----------------------------------------------------
// 1. Define the Serializable Stat Classes
// ----------------------------------------------------

[Serializable]
public class ProjectileMagicStats
{
    [Header("Base Stats (Configurable)")]
    public float baseSpeed = 10f;
    public float baseDamage = 5f;
    public float baseSize = 1f;
    public float baseAttackRate = 0.5f;
    public int baseProjectileAmount = 1;
    public float baseProjectileSpread = 0f;
    
    [Header("Effect Base Settings")]
    public bool baseHasImpactEffect = false;
    public GameObject impactEffectPrefab; 
    
    [Range(0f, 5f)] public float baseEffectDuration = 1f;
    [Range(0f, 10f)] public float baseEffectDamageRadius = 2f;
    [Range(0f, 100f)] public float baseEffectDamage = 10f;
    [Range(0f, 5f)] public float baseEffectSize = 1f;

    // Runtime Stats
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;
    [HideInInspector] public int projectileAmount;
    [HideInInspector] public float projectileSpread;
    [HideInInspector] public bool hasImpactEffect;
    [HideInInspector] public float EffectDuration;
    [HideInInspector] public float EffectDamageRadius;
    [HideInInspector] public float EffectDamage;
    [HideInInspector] public float EffectSize;

    public void Init()
    {
        speed = baseSpeed;
        damage = baseDamage;
        size = baseSize;
        attackRate = baseAttackRate;
        projectileAmount = baseProjectileAmount;
        projectileSpread = baseProjectileSpread;
        hasImpactEffect = baseHasImpactEffect;
        EffectDuration = baseEffectDuration;
        EffectDamageRadius = baseEffectDamageRadius;
        EffectDamage = baseEffectDamage;
        EffectSize = baseEffectSize;
        
    }
}

[Serializable]
public class AroundPlayerMagicStats
{
    // ... (Sem alterações aqui)
    [Header("Base Stats (Configurable)")]
    public float baseRadius = 3f;
    public float baseDamage = 10f;
    public float baseSize = 2f;
    public float baseAttackRate = 1f;

    // Runtime Stats
    [HideInInspector] public float radius;
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;

    public void Init()
    {
        radius = baseRadius;
        damage = baseDamage;
        size = baseSize;
        attackRate = baseAttackRate;

    }
}

[Serializable]
public class AtTargetMagicStats
{
    // ... (Sem alterações aqui)
    [Header("Base Stats (Configurable)")]
    public float baseDamage = 15f;
    public float baseSize = 1f;
    public float baseAttackRate = 2f;

    // Runtime Stats
    [HideInInspector] public float damage;
    [HideInInspector] public float size;
    [HideInInspector] public float attackRate;

    public void Init()
    {
        damage = baseDamage;
        size = baseSize;
        attackRate = baseAttackRate;
    }
}

// ----------------------------------------------------
// 2. The Main ScriptableObject
// ----------------------------------------------------

[CreateAssetMenu(menuName = "MagicStats")]
public class MagicStats : ScriptableObject
{
    public enum MagicType { Projectile, AroundPlayer, AtTarget }

    [Header("Magic Settings")]
    public MagicType magicType = MagicType.Projectile;
    public string magicName = "New Magic";
    public Sprite magicIcon;
    public int level = 1;
    public bool isActive = false;

    [Space(10)]
    [Header("All Magic Stats")]
    public ProjectileMagicStats projectileMagicStats = new ProjectileMagicStats();
    public AroundPlayerMagicStats aroundPlayerMagicStats = new AroundPlayerMagicStats();
    public AtTargetMagicStats atTargetMagicStats = new AtTargetMagicStats();

    public void ResetStats(bool startActive)
    {
        isActive = startActive;
        level = 1; 

        if (projectileMagicStats != null) projectileMagicStats.Init();
        if (aroundPlayerMagicStats != null) aroundPlayerMagicStats.Init();
        if (atTargetMagicStats != null) atTargetMagicStats.Init();
    }
}