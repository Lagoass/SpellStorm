using UnityEngine;
using System.Collections.Generic;
// Removido TMPro pois o tempo é controlado pelo GameController

public class EnemySpawner : MonoBehaviour
{
    [Header("Referências")]
    public GameController gameController;

    [Header("Pools de Inimigos")]
    [Tooltip("A ordem deve bater com a lista de EnemyStats!")]
    public List<ObjectPool> enemyPools;
    public List<EnemyStats> enemyStatsList;

    [Header("Configurações Globais de Eventos")]
    [Tooltip("Raio de 'espalhamento' para o grupo do Batch.")]
    public float batchSpread = 1.5f;
    [Tooltip("Raios para o evento Circle.")]
    public float minCircleRadius = 8f;
    public float maxCircleRadius = 10f;

    [Header("Raio de Spawn (Gotejamento)")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 15f;
    public float despawnRadius = 20f;

    [Header("Debug")]
    [SerializeField] private int enemyCount = 0;
    public bool ShowGizmos = true;

    // --- Variáveis de Runtime ---
    private WaveConfig currentWaveConfig;
    private float nextTrickleSpawnTime;
    private float nextEventTriggerTime;
    private int currentTrickleBudget;
    
    // Internals
    Transform player;
    Rigidbody2D playerRb;
    readonly HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    Vector3 PlayerPos => player != null ? player.position : Vector3.zero;
    private Dictionary<EnemyStats, ObjectPool> enemyPoolMap;

    void Awake()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            playerRb = playerGO.GetComponent<Rigidbody2D>();
        }

        if (gameController == null) gameController = FindObjectOfType<GameController>();

        // Mapeamento para acesso rápido aos pools
        enemyPoolMap = new Dictionary<EnemyStats, ObjectPool>();
        if (enemyStatsList.Count != enemyPools.Count)
        {
            Debug.LogError("ERRO: Listas de Stats e Pools têm tamanhos diferentes!");
        }
        else
        {
            for (int i = 0; i < enemyStatsList.Count; i++)
                enemyPoolMap[enemyStatsList[i]] = enemyPools[i];
        }
    }

    // Chamado pelo GameController
    public void StartNewWave(WaveConfig config)
    {
        currentWaveConfig = config;
        
        // Reseta os timers
        nextTrickleSpawnTime = Time.time; 
        nextEventTriggerTime = Time.time + config.eventInterval;

        // O orçamento do gotejamento é o total da onda
        // (Eventos ignoram orçamento e usam apenas seus limites de min/max spawn)
        currentTrickleBudget = config.totalBudget;
    }

    void Update()
    {
        if (currentWaveConfig == null || gameController == null || !gameController.canSpawn) return;

        HandleTrickleSpawn();
        HandleEventSpawns();
        DespawnCheck();
    }

    void HandleTrickleSpawn()
    {
        if (Time.time < nextTrickleSpawnTime) return;
        if (currentTrickleBudget <= 0) return;
        if (currentWaveConfig.availableEnemies.Count == 0) return;
        
        // Verifica limite de inimigos (Gotejamento + Eventos somados, para performance)
        if (activeEnemies.Count >= currentWaveConfig.maxTrickleEnemies) return;

        EnemyStats enemyToSpawn = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];
        if (enemyToSpawn == null) return;

        if (currentTrickleBudget >= enemyToSpawn.spawnCost)
        {
            if (SpawnEnemy(enemyToSpawn, GetRandomSpawnPosition(), false, Vector2.zero, 0f))
            {
                currentTrickleBudget -= enemyToSpawn.spawnCost;
            }
        }
        nextTrickleSpawnTime = Time.time + currentWaveConfig.trickleSpawnInterval;
    }

    void HandleEventSpawns()
    {
        if (Time.time > nextEventTriggerTime)
        {
            TriggerRandomEvent();
            nextEventTriggerTime = Time.time + currentWaveConfig.eventInterval;
        }
    }

    void TriggerRandomEvent()
    {
        bool batch = currentWaveConfig.enableBatchEvents;
        bool circle = currentWaveConfig.enableAroundEvents;

        if (batch && circle)
        {
            if (Random.value > 0.5f) ExecuteBatchEvent(currentWaveConfig.batchSettings);
            else ExecuteCircleEvent(currentWaveConfig.circleSettings);
        }
        else if (batch) ExecuteBatchEvent(currentWaveConfig.batchSettings);
        else if (circle) ExecuteCircleEvent(currentWaveConfig.circleSettings);
    }

    void ExecuteBatchEvent(BatchSettings settings)
    {
        if (player == null || currentWaveConfig.availableEnemies.Count == 0) return;

        // 1. Escolhe um ponto aleatório perto do jogador (dentro do anel)
        Vector2 spawnCenter = GetRandomSpawnPosition();

        // 2. Calcula a direção para o jogador
        Vector2 batchMoveDir = ((Vector2)PlayerPos - spawnCenter).normalized;

        // 3. Sorteia quantidade
        int amountToSpawn = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);
        
        // Sorteia um tipo de inimigo para o bando inteiro
        EnemyStats enemyType = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];

        for (int i = 0; i < amountToSpawn; i++)
        {
            // 4. Cluster: Spawna perto do centro e deixa a física empurrar
            Vector2 clusterOffset = Random.insideUnitCircle * batchSpread;
            Vector2 spawnPos = spawnCenter + clusterOffset;

            GameObject enemy = SpawnEnemy(enemyType, spawnPos, true, batchMoveDir, settings.speedOverride);
        }
    }

    void ExecuteCircleEvent(CircleSettings settings)
    {
        if (player == null || currentWaveConfig.availableEnemies.Count == 0) return;

        float radius = Random.Range(minCircleRadius, maxCircleRadius);
        int amountToSpawn = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);
        if (amountToSpawn == 0) return;

        for (int i = 0; i < amountToSpawn; i++)
        {
            EnemyStats enemyType = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];
            
            float angle = i * (360f / amountToSpawn);
            Vector2 direction = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
            Vector2 spawnPos = (Vector2)PlayerPos + (direction * radius);

            GameObject enemy = SpawnEnemy(enemyType, spawnPos, false, Vector2.zero, 0f);
            if (enemy != null)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetSpeed(settings.speedOverride);
                }
            }
        }
    }

    GameObject SpawnEnemy(EnemyStats stats, Vector2 pos, bool overrideMove, Vector2 dir, float speed)
    {
        if (!enemyPoolMap.ContainsKey(stats)) return null;

        ObjectPool pool = enemyPoolMap[stats];
        GameObject enemy = pool.GetPooledObject();
        if (enemy == null) return null;

        enemy.transform.position = pos;
        enemy.SetActive(true);
        activeEnemies.Add(enemy);
        enemyCount = activeEnemies.Count;

        if (overrideMove)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetOverrideMovement(dir, speed);
            }
        }
        return enemy;
    }

    void DespawnCheck()
    {
        if (activeEnemies.Count == 0) return;
        
        List<GameObject> toDespawn = new List<GameObject>();
        foreach (var enemy in activeEnemies)
        {
            if (!enemy.activeInHierarchy)
            {
                toDespawn.Add(enemy);
                continue;
            }
            if (Vector2.Distance(enemy.transform.position, PlayerPos) > despawnRadius)
            {
                toDespawn.Add(enemy);
            }
        }

        foreach (var enemy in toDespawn)
        {
            enemy.SetActive(false);
            activeEnemies.Remove(enemy);
        }
        enemyCount = activeEnemies.Count;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        float r = Random.Range(minSpawnRadius, maxSpawnRadius);
        return (Vector2)PlayerPos + dir * r;
    }

    void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        Transform p = (player != null) ? player : transform;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(p.position, minSpawnRadius);
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(p.position, maxSpawnRadius);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(p.position, despawnRadius);
    }
}