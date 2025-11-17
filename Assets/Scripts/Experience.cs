using UnityEngine;

public class Experience : MonoBehaviour
{
    public ExperienceStats Stats;
    public Vector2 direction;
    GameObject player;
    Transform playerTransform; 
    
    void OnEnable()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float radiusSqr = Stats.pickupRadius * Stats.pickupRadius;
        float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;

        if (distanceSqr < radiusSqr)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, 5f * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;
        
        Player playerScript = collider.GetComponent<Player>();
        
        // Lógica de Multiplicação (Correta)
        playerScript.Stats.experience += Stats.experienceValue * playerScript.Stats.XpGains;
        
        gameObject.SetActive(false);
    }
}