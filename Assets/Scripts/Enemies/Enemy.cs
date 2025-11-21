using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public EnemyStats Stats;

    // --- Referências ---
    Rigidbody2D rb;
    Transform player;
    ExperienceSpawner expSpawner;
    SpriteRenderer spriteRenderer;
    public EnemyAnimator enemyAnimator;
    
    private EnemyProjectileController projectileController; 
    private GameController gameController; // Necessário para avisar vitória do Boss

    // --- Variáveis de Estado ---
    private float currentHealth; 
    private float currentMoveSpeed;
    public Vector2 MoveDirection; 

    // --- Controle (Batch/Eventos) ---
    [HideInInspector] public bool isOverridden = false;
    private Vector2 overrideDirection;
    private float overrideSpeed;

    // --- Identidade ---
    [HideInInspector] public bool isBoss = false;

    // --- Efeitos (Gelo) ---
    [HideInInspector] public bool isFrozen = false;
    public float effectDuration = 0f;
    private Stopwatch frozenStopwatch = new Stopwatch(); 

    // --- Estado Ranged ---
    private Stopwatch attackCooldownStopwatch = new Stopwatch();
    private bool isAttacking = false; 
    private GameObject chargeVisualInstance;
    private Coroutine attackCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    void OnEnable()
    {
        // 1. Reset Geral e Trava de Segurança
        if (Stats.health <= 0) currentHealth = Stats.baseHealth;
        else currentHealth = Stats.health;

        currentMoveSpeed = Stats.moveSpeed; 
        rb.mass = Stats.baseMass;
        
        isFrozen = false;
        isOverridden = false; 
        BlueOverlay(false);
        
        transform.localScale = new Vector3(Stats.size, Stats.size, 1f);

        // 2. Reset Específico Ranged
        if (Stats.enemyType == EnemyType.Ranged)
        {
            isAttacking = false;
            attackCooldownStopwatch.Restart(); 
            if (chargeVisualInstance != null) Destroy(chargeVisualInstance);
        }
    }

    void Start()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.transform;
        
        expSpawner = FindObjectOfType<ExperienceSpawner>();
        gameController = FindObjectOfType<GameController>();

        if (Stats.enemyType == EnemyType.Ranged)
        {
            FindMyProjectileController();
        }
    }

    void FindMyProjectileController()
    {
        // Tenta buscar no Dicionário Estático (Otimizado)
        if (EnemyProjectileController.ControllerMap.TryGetValue(this.Stats, out var ctrl))
        {
            projectileController = ctrl;
        }
        else
        {
            // Fallback: Busca na cena se não estiver no dicionário
            var controllers = FindObjectsOfType<EnemyProjectileController>();
            foreach (var c in controllers)
            {
                if (c.projectileStatsSource == this.Stats)
                {
                    projectileController = c;
                    break;
                }
            }
        }
        
        if (projectileController == null) 
        {
            Debug.LogWarning($"[ERRO] Inimigo Ranged '{name}' não achou Controller para {Stats.name}");
        }
    }

    void Update()
    {
        if (player == null) return;

        ApplyEffects(); 
        if (isFrozen) return; 

        // --- MÁQUINA DE ESTADOS DE MOVIMENTO ---

        if (isOverridden)
        {
            // Movimento controlado pelo Spawner (Batch) - Tratado no FixedUpdate
        }
        else if (!isAttacking)
        {
            if (Stats.enemyType == EnemyType.Melee)
            {
                LogicMelee();
            }
            else if (Stats.enemyType == EnemyType.Ranged)
            {
                LogicRanged();
            }
        }
        else 
        {
            MoveDirection = Vector2.zero; // Para enquanto carrega o ataque
        }

        // Verifica Morte
        if (currentHealth <= 0) Die();
    }

    void FixedUpdate()
    {
        if (isFrozen) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isOverridden)
        {
            rb.linearVelocity = overrideDirection * overrideSpeed;
        }
        else
        {
            rb.linearVelocity = MoveDirection * currentMoveSpeed;
        }
    }

    void LogicMelee()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        MoveDirection = direction;
        if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;
    }

    void LogicRanged()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        // 1. Movimento (Kiting)
        if (distToPlayer > Stats.attackRange)
        {
            MoveDirection = dirToPlayer; // Aproxima
        }
        else if (distToPlayer < Stats.keepDistance)
        {
            MoveDirection = -dirToPlayer; // Foge
        }
        else
        {
            MoveDirection = Vector2.zero; // Para na zona ideal
            
            // 2. Tenta Atacar
            if (attackCooldownStopwatch.ElapsedTimeSec() >= Stats.baseAttackRate)
            {
                attackCoroutine = StartCoroutine(AttackRoutine());
            }
        }

        if (dirToPlayer.x != 0) spriteRenderer.flipX = dirToPlayer.x < 0;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        MoveDirection = Vector2.zero; 

        // 1. Visual de Carga
        if (Stats.preAttackVisual != null)
        {
            chargeVisualInstance = Instantiate(Stats.preAttackVisual, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);
        }

        // 2. Espera tempo de carga
        yield return new WaitForSeconds(Stats.chargeTime);

        // 3. Disparo
        if (gameObject.activeInHierarchy && !isFrozen && player != null)
        {
            if (projectileController != null)
            {
                Vector2 fireDir = (player.position - transform.position).normalized;
                
                projectileController.SpawnProjectile(
                    transform.position, 
                    fireDir, 
                    Stats.projectileSpeed, 
                    Stats.damage, 
                    0.2f, // Size (padrão 1)
                    Stats.attackRange * 1.5f // Range (um pouco maior que o ataque)
                );
            }
        }

        // 4. Limpeza e Cooldown
        if (chargeVisualInstance != null) Destroy(chargeVisualInstance);
        
        attackCooldownStopwatch.Restart(); 
        isAttacking = false;
        attackCoroutine = null;
    }

    // --- CONTROLE EXTERNO ---

    public void SetOverrideMovement(Vector2 direction, float speed)
    {
        overrideDirection = direction.normalized;
        overrideSpeed = speed;
        isOverridden = true;
        if (isAttacking) CancelAttack();
    }
    
    public void SetSpeed(float newSpeed)
    {
        if (newSpeed == 0) return;
        currentMoveSpeed = newSpeed;
    }

    // --- SISTEMA DE DANO ---

    public void TakeDamage(float damage, String effect = null, float duration = 0)
    {
        if (enemyAnimator != null) enemyAnimator.PlayHurtAnimation();
        currentHealth -= damage; 
        SoundManager.Instance.PlaySound(SoundType.EnemyHit);
        if (effect == "Ice") ApplyFreeze(duration);
    }

    void ApplyFreeze(float duration)
    {
        isFrozen = true;
        effectDuration = duration;
        frozenStopwatch.Restart();

        if (isOverridden)
        {
            isOverridden = false;
            currentMoveSpeed = Stats.moveSpeed; 
        }

        if (isAttacking) CancelAttack();
    }

    void CancelAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (chargeVisualInstance != null) Destroy(chargeVisualInstance);
        isAttacking = false;
        attackCooldownStopwatch.Restart(); 
        attackCoroutine = null;
    }

    public void ApplyEffects()
    {
        if (isFrozen)
        {
            BlueOverlay(true);
            rb.linearVelocity = Vector2.zero; 
            
            if (frozenStopwatch.ElapsedTimeSec() >= effectDuration)
            {
                BlueOverlay(false);
                isFrozen = false;
                currentMoveSpeed = Stats.moveSpeed; 
            }
        }
    }

    public void BlueOverlay(bool state)
    {
        if (spriteRenderer == null) return;
        
        if (state) spriteRenderer.color = Color.cyan;
        else spriteRenderer.color = Color.white;
    }

    public void Die()
    {
        // Limpeza Visual
        if (chargeVisualInstance != null) Destroy(chargeVisualInstance);
        
        // Verifica Vitória (Boss)
        if (isBoss && gameController != null)
        {
            gameController.WinGame();
        }

        // Desativa
        gameObject.SetActive(false);
        
        // Drop XP
        if (expSpawner != null)
        {
            expSpawner.SpawnExperience(transform.position, Stats.enemyName);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Apenas inimigos Melee causam dano de contato
        if (Stats.enemyType == EnemyType.Melee && collision.gameObject.CompareTag("Player"))
        {
            Player p = collision.gameObject.GetComponent<Player>();
            if (p != null) p.TakeDamage(Stats.damage);
        }
    }
}