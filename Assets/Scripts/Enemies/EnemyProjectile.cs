using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float despawnRadius; // Agora usamos isso como raio ao redor do player
    private Rigidbody2D rb;
    private Transform playerTransform; // Precisamos da referência do player

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Cache do Player para checar distância (Performance)
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) playerTransform = playerGO.transform;
    }

    public void Init(float dmg, float spd, Vector2 dir, float size, float range)
    {
        damage = dmg;
        despawnRadius = range; // O valor 'range' agora será o raio de despawn

        transform.localScale = Vector3.one * size;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        rb.linearVelocity = dir.normalized * spd;
    }

    void Update()
    {
        // Se o player sumir, desativa a bala
        if (playerTransform == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        // NOVA LÓGICA: Checa distância do Player
        // Usamos sqrMagnitude para ser mais rápido (evita raiz quadrada)
        float distSqr = (transform.position - playerTransform.position).sqrMagnitude;
        
        // Se a distância ao quadrado for maior que o raio ao quadrado
        if (distSqr > despawnRadius * despawnRadius)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); 
                Debug.Log($"Player atingido por {damage} de dano!");
            }
            gameObject.SetActive(false); 
        }
    }
}