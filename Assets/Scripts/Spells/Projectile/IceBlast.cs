using UnityEngine;

public class IceBlast : ProjectileBase
{
    [HideInInspector]
    public string EffectType = "Ice"; // Efeito de Slow

    // Sobrescreve a colisão para adicionar a explosão (que aplica Slow)
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
            Explode(); 
        }
        
        gameObject.SetActive(false);
    }
    
    void Explode()
    {
        if (impactEffectPrefab == null) return;

        GameObject explosion = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        explosion.transform.localScale *= effectSize; // Usa o effectSize
        Destroy(explosion, effectDuration);
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, effectDamageRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Aplica o dano E o efeito de Slow (Ice)
                    enemy.TakeDamage(effectDamage, EffectType, effectDuration);
                }
            }
        }
    }
}