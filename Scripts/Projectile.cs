using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static event Action<float, Transform> HitEvent;

    public TrailRenderer trail { get; set; }
    public Material trailMaterial { get; set; }
    public Gradient projectileColorGradient { get; set; }
    public float trailStartWidth { get; set; }
    public float trailEndWidth { get; set; }
    public float trailMinVertDistance { get; set; }


    public float damage = 10f;
    public float lifeTime = 1f;
    public bool adjustSphereCastRadius = false;
    public float startSphereCastRadius = 2.0f;
    public float endSphereCastRadius = 5.0f;

    public LayerMask collisionMask;
    public bool drawDebugLines = false;

    Vector3 projectedPosition = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    float currentLifeTime = 0.0f;

    Vector3 startPosition = Vector3.zero;
    float currentSphereCastRadius = 0f;
    float scaleStartTime = 0f;

    private void Awake()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;

        scaleStartTime = Time.time;
        currentSphereCastRadius = startSphereCastRadius;

    }

    public void SetTrailParameters()
    {
        trail.material = trailMaterial;
        trail.colorGradient = projectileColorGradient;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        trail.minVertexDistance = trailMinVertDistance;
        trail.numCapVertices = 3;
        trail.numCornerVertices = 1;
        trail.receiveShadows = false;
        trail.time = lifeTime;
        trail.alignment = LineAlignment.View;
        trail.generateLightingData = false;
        trail.emitting = true;
    }

    // Update is called once per frame
    void Update()
    {
        projectedPosition.x = transform.position.x + velocity.x * Time.deltaTime;
        projectedPosition.y = transform.position.y + velocity.y * Time.deltaTime;
        projectedPosition.z = transform.position.z + velocity.z * Time.deltaTime;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, currentSphereCastRadius, projectedPosition - transform.position, out hit, (projectedPosition - transform.position).magnitude, collisionMask))
        {
            transform.position = hit.point;
            OnHit(hit.transform);
        }
        else
        {
            transform.position = projectedPosition;
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        currentLifeTime += Time.deltaTime;

        if(adjustSphereCastRadius)
        {
            float currentScaleTime = (Time.time - scaleStartTime) / lifeTime;
            currentSphereCastRadius = Mathf.Lerp(startSphereCastRadius, endSphereCastRadius, currentScaleTime);
        }
 
        if (currentLifeTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetVelocity(Vector3 dir, float speed)
    {
        velocity = dir * speed;
    }

    void OnHit(Transform hitTransform)
    {
        if(HitEvent != null)
        {
            HitEvent.Invoke(damage, hitTransform);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if(drawDebugLines)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPosition, projectedPosition);
            Gizmos.DrawWireSphere(projectedPosition, currentSphereCastRadius);

        }
    }

}
