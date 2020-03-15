using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool spawnShips = false;
    public GameObject enemyShipObject;
    public int numShips = 10;
    public float spawnShipDelay = 5f;
    public float spawnDistance = 20f;

    public Transform target;

    int spawnedShipsCount = 0;
    float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        if(spawnShips)
        {
            if(spawnedShipsCount < numShips && timer > spawnShipDelay)
            {
                var enemy = Instantiate(enemyShipObject);
                enemy.transform.position = transform.position + (Random.onUnitSphere * spawnDistance);
                EnemyShip enemyShip = enemy.GetComponent<EnemyShip>();
                enemyShip.targetTransform = target;
                timer = 0f;
                spawnedShipsCount++;
            }

            timer += Time.deltaTime;
        }

        if(spawnedShipsCount == numShips)
        {
            Destroy(gameObject);
        }
    }

}
