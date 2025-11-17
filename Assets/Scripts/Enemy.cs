using System;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public EnemyStats Stats;
    Rigidbody2D rb;
    Transform player;
    ExperienceSpawner expSpawner;
    SpriteRenderer spriteRenderer;
    private Stopwatch frozenStopwatch = new Stopwatch();
    public Vector2 MoveDirection;
    public EnemyAnimator enemyAnimator;
    [HideInInspector]
    public bool isFrozen = false;
    public float effectDuration = 0f;

    private float currentHealth; 
    private float currentMoveSpeed;

    // --- VARIÁVEIS DO MODO BATCH ---
    private bool isOverridden = false;
    private Vector2 overrideDirection;
    private float overrideSpeed;
    // --- FIM DA ADIÇÃO ---

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    void OnEnable()
    {
        currentHealth = Stats.health; 
        currentMoveSpeed = Stats.moveSpeed; // Velocidade padrão do SO
        isFrozen = false;
        effectDuration = 0f;
        BlueOverlay(false);
        
        isOverridden = false; 
        
        rb.mass = Stats.mass;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        expSpawner = FindObjectOfType<ExperienceSpawner>();
    }

    void Update()
    {
        ApplyEffects();
        
        if (!isOverridden)
        {
            DirectionToPlayer();
        }

        transform.localScale = new Vector3(Stats.size, Stats.size, 1f);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        if (isOverridden)
        {
            // Evento Batch: Usa a velocidade e direção forçadas
            rb.linearVelocity = overrideDirection * overrideSpeed;
        }
        else
        {
            // Evento Circle ou Gotejamento: Usa a velocidade 'currentMoveSpeed'
            // (que pode ter sido alterada pelo SetSpeed)
            rb.linearVelocity = new Vector2(MoveDirection.x, MoveDirection.y) * currentMoveSpeed;
        }
    }

    // Função chamada pelo Evento Batch do Spawner
    public void SetOverrideMovement(Vector2 direction, float speed)
    {
        overrideDirection = direction.normalized;
        overrideSpeed = speed;
        isOverridden = true;
    }

    // --- NOVA FUNÇÃO ---
    // Chamada pelo Evento Circle para apenas mudar a velocidade,
    // mantendo o inimigo seguindo o jogador (isOverridden = false).
    public void SetSpeed(float newSpeed)
    {
        currentMoveSpeed = newSpeed;
    }
    // --- FIM DA NOVA FUNÇÃO ---

    void DirectionToPlayer()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        MoveDirection = direction;
    }
    
    public void TakeDamage(float damage, String effect = null, float effectDurationReceived = 0)
    {
        enemyAnimator.PlayHurtAnimation();
        currentHealth -= damage; 

        if (effect == "Ice")
        {
            isFrozen = true;
            effectDuration = effectDurationReceived;
            frozenStopwatch.Restart();
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        
        if (expSpawner != null)
        {
            expSpawner.SpawnExperience(transform.position);
        }
    }

    public void ApplyEffects()
    {
        if (isFrozen)
        {
            BlueOverlay(true);
            // Não precisamos definir currentMoveSpeed = 0 aqui,
            // pois o FixedUpdate já para se isFrozen = true
            rb.linearVelocity = Vector2.zero; 
            
            if (frozenStopwatch.ElapsedTimeSec() >= effectDuration)
            {
                BlueOverlay(false);
                isFrozen = false;
                currentMoveSpeed = Stats.moveSpeed; // Reseta para a velocidade do SO
            }
        }
    }

    public void BlueOverlay(bool state)
    {
        if (state)
        {
            spriteRenderer.color = Color.cyan;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}