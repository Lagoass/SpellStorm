using UnityEngine;
using System.Collections.Generic;

public class EnemyProjectileController : MonoBehaviour
{
    // --- OTIMIZAÇÃO: Lista Telefônica Global ---
    // Permite que o inimigo encontre o controlador instantaneamente sem FindObjectsOfType
    public static Dictionary<EnemyStats, EnemyProjectileController> ControllerMap = new Dictionary<EnemyStats, EnemyProjectileController>();

    [Header("Configuração do Pool Único")]
    [Tooltip("O EnemyStats específico que este controller gerencia.")]
    public EnemyStats projectileStatsSource; 
    
    [Tooltip("O ObjectPool específico para este tipo de projétil.")]
    public ObjectPool projectilePool;

    void Awake()
    {
        // Se registra na lista global
        if (projectileStatsSource != null && !ControllerMap.ContainsKey(projectileStatsSource))
        {
            ControllerMap.Add(projectileStatsSource, this);
        }
    }

    void OnDestroy()
    {
        // Limpa registro ao sair
        if (projectileStatsSource != null && ControllerMap.ContainsKey(projectileStatsSource))
        {
            ControllerMap.Remove(projectileStatsSource);
        }
    }

    // Assinatura atualizada para passar Size e Range
    public void SpawnProjectile(Vector3 position, Vector2 direction, float speed, float damage, float size, float range)
    {
        if (projectilePool == null) return;

        GameObject projGO = projectilePool.GetPooledObject();
        
        if (projGO != null)
        {
            projGO.transform.position = position;
            projGO.SetActive(true);

            EnemyProjectile projectile = projGO.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                // Agora chama o Init com todos os dados
                projectile.Init(damage, speed, direction, size, range);
            }
        }
    }
}