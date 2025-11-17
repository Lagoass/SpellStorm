using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao GameController para controle de pausa.")]
    public GameController gameController; 

    [Header("Pools de Inimigos")]
    [Tooltip("Lista de TODOS os EnemyStats possíveis (Red, Tiny, etc.). A ORDEM DEVE BATER COM A LISTA DE POOLS!")]
    public List<EnemyStats> enemyStatsList; 
    [Tooltip("Lista de TODOS os ObjectPools correspondentes. A ORDEM DEVE BATER COM A LISTA DE STATS!")]
    public List<ObjectPool> enemyPools;
    
    [Header("Valores Padrão de Eventos")]
    [Tooltip("Raio de 'espalhamento' para o grupo do Batch (ex: 1.5f).")]
    public float batchSpread = 1.5f;
    [Tooltip("Raio MÍNIMO do círculo (padrão: 8).")]
    public float minCircleRadius = 8f;
    [Tooltip("Raio MÁXIMO do círculo (padrão: 10).")]
    public float maxCircleRadius = 10f;
    [Tooltip("Velocidade (lenta) para o evento Circle (padrão: 0.5).")]
    public float circleSpeedOverride = 0.5f;

    [Header("Raio (Distância do Jogador)")]
    public float minSpawnRadius = 8f; 
    public float maxSpawnRadius = 15f; 
    public float despawnRadius  = 20f;

    [Header("Contadores e Debug")]
    [SerializeField]
    private int enemyCount = 0;
    public bool ShowGizmos = true;

    // --- Variáveis de Runtime (Controladas pelo GameController) ---
    private WaveConfig currentWaveConfig;
    private float nextTrickleSpawnTime;
    private float nextEventTriggerTime; // Timer para eventos aleatórios
    private int currentTrickleBudget;
    private int currentTrickleEnemiesAlive; 
    private float waveStartTime; 
    
    // --- Internals ---
    Transform player;
    Rigidbody2D playerRb; 
    readonly HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    Vector3 PlayerPos => player != null ? player.position : Vector3.zero;
    
    static readonly List<GameObject> ListCache = new List<GameObject>(32); 
    
    private Dictionary<EnemyStats, ObjectPool> enemyPoolMap; 

    void Awake()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) 
        {
            player = playerGO.transform;
            playerRb = playerGO.GetComponent<Rigidbody2D>(); 
        }

        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
        }
        
        enemyPoolMap = new Dictionary<EnemyStats, ObjectPool>();
        if (enemyStatsList.Count != enemyPools.Count)
        {
            Debug.LogError("ERRO DE SPAWNER: A lista 'enemyStatsList' e 'enemyPools' TÊM TAMANHOS DIFERENTES!");
        }
        else
        {
            for (int i = 0; i < enemyStatsList.Count; i++)
            {
                enemyPoolMap[enemyStatsList[i]] = enemyPools[i];
            }
        }
    }

    // Esta função é chamada pelo GameController no início de cada onda
    public void StartNewWave(WaveConfig config)
    {
        currentWaveConfig = config;
        waveStartTime = Time.time;
        
        nextTrickleSpawnTime = Time.time;
        nextEventTriggerTime = Time.time + config.eventInterval; 
        
        // Calcula o orçamento (saldo) disponível apenas para o Gotejamento
        // (Assumindo que eventos NÃO gastam do orçamento principal, apenas 'maxEnemies')
        currentTrickleBudget = config.totalBudget;
        
        currentTrickleEnemiesAlive = 0;
    }

    void Update()
    {
        if (currentWaveConfig == null || gameController == null || !gameController.canSpawn)
        {
            return;
        }

        HandleTrickleSpawn(); 
        HandleEventSpawns(); 
        DespawnCheck();
    }
    
    void HandleTrickleSpawn()
    {
        if (Time.time < nextTrickleSpawnTime) return;
        if (currentTrickleBudget <= 0) return; 
        if (currentTrickleEnemiesAlive >= currentWaveConfig.maxTrickleEnemies) return;
        if (currentWaveConfig.availableEnemies.Count == 0) return;

        EnemyStats enemyToSpawn = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];
        if (currentTrickleBudget >= enemyToSpawn.spawnCost)
        {
            if (SpawnEnemy(enemyToSpawn, GetRandomSpawnPosition(), false, Vector2.zero, 0f))
            {
                currentTrickleBudget -= enemyToSpawn.spawnCost; 
                currentTrickleEnemiesAlive++;
            }
        }
        
        nextTrickleSpawnTime = Time.time + currentWaveConfig.trickleSpawnInterval;
    }
    
    // Lida com o timer de evento aleatório
    void HandleEventSpawns()
    {
        if (Time.time > nextEventTriggerTime)
        {
            TriggerRandomEvent();
            // Reseta o timer para o próximo evento
            nextEventTriggerTime = Time.time + currentWaveConfig.eventInterval; 
        }
    }

    // Dispara um evento aleatório se ele estiver habilitado na WaveConfig
    void TriggerRandomEvent()
    {
        bool batchEnabled = currentWaveConfig.enableBatchEvents;
        bool circleEnabled = currentWaveConfig.enableAroundEvents;

        if (batchEnabled && circleEnabled)
        {
            // Sorteia entre os dois
            if (Random.value > 0.5f)
                ExecuteBatchEvent(currentWaveConfig.batchSettings);
            else
                ExecuteCircleEvent(currentWaveConfig.circleSettings);
        }
        else if (batchEnabled)
        {
            ExecuteBatchEvent(currentWaveConfig.batchSettings);
        }
        else if (circleEnabled)
        {
            ExecuteCircleEvent(currentWaveConfig.circleSettings);
        }
    }

    void ExecuteCircleEvent(CircleSettings settings)
    {
        if (player == null || currentWaveConfig.availableEnemies.Count == 0 || settings == null) return;

        float radius = Random.Range(minCircleRadius, maxCircleRadius); 
        int amountToSpawn = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);
        if (amountToSpawn == 0) return;

        for (int i = 0; i < amountToSpawn; i++)
        {
            EnemyStats enemyToSpawn = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];
            
            float angle = i * (360f / amountToSpawn); 
            Vector2 direction = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
            Vector2 spawnPos = (Vector2)PlayerPos + (direction * radius);

            GameObject enemy = SpawnEnemy(enemyToSpawn, spawnPos, false, Vector2.zero, 0f); 
            if (enemy != null)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.SetSpeed(circleSpeedOverride); 
                }
            }
        }
    }
    
    void ExecuteBatchEvent(BatchSettings settings)
    {
        if (player == null || currentWaveConfig.availableEnemies.Count == 0 || settings == null) return;

        // 1. ESCOLHER PONTO DE SPAWN (Próximo ao jogador, no anel visível)
        Vector2 spawnCenter = GetRandomSpawnPosition(); 
        
        // 2. CALCULAR A DIREÇÃO (Mirar no jogador a partir desse ponto)
        Vector2 batchMoveDir = ((Vector2)PlayerPos - spawnCenter).normalized;
        
        // 3. QUANTIDADE (Dos settings)
        int amountToSpawn = Random.Range(settings.minSpawnCount, settings.maxSpawnCount + 1);

        for (int i = 0; i < amountToSpawn; i++)
        {
            EnemyStats enemyToSpawn = currentWaveConfig.availableEnemies[Random.Range(0, currentWaveConfig.availableEnemies.Count)];

            Vector2 clusterOffset = Random.insideUnitCircle * batchSpread; 
            Vector2 spawnPos = spawnCenter + clusterOffset; 
            
            // 4. Spawnar (Usando a velocidade dos settings)
            SpawnEnemy(enemyToSpawn, spawnPos, true, batchMoveDir, settings.speedOverride);
        }
    }

    // Função centralizada para spawnar e registrar um inimigo
    GameObject SpawnEnemy(EnemyStats enemyStats, Vector2 position, bool overrideMovement, Vector2 overrideDir, float overrideSpeed)
    {
        if (!enemyPoolMap.ContainsKey(enemyStats))
        {
            Debug.LogWarning("Nenhum pool encontrado para o inimigo: " + enemyStats.enemyName);
            return null;
        }

        ObjectPool pool = enemyPoolMap[enemyStats];
        GameObject enemy = pool.GetPooledObject();
        if (enemy == null) return null; 

        enemy.transform.position = position;
        enemy.SetActive(true);
        activeEnemies.Add(enemy);
        enemyCount = activeEnemies.Count;
        
        if (overrideMovement)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetOverrideMovement(overrideDir, overrideSpeed);
            }
        }
        
        return enemy;
    }
    
    void DespawnCheck()
    {
        if (activeEnemies.Count == 0) return;
        var toDespawn = ListCache;
        toDespawn.Clear();
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) 
            { 
                toDespawn.Add(enemy); 
                continue; 
            }
            float distToPlayer = Vector2.Distance(enemy.transform.position, PlayerPos);
            if (distToPlayer > despawnRadius) toDespawn.Add(enemy);
        }
        for (int i = 0; i < toDespawn.Count; i++)
            DespawnEnemy(toDespawn[i]);
        enemyCount = activeEnemies.Count;
    }

    void DespawnEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        
        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
        
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        // Checa se o inimigo NÃO estava em modo override (ou seja, era do Gotejamento ou Circle)
        if (enemyScript != null && !enemyScript.isOverridden) 
        {
            if (currentTrickleEnemiesAlive > 0)
                currentTrickleEnemiesAlive--; // Libera espaço para o gotejamento
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
        Transform p = player;
        if (p == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) p = playerGO.transform;
        }
        if (p == null) return;

        Vector3 c = p.position;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(c, minSpawnRadius);
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(c, maxSpawnRadius);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(c, despawnRadius);
    }
    
    // Funções de Wave (Legado) - REMOVIDAS
}