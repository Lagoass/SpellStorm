using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    [Header("Player Stats")]
    public PlayerStats Stats;
    [HideInInspector]
    public Vector2 MovementDirection;
    public PlayerAnimator playerAnimator;

    private Stopwatch damageStopwatch = new Stopwatch();
    private Stopwatch regenStopwatch = new Stopwatch();
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Stats.health = 100f;
    }
    void Update()
    {
        rb.mass = Stats.mass;
        transform.localScale = new Vector3(Stats.size, Stats.size, 1f);
        HealthRegen();
    }
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        MovementDirection = new Vector2(moveHorizontal, moveVertical).normalized;
        rb.MovePosition(rb.position + MovementDirection * Stats.moveSpeed * Time.fixedDeltaTime);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        TakeDamage(enemy.Stats.damage);
    }
    public void TakeDamage(float damage)
    {
        if (damageStopwatch.ElapsedTimeSec() < 0.25f) { return; }
        SoundManager.Instance.PlaySound(SoundType.PlayerDamage);
        Stats.health -= damage;
        playerAnimator.PlayHurtAnimation();
        damageStopwatch.Restart();
    }
    public void HealthRegen()
    {
        if (regenStopwatch.ElapsedTimeSec() < 1.5f) { return; }
        Stats.health += Stats.healthRegen;
        if (Stats.health > Stats.startingHealth)
        {
            Stats.health = Stats.startingHealth;
        }
        regenStopwatch.Restart();
    }
}
