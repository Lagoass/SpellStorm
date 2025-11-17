using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileBase : MonoBehaviour
{
    // --- Dados Injetados pelo Controller ---
    [HideInInspector] public Transform currentTarget;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float currentSize;
    [HideInInspector] public float despawnDistance = 10f;
    [HideInInspector] public float currentSpread = 0f; // <-- ADICIONADO: A variação angular
    
    // --- Efeitos (Injetados) ---
    [HideInInspector] public bool hasImpactEffect;
    [HideInInspector] public GameObject impactEffectPrefab;
    [HideInInspector] public float effectDuration;
    [HideInInspector] public float effectDamageRadius;
    [HideInInspector] public float effectDamage;
    [HideInInspector] public float effectSize;
    
    // --- Componentes Internos ---
    protected Rigidbody2D rb;
    private Transform playerTransform;
    private bool needsInitialization = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerGO = GameObject.FindWithTag("Player"); 
        if (playerGO != null) playerTransform = playerGO.transform;
    }

    void OnEnable()
    {
        needsInitialization = true;
    }

    // InitMovement agora aplica a variação (Spread)
    public virtual void InitMovement()
    {
        if (currentTarget == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        // 1. Calcula a direção base
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // 2. Calcula a variação aleatória
        // (Ex: Se currentSpread = 5, gera um ângulo entre -5 e 5)
        float randomSpread = Random.Range(-currentSpread, currentSpread);
        Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);

        // 3. Aplica a variação à direção
        Vector2 finalDirection = spreadRotation * direction;
        
        // 4. Aplica a velocidade e a rotação
        rb.linearVelocity = finalDirection * currentSpeed;

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void Update()
    {
        if (needsInitialization)
        {
            InitMovement();
            needsInitialization = false;
        }
        
        transform.localScale = new Vector3(currentSize, currentSize, 1f);
        DespawnCheck();
    }
    
    void DespawnCheck()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > despawnDistance)
        {
            gameObject.SetActive(false);
        }
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Enemy")) return;
        
        Enemy enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(currentDamage);
        }
        
        gameObject.SetActive(false);
    }
}