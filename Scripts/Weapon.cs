using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Projectile projectile;
    public Material trailMaterial;
    public Color projectileColor;
    public Gradient projectileColorGradient;
    public float trailStartWidth;
    public float trailEndWidth;
    public float trailMinVertDistance;
    public Light muzzleFlash;

    public bool adjustProjectileSphereCast = false;
    public string shootSound;

    public Vector3 firePosition;
    public Vector3 fireDirection { get; set; }

    public float minProjectileSpeed = 100f;
    public float maxProjectileSpeed = 150f;

    [Range(.00001f, 1f)]
    public float firePositionOffset = 0.2f;
    [Range(.00001f, .1f)]
    public float spreadAngleOffset = 2.0f;

    public bool isFullAuto = false;
    public float rateOfFire = 0.25f;

    bool fired = false;
    float speed = 0f;
    float currentTime = 0f;

    Vector3 initialDirection = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.localPosition = firePosition;
            muzzleFlash.color = projectileColor;
            muzzleFlash.enabled = false;
        }
    }

    void Update()
    {
        projectile.adjustSphereCastRadius = adjustProjectileSphereCast;
    }

    public void Fire(Vector3 direction)
    {
        if(fired == false)
        {
            SoundManager.instance.PlaySound(shootSound);

            if(muzzleFlash != null)
            {
                muzzleFlash.enabled = true;
            }

            Vector3 firePos = transform.TransformVector(firePosition + Random.insideUnitSphere * firePositionOffset);

            fireDirection = direction + Random.onUnitSphere * spreadAngleOffset;

            speed = Random.Range(minProjectileSpeed, maxProjectileSpeed);

            Projectile p = Instantiate(projectile, 
                transform.position + firePos,
                Quaternion.LookRotation(fireDirection * speed));

            p.SetVelocity(fireDirection, speed);

            p.GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", projectileColor);

            p.trailMaterial = trailMaterial;
            p.projectileColorGradient = projectileColorGradient;
            p.trailStartWidth = trailStartWidth;
            p.trailEndWidth = trailEndWidth;
            p.trailMinVertDistance = trailMinVertDistance;

            p.SetTrailParameters();

            fired = true;
        }

        if(isFullAuto)
        {
            currentTime += Time.deltaTime;
            if(currentTime >  rateOfFire)
            {
                ResetFire();
            }
        }
    }

    public void ResetFire()
    {
        fired = false;
        currentTime = 0.0f;

        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.TransformVector(firePosition), Vector3.one);

        Gizmos.DrawLine(transform.position + transform.TransformVector(firePosition), transform.position + fireDirection);
    }
}
