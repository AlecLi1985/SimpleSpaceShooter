using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    public float health;
    public GameObject[] rocks;
    public GameObject explosionObject;
    public string explosionSound;

    public float startMinPushForce;
    public float startMaxPushForce;

    public float startMinTorqueForce;
    public float startMaxTorqueForce;

    Rigidbody rb;

    int numRocksToSpawn;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 100f;

        if (rb != null)
        {
            rb.AddRelativeForce(Random.onUnitSphere * Random.Range(startMinPushForce, startMaxPushForce), ForceMode.Impulse);
            rb.AddRelativeTorque(Random.onUnitSphere * Random.Range(startMinTorqueForce, startMaxTorqueForce), ForceMode.Impulse);
        }

        health = Random.Range(10f, 50f);

        numRocksToSpawn = Random.Range(2, 4);

        Projectile.HitEvent += OnHit;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnHit(float damage, Transform hitTransform)
    {
        if(hitTransform == transform)
        {
            health -= damage;
            if (health < 0f)
            {
                SoundManager.instance.PlaySound(explosionSound);

                if (transform.localScale.magnitude > (Vector3.one * 10f).magnitude)
                {
                    BreakApart();
                }

                OnDestroy();
            }
        }
    }

    void BreakApart()
    {
        for(int i = 0; i < numRocksToSpawn; i++)
        {
            int id = Random.Range(0, rocks.Length);
            GameObject rockInstance = Instantiate(rocks[id]);

            Vector3 randomPoint = transform.position + Random.insideUnitSphere * Random.Range(-10f, 10f);

            rockInstance.transform.position = randomPoint;
            rockInstance.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
            rockInstance.transform.localScale = transform.localScale * .5f; 

            Rigidbody instanceRB = rockInstance.GetComponent<Rigidbody>();

            if (instanceRB != null)
            {
                instanceRB.AddExplosionForce(Random.Range(1000f, 10000f), transform.position, Random.Range(10f, 20f));
            }
        }

        if(explosionObject != null)
        {
            GameObject explosion = Instantiate(explosionObject);
            explosion.transform.position = transform.position;
        }
    }

    void OnDestroy()
    {
        Projectile.HitEvent -= OnHit;
        Destroy(gameObject);
    }

}
