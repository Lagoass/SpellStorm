using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3 newPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = newPosition;
    }
}
