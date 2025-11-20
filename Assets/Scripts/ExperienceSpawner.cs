using System.Collections.Generic;
using UnityEngine;

public class ExperienceSpawner : MonoBehaviour
{
    [Header("Experience")]
    public List<ObjectPool> Pool; // 0: Small, 1: Medium, 2: Large

    // Agora aceita o nome do inimigo para decidir a qualidade do drop
    public void SpawnExperience(Vector3 position, string enemyName)
    {
        int randomIndex = 0;
        float rand = Random.Range(0f, 1f);

        switch (enemyName)
        {
            case "Tiny":
                // INIMIGO FRACO: Alta chance de XP pequeno
                if (rand < 0.75f)       // 75% chance de Small
                {
                    randomIndex = 0;
                }
                else if (rand < 0.95f)  // 20% chance de Medium
                {
                    randomIndex = 1;
                }
                else                    // 5% chance de Large (Muito raro)
                {
                    randomIndex = 2;
                }
                break;

            case "Red":
                // INIMIGO FORTE: Alta chance de XP médio/grande
                if (rand < 0.20f)       // 20% chance de Small
                {
                    randomIndex = 0;
                }
                else if (rand < 0.70f)  // 50% chance de Medium
                {
                    randomIndex = 1;
                }
                else                    // 30% chance de Large (Bem comum)
                {
                    randomIndex = 2;
                }
                break;

            default:
                // PADRÃO (Caso adicione outros inimigos sem configurar)
                if (rand < 0.5f) randomIndex = 0;
                else if (rand < 0.8f) randomIndex = 1;
                else randomIndex = 2;
                break;
        }

        // Pega o pool correto baseada na sorte acima
        if (Pool != null && Pool.Count > randomIndex)
        {
            ObjectPool selectedPool = Pool[randomIndex];
            if (selectedPool != null)
            {
                GameObject exp = selectedPool.GetPooledObject();
                if (exp != null)
                {
                    exp.transform.position = position;
                    exp.SetActive(true);
                }
            }
        }
    }
}