using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    public bool spawnRocks = false;
    public GameObject rock;
    public int rockCount = 100;
    public float spawnMinRange = 100f;
    public float spawnMaxRange = 1000f;
    public float minRockScale = 2.0f;
    public float maxRockScale = 20.0f;

    public Texture2D mouseTexture;
    // Start is called before the first frame update
    void Start()
    {
        if(spawnRocks)
        {
            SpawnRocks();
        }
        Vector2 mouseTextureCentre = new Vector2(mouseTexture.width * 0.5f, mouseTexture.height * 0.5f);
        Cursor.SetCursor(mouseTexture, mouseTextureCentre, CursorMode.ForceSoftware);
    }

    void SpawnRocks()
    {
        if (rock != null)
        {
            for (int i = 0; i < rockCount; i++)
            {
                Vector3 randomPoint = Random.insideUnitSphere * Random.Range(spawnMinRange, spawnMaxRange);
                GameObject rockInstance = Instantiate(rock);
                rockInstance.transform.position = randomPoint;
                rockInstance.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
                rockInstance.transform.localScale *= Random.Range(minRockScale, maxRockScale);
                Rigidbody rb = rockInstance.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.mass = 100;
                rb.angularDrag = 0f;

                rb.AddRelativeForce(Random.onUnitSphere * Random.Range(1000f, 10000f), ForceMode.Impulse);
                rb.AddRelativeTorque(Random.onUnitSphere * Random.Range(1000f, 50000f), ForceMode.Impulse);

            }
        }
    }
}
