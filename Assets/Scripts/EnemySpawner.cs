using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao GameController para controle de pausa.")]
    public GameController gameController;
    public TextMeshProUGUI TimerText; 

    [Header("Pools de Inimigos")]
    public List<ObjectPool> enemyPools;
    public List<EnemyStats> enemyStatsList; 

    [Header("Raio (Distância do Jogador)")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 15f;
    public float despawnRadius  = 20f;

    [Header("Configuração de Spawn (Gotejamento)")]
    [Range(0f, 2f)]
    public float spawnInterval = 0.5f;
    [Range(1, 100)]
    public int maxEnemies = 1; 

    
    [Header("Eventos Especiais")]
    [Tooltip("O tempo (em segundos) entre as tentativas de disparar um evento.")]
    public float eventInterval = 20f;
    
    [Header("Evento: Batch (Emboscada)")]
    [Tooltip("A velocidade forçada para os inimigos do Batch.")]
    public float batchSpeedOverride = 5f;
    [Tooltip("O RAIO do círculo de spawn para o 'grupo' do batch.")]
    public float batchSpread = 3f; 
    [Tooltip("O número MÍNIMO de inimigos neste evento.")]
    public int minBatchAmount = 15;
    [Tooltip("O número MÁXIMO de inimigos neste evento.")]
    public int maxBatchAmount = 25;

    // --- MUDANÇAS AQUI ---
    [Header("Evento: Circle (Cerco)")]
    [Tooltip("O número MÍNIMO de inimigos neste evento.")]
    public int minCircleAmount = 15; 
    [Tooltip("O número MÁXIMO de inimigos neste evento.")]
    public int maxCircleAmount = 20;
    public float minCircleRadius = 8f;
    public float maxCircleRadius = 10f;
    [Tooltip("Velocidade forçada (mais lenta) para inimigos do Círculo.")]
    public float circleSpeedOverride = 1.5f; 
    // --- FIM DAS MUDANÇAS ---

    [Header("Contadores e Debug")]
    [SerializeField]
    private int enemyCount = 0;
    public bool ShowGizmos = true;

    
    float nextSpawnTime;
    float nextEventTime; 
    Transform player;
    Rigidbody2D playerRb; 
    readonly HashSet<GameObject> activeEnemies = new HashSet<GameObject>();
    Vector3 PlayerPos => player != null ? player.position : Vector3.zero;

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
    }
    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        nextEventTime = Time.time + eventInterval; 
    }

    void Update()
    {
        if (gameController == null || !gameController.canSpawn)
        {
            return;
        }

        SpawnEnemy_RandomPosition(); 

        if (Time.time > nextEventTime)
        {
            TriggerRandomEvent();
            nextEventTime = Time.time + eventInterval; 
        }
        
        DespawnCheck();
    }

    void TriggerRandomEvent()
    {
        if (Random.value > 0.5f)
        {
            Debug.Log("Triggering Batch Event");
            ExecuteBatchEvent();
        }
        else
        {
            Debug.Log("Triggering Circle Event");
            ExecuteCircleEvent();
        }
    }

    // --- MUDANÇA AQUI ---
    void ExecuteCircleEvent()
    {
        if (player == null || enemyPools.Count == 0) return;

        float radius = Random.Range(minCircleRadius, maxCircleRadius);

        // Calcula a quantidade aleatória de inimigos
        int amountToSpawn = Random.Range(minCircleAmount, maxCircleAmount + 1);
        if (amountToSpawn == 0) return; // Evita divisão por zero se o range for 0

        for (int i = 0; i < amountToSpawn; i++) // Usa a quantidade aleatória
        {
            ObjectPool pool = enemyPools[Random.Range(0, enemyPools.Count)];
            GameObject enemy = pool.GetPooledObject();
            if (enemy == null || activeEnemies.Contains(enemy)) continue;

            // Calcula o ângulo baseado na quantidade aleatória
            float angle = i * (360f / amountToSpawn); 
            Vector2 direction = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
            Vector2 spawnPos = (Vector2)PlayerPos + (direction * radius);

            enemy.transform.position = spawnPos;
            enemy.SetActive(true);

            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetSpeed(circleSpeedOverride);
            }

            activeEnemies.Add(enemy);
        }
        enemyCount = activeEnemies.Count;
    }
    // --- FIM DA MUDANÇA ---
    
    void ExecuteBatchEvent()
    {
        if (player == null || enemyPools.Count == 0) return;

        Vector2 spawnCenter = GetRandomSpawnPosition(); 
        Vector2 batchMoveDir = ((Vector2)PlayerPos - spawnCenter).normalized;
        int amountToSpawn = Random.Range(minBatchAmount, maxBatchAmount + 1);

        for (int i = 0; i < amountToSpawn; i++)
        {
            ObjectPool pool = enemyPools[Random.Range(0, enemyPools.Count)];
            GameObject enemy = pool.GetPooledObject();
            if (enemy == null || activeEnemies.Contains(enemy)) continue;

            Vector2 clusterOffset = Random.insideUnitCircle * batchSpread;
            Vector2 spawnPos = spawnCenter + clusterOffset; 
            
            enemy.transform.position = spawnPos;
            enemy.SetActive(true);

            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetOverrideMovement(batchMoveDir, batchSpeedOverride);
            }
            
            activeEnemies.Add(enemy);
        }
        enemyCount = activeEnemies.Count;
    }

    void DespawnCheck()
    {
        if (activeEnemies.Count == 0) return;

        var toDespawn = ListCache;
        toDespawn.Clear();

        foreach (var enemy in activeEnemies)
        {
            if (!enemy.activeInHierarchy) { toDespawn.Add(enemy); continue; }

            float distToPlayer = Vector2.Distance(enemy.transform.position, PlayerPos);
            if (distToPlayer > despawnRadius) toDespawn.Add(enemy);
        }

        for (int i = 0; i < toDespawn.Count; i++)
            DespawnEnemy(toDespawn[i]);

        enemyCount = activeEnemies.Count;
    }

    void SpawnEnemy_RandomPosition()
    {
        
        if (Time.time < nextSpawnTime || activeEnemies.Count >= maxEnemies) return;
        if (enemyPools == null || enemyPools.Count == 0) return;

        ObjectPool pool = enemyPools[Random.Range(0, enemyPools.Count)];
        if (pool == null) return;

        GameObject enemy = pool.GetPooledObject();
        if (enemy == null) return; 
        if (activeEnemies.Contains(enemy)) return; 

        enemy.transform.position = GetRandomSpawnPosition();
        enemy.SetActive(true);
        
        activeEnemies.Add(enemy);
        enemyCount = activeEnemies.Count;
        nextSpawnTime = Time.time + spawnInterval;
    }
    void DespawnEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
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
    
    static readonly List<GameObject> ListCache = new List<GameObject>(32);
}