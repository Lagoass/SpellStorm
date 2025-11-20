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
    
    [HideInInspector] public bool isFrozen = false;
    public float effectDuration = 0f;

    private float currentHealth; 
    private float currentMoveSpeed;

    [HideInInspector] public bool isOverridden = false;
    private Vector2 overrideDirection;
    private float overrideSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    void OnEnable()
    {
        currentHealth = Stats.health; 
        currentMoveSpeed = Stats.moveSpeed; 
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
            rb.linearVelocity = overrideDirection * overrideSpeed;
        }
        else
        {
            rb.linearVelocity = new Vector2(MoveDirection.x, MoveDirection.y) * currentMoveSpeed;
        }
    }

    public void SetOverrideMovement(Vector2 direction, float speed)
    {
        overrideDirection = direction.normalized;
        overrideSpeed = speed;
        isOverridden = true;
    }
    
    public void SetSpeed(float newSpeed)
    {
        if (newSpeed == 0) return;
        currentMoveSpeed = newSpeed;
    }

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
            
            // --- MUDANÇA AQUI ---
            // Ao ser congelado, o inimigo "esquece" a ordem de Batch
            if (isOverridden)
            {
                isOverridden = false;
                // Opcional: Se quiser que ele volte à velocidade normal do Stats ao descongelar,
                // em vez da velocidade rápida do Batch:
                currentMoveSpeed = Stats.moveSpeed; 
            }
            // --------------------
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        if (expSpawner != null)
        {
            expSpawner.SpawnExperience(transform.position, Stats.enemyName);
        }
    }

    public void ApplyEffects()
    {
        if (isFrozen)
        {
            BlueOverlay(true);
            rb.linearVelocity = Vector2.zero; // Para o movimento completamente
            
            if (frozenStopwatch.ElapsedTimeSec() >= effectDuration)
            {
                BlueOverlay(false);
                isFrozen = false;
                // Como já setamos isOverridden = false no TakeDamage,
                // ele vai voltar a usar 'currentMoveSpeed' e perseguir o jogador.
            }
        }
    }

    public void BlueOverlay(bool state)
    {
        if (state) spriteRenderer.color = Color.cyan;
        else spriteRenderer.color = Color.white;
    }
}