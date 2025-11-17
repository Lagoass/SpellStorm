using UnityEngine;
using System.Collections.Generic; // Necessário para List<T>
using System.Linq; // Necessário para OrderBy
using System.Diagnostics; // Necessário para Stopwatch

public class ProjectileController : MonoBehaviour
{
    [Header("Spell Pool")]
    public ObjectPool pool;
    public MagicStats Stats;
    
    [Header("Settings")]
    public int CheckRadius = 5;
    public int DespawnRadius = 10;
    public bool ShowGizmos = true;
    
    private Transform player;
    private Stopwatch shootStopwatch = new Stopwatch(); 
    
    // Não precisamos mais de 'currentTarget' aqui, ele é buscado e usado localmente

    void Start()
    {
        shootStopwatch.Restart();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    void Update()
    {
        if (Stats == null || Stats.isActive == false) return;
        if (pool == null) return;
        if (player == null) return;

        // 1. Checagem de Cooldown
        if (shootStopwatch.ElapsedTimeSec() < Stats.projectileMagicStats.attackRate) return;
        
        // 2. Buscar TODOS os alvos (não apenas um)
        List<Transform> targets = GetTargets();
        if (targets.Count == 0) return; // Se não há alvos, não atira (e não reinicia o cooldown)

        // 3. Dispara múltiplos projéteis
        ShootProjectiles(targets); 
    }

    // --- FUNÇÃO GETTARGETS (MODIFICADA) ---
    // Agora retorna uma LISTA ordenada de alvos
    public List<Transform> GetTargets()
    {
        List<Transform> enemiesFound = new List<Transform>();
        Collider2D[] Enemies = Physics2D.OverlapCircleAll(player.transform.position, CheckRadius);

        foreach (Collider2D enemy in Enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemiesFound.Add(enemy.transform);
            }
        }

        // Ordena a lista pela distância (do mais próximo ao mais distante)
        if (enemiesFound.Count > 1)
        {
            enemiesFound = enemiesFound.OrderBy(
                enemy => Vector2.Distance(player.transform.position, enemy.position)
            ).ToList();
        }

        return enemiesFound;
    }

    // --- FUNÇÃO SHOOTPROJECTILES (MODIFICADA) ---
    // Agora faz o loop e aplica a lógica cíclica
    public void ShootProjectiles(List<Transform> targets)
    {
        // Pega a quantidade do SO (que foi resetado ou aumentado pelo LevelUp)
        int amount = Stats.projectileMagicStats.projectileAmount;

        for (int i = 0; i < amount; i++)
        {
            GameObject projectileGO = pool.GetPooledObject();
            if (projectileGO == null) continue; // Pula se o pool estiver vazio

            ProjectileBase projectile = projectileGO.GetComponent<ProjectileBase>();
            if (projectile == null) continue;

            // --- LÓGICA CÍCLICA (O "Looping") ---
            // i % targets.Count
            // 0 % 2 = 0 (Alvo A)
            // 1 % 2 = 1 (Alvo B)
            // 2 % 2 = 0 (Alvo A)
            // 3 % 2 = 1 (Alvo B)
            Transform target = targets[i % targets.Count];
            // --- FIM DA LÓGICA ---

            // Injeta todos os dados no projétil
            var stats = Stats.projectileMagicStats; 
            projectile.currentTarget = target; 
            projectile.currentSpeed = stats.speed;
            projectile.currentDamage = stats.damage;
            projectile.currentSize = stats.size;
            projectile.despawnDistance = DespawnRadius;
            projectile.currentSpread = stats.projectileSpread;
            projectile.hasImpactEffect = stats.hasImpactEffect;
            projectile.impactEffectPrefab = stats.impactEffectPrefab;
            projectile.effectDuration = stats.EffectDuration;
            projectile.effectDamageRadius = stats.EffectDamageRadius;
            projectile.effectDamage = stats.EffectDamage;
            projectile.effectSize = stats.EffectSize;
            
            projectileGO.transform.position = player.transform.position;
            projectileGO.SetActive(true);
        }

        // Reinicia o cooldown DEPOIS que todos os projéteis foram disparados
        shootStopwatch.Restart();
    }
    
    void OnDrawGizmos()
    {
        if (!ShowGizmos || player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, CheckRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.transform.position, DespawnRadius);
    }
}