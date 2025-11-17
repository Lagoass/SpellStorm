using UnityEngine;

// Herda do ProjectileBase, não mais do MonoBehaviour
public class FireBall : ProjectileBase
{
    public int rotationOffset = 0;

    // Sobrescreve o InitMovement para adicionar o offset de rotação
    public override void InitMovement()
    {
        if (currentTarget == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * currentSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // A ÚNICA DIFERENÇA: Adiciona o offset
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + rotationOffset));
    }

    // Sobrescreve a colisão para adicionar a explosão
    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Enemy")) return;
        
        Enemy enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(currentDamage); // Dano do projétil
        }
        
        if (hasImpactEffect)
        {
            Explode(); // Efeito especial
        }
        
        gameObject.SetActive(false);
    }
    
    void Explode()
    {
        if (impactEffectPrefab == null) return;

        // Cria a explosão
        GameObject explosion = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, effectDuration);
        
        // Aplica dano em área (lendo os dados injetados)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, effectDamageRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(effectDamage); // Dano da explosão
                }
            }
        }
    }
}