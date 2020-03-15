using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class ProjectileSpawner : MonoBehaviour
{
    public float startDelay = 1.0f;
    public float repeatInterval = 0.5f;
    Weapon weapon;

    // Start is called before the first frame update
    void Start()
    {
        weapon = GetComponent<Weapon>();

        InvokeRepeating("Fire", startDelay, repeatInterval);
    }

    void Fire()
    {
        weapon.Fire(transform.forward);
        weapon.ResetFire();

    }
}
