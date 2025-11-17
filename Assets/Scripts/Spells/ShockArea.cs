using System.Collections.Generic;
using UnityEngine;

public class ShockArea : MonoBehaviour
{
    public MagicStats Stats;
    public bool ShowGizmos = true;
    Transform Player;
    private Stopwatch damageStopwatch = new Stopwatch();
    public List<GameObject> particles;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        RenderParticles();
        if (Stats.isActive == false) return;
        if (Player == null) return;
        transform.position = Player.position;
        transform.localScale = new Vector3(Stats.aroundPlayerMagicStats.size, Stats.aroundPlayerMagicStats.size, 1);

        DamageEnemiesInArea();
    }

    public void DamageEnemiesInArea()
    {
        if (damageStopwatch.ElapsedTimeSec() < Stats.aroundPlayerMagicStats.attackRate) { return; }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Stats.aroundPlayerMagicStats.radius / 2);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                enemy.TakeDamage(Stats.aroundPlayerMagicStats.damage);
            }
        }
        damageStopwatch.Restart();
    }

    void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Stats.aroundPlayerMagicStats.radius / 2);
    }

    public void RenderParticles()
    {
        foreach (GameObject particle in particles)
        {
            particle.SetActive(Stats.isActive);
        }
    }
}
