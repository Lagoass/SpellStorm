using System.Collections.Generic;
using UnityEngine;

public class ExperienceSpawner : MonoBehaviour
{
    [Header("Experience")]
    public List<ObjectPool> Pool;
    // Não precisamos de LSTexperienceStats ou playerStats aqui
 
    // Não precisamos de Start() ou Update()
    // O GameController já cuida de atualizar o pickupRadius

    public void SpawnExperience(Vector3 position)
    {
        int randomIndex;
        float rand = Random.Range(0f, 1f);
        if (rand < 0.5f)
        {
            randomIndex = 0; // Small
        }
        else if (rand < 0.8f)
        {
            randomIndex = 1; // Medium
        }
        else
        {
            randomIndex = 2; // Large
        }

        ObjectPool selectedPool = Pool[randomIndex];
        GameObject exp = selectedPool.GetPooledObject();
        exp.transform.position = position;
        exp.SetActive(true);
    }
}