using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyShip : MonoBehaviour
{
    public static event Action<EnemyShip> HitEvent;
    public static event Action<EnemyShip> DestroyEvent;

    public bool isAttacking { get; set; }
    //public float maxVelocity;
    public float health = 100f;
    public float currentHealth { get; set; }

    public float attackDuration { get; set; }
    float attackTimer;

    public bool isGamePaused { get; set; }

    public GameObject explosionObject;
    public string explosionSound;

    public float maxSpeed;
    public float maxForce;
    public float mass;

    //need to pass this in somehow
    public float pursuitTargetMaxSpeed = 10f;
    [Range(1f, 10f)]
    public float pursuitMultiplier = 1.0f;

    public Transform targetTransform;
    public float targetPointOffsetDistance = 5.0f;

    Vector3 currentVelocity = Vector3.zero;
    Vector3 desiredVelocity = Vector3.zero;
    Vector3 steeringForce = Vector3.zero;
    Vector3 finalVelocity = Vector3.zero;

    //arrive variables
    public bool arrive = false;
    public float slowingDistance = 200f;
    public float stoppingDistance = 5f;

    //pursuit variables
    public bool pursue = false;
    Vector3 pursuitPosition = Vector3.zero;
    Vector3 distanceFromTargetVector = Vector3.zero;

    //wander variables
    public bool wander = false;
    public float wanderCircleDistance = 1.0f;
    public float wanderCircleRadius = 5.0f;
    public float wanderRandomPointRadius = 3.0f;

    public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);
    public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);
    public float minimumBrakeDistance = 75f;
    public float minimumStopDistance = 25f;

    public float collisionPushForce = 50f;

    public bool canShoot = true;
    [Range(0f, 1f)]
    public float engageTargetMinimumAngleThreshold = .9f;

    public PID pitchPID;
    public PID yawPID;
    public PID rollPID;

    public bool drawDebug = false;

    Rigidbody rb;
    AudioSource engineSoundAudioSource;

    float cachedDeltaTime; //when setting Time.timeScale to 0, need this so Time.deltaTime is not 0

    Vector3 targetPointOffset = Vector3.zero;

    float forwardThrottle = 0.0f;
    float m_ThrottleSpeed = 0.5f;

    Vector3 linearInput = Vector3.zero;
    Vector3 appliedLinearForce = Vector3.zero;

    Vector3 angularInput = Vector3.zero;
    Vector3 appliedAngularForce = Vector3.zero;

    Vector3 targetPosition = Vector3.zero;
    Vector3 targetAimPosition = Vector3.zero;
    Vector3 directionToTarget = Vector3.zero;
    float distanceFromTarget;

    Vector3 wanderCirclePosition = Vector3.zero;
    Vector3 wanderDisplacement = Vector3.zero;
    Vector3 wanderRandomPoint = Vector3.zero;
    Vector3 wanderForce = Vector3.zero;
    float wanderRandomPointMinDistance = 0f;

    Weapon weapon;
    bool engageTarget = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag += Random.Range(.01f, .1f);

        engineSoundAudioSource = GetComponent<AudioSource>();
        engineSoundAudioSource.pitch = Random.Range(.95f, 1.05f);
        engineSoundAudioSource.priority = Random.Range(250, 280);

        targetPointOffset = Random.onUnitSphere * targetPointOffsetDistance;

        weapon = GetComponent<Weapon>();
        weapon.isFullAuto = true;

        currentHealth = health;
        isAttacking = false;

        attackTimer = 0f;

        Projectile.HitEvent += OnHit;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGamePaused == false)
        {
            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
            }
            else
            {
                targetPosition = Vector3.zero;
            }

            if (arrive)
            {
                Arrival(targetPosition);
            }
            if (pursue)
            {
                Pursuit(targetPosition);
            }
            if (wander)
            {
                Wander();
            }

            if (isAttacking)
            {
                attackTimer += Time.deltaTime;

                if (attackTimer < attackDuration)
                {
                    wander = false;
                    arrive = true;
                }
                else
                {
                    isAttacking = false;
                    attackTimer = 0f;
                    arrive = false;
                    wander = true;
                }
            }

            UpdateShip();

            if (targetTransform != null && canShoot)
            {
                targetAimPosition = targetTransform.position;
                EngageTarget();
            }

            cachedDeltaTime = Time.deltaTime;
        }
 
    }

    private void FixedUpdate()
    {
        if (IsVector3Valid(appliedLinearForce))
        {
            rb.AddRelativeForce(appliedLinearForce, ForceMode.Force);
        }
        //if (IsVector3Valid(appliedAngularForce))
        //{
            rb.AddRelativeTorque(appliedAngularForce, ForceMode.Force);
        //}

        //FixedUpdateSteering();
    }

    void Wander()
    {
        wanderCirclePosition = transform.forward * wanderCircleDistance;
        if (wanderDisplacement == Vector3.zero)
        {
            //get first random point on wander sphere
            wanderDisplacement = wanderCirclePosition + Random.onUnitSphere * wanderCircleRadius;
        }
        else
        {
            wanderRandomPointMinDistance = wanderRandomPointRadius * 2f;
            distanceFromTarget = (transform.position - wanderDisplacement).magnitude;

            if (distanceFromTarget < wanderRandomPointMinDistance)
            {
                //get a random point on a unit sphere at the wanderDisplacement point, projected back onto the wander sphere
                wanderRandomPoint = wanderDisplacement + Random.onUnitSphere * wanderRandomPointRadius;
                wanderDisplacement = (wanderRandomPoint - wanderCirclePosition).normalized * wanderCircleRadius + wanderCirclePosition;
            }

            targetPosition = wanderDisplacement;

            Arrival(targetPosition);
        }

        //can't resolve jitter
        //wanderForce = wanderCirclePosition + wanderDisplacement;
        //desiredVelocity = (currentVelocity + wanderForce).normalized * maxSpeed;
        //distanceFromTarget = (transform.position - wanderRandomPoint).magnitude;
        //steeringForce = desiredVelocity - currentVelocity;

        //UpdateSteeringForce();
    }

    void Pursuit(Vector3 target)
    {
        distanceFromTargetVector = target - transform.position;
        float T = distanceFromTargetVector.magnitude / (maxSpeed + pursuitTargetMaxSpeed);
        pursuitPosition = target + targetTransform.GetComponent<Rigidbody>().velocity * (T * pursuitMultiplier);

        targetPosition = pursuitPosition;

        Arrival(targetPosition);
    }

    void Arrival(Vector3 target)
    {
        desiredVelocity = target - transform.position;
        distanceFromTarget = desiredVelocity.magnitude;

        if (distanceFromTarget < minimumBrakeDistance)
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed * (distanceFromTarget / slowingDistance);
        }
        else
        {
            desiredVelocity = desiredVelocity * maxSpeed;
        }

        steeringForce = desiredVelocity - currentVelocity;

        UpdateSteeringForce();
    }

    void Flee(Vector3 target)
    {
        desiredVelocity = (transform.position - target).normalized * maxSpeed;
        steeringForce = desiredVelocity - currentVelocity;

        UpdateSteeringForce();
    }

    void Seek(Vector3 target)
    {
        desiredVelocity = (target - transform.position).normalized * maxSpeed;
        steeringForce = desiredVelocity - currentVelocity;

        UpdateSteeringForce();
    }

    void UpdateSteeringForce()
    {
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
        steeringForce /= mass;

        finalVelocity = Vector3.ClampMagnitude(currentVelocity + steeringForce, maxSpeed);
        currentVelocity = finalVelocity;
    }

    //void UpdateSteering()
    //{
    //    transform.position = transform.position + currentVelocity * Time.deltaTime;

    //    if (currentVelocity != Vector3.zero)
    //    {
    //        transform.rotation = Quaternion.LookRotation(currentVelocity, transform.up);
    //    }
    //}

    void FixedUpdateSteering()
    {
        if (IsVector3Valid(currentVelocity))
        {
            rb.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);
            if (distanceFromTarget > stoppingDistance)
            {
                rb.MoveRotation(Quaternion.LookRotation(currentVelocity));
            }
        }
    }

    /// <summary>
    /// PID Control Update
    /// </summary>
	void UpdateShip()
    {
        float deltaTime = Time.deltaTime == 0 ? cachedDeltaTime : Time.deltaTime;

        float throttleTarget = forwardThrottle;

        directionToTarget = targetPosition - transform.position;
        distanceFromTarget = directionToTarget.magnitude;

        if (distanceFromTarget > minimumBrakeDistance)
        {
            throttleTarget = 1.0f;
        }
        else
        {
            throttleTarget = 0.2f;
        }

        if (distanceFromTarget < minimumStopDistance)
        {
            throttleTarget = 0.0f;
        }

        forwardThrottle = Mathf.MoveTowards(forwardThrottle, throttleTarget, deltaTime * m_ThrottleSpeed);
        linearInput.z = forwardThrottle;

        Vector3 localWorldPosition = transform.InverseTransformVector((targetPosition + targetPointOffset) - transform.position).normalized;

        angularInput.x += pitchPID.Update(-localWorldPosition.y, angularInput.x, deltaTime);
        angularInput.y += yawPID.Update(localWorldPosition.x, angularInput.y, deltaTime);

        angularInput.y = Mathf.Clamp(angularInput.y, -1f, 1f);
        angularInput.x = Mathf.Clamp(angularInput.x, -1f, 1f);

        //float roll = Input.GetAxis("Roll"); //???

        //angularInput.z += rollPID.Update(-roll, angularInput.z, Time.deltaTime);
        //angularInput.z = Mathf.Clamp(angularInput.z, -1f, 1f);

        appliedLinearForce = Vector3.Scale(linearForce, linearInput);
        appliedAngularForce = Vector3.Scale(angularForce, angularInput);
    }

    void EngageTarget()
    {
        directionToTarget = targetAimPosition - transform.position;
        if (Vector3.Dot(transform.forward, directionToTarget.normalized) > engageTargetMinimumAngleThreshold)
        {
            engageTarget = true;
        }
        else
        {
            engageTarget = false;
        }

        if (engageTarget)
        {
            weapon.Fire((targetAimPosition - transform.position).normalized);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        //Vector3 pushVector = Vector3.ProjectOnPlane(transform.forward, collision.GetContact(0).normal);

        rb.AddRelativeForce(collision.GetContact(0).normal * collisionPushForce, ForceMode.Impulse);
        rb.AddRelativeTorque(collision.GetContact(0).normal * collisionPushForce, ForceMode.Impulse);
    }

    void OnHit(float damage, Transform hitTransform)
    {
        if (hitTransform == transform)
        {
            currentHealth -= damage;
            if(currentHealth > 0f)
            {
                HitEvent.Invoke(this);
            }
        }

        if (currentHealth <= 0f)
        {
            if(explosionObject != null)
            {
                GameObject explosion = Instantiate(explosionObject);
                explosion.transform.position = transform.position;
            }

            SoundManager.instance.PlaySound(explosionSound);

            if (DestroyEvent != null)
            {
                DestroyEvent.Invoke(this);
            }

            OnDestroy();
        }
    }

    public void OnDestroy()
    {
        //if (DestroyEvent != null)
        //{
        //    DestroyEvent.Invoke(this);
        //}

        Projectile.HitEvent -= OnHit;
        Destroy(gameObject);
    }

    bool IsVector3Valid(Vector3 v)
    {
        return float.IsNaN(v.x) == false &&
                float.IsNaN(v.y) == false &&
                float.IsNaN(v.z) == false;

    }

    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pursuitPosition, 1.0f);
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(targetPosition + targetPointOffset, 1.0f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + transform.forward * wanderCircleDistance, wanderCircleRadius);
            Gizmos.color = Color.cyan;
            //Gizmos.DrawWireSphere(transform.position + wanderRandomPoint, wanderRandomPointRadius);
            Gizmos.DrawWireSphere(wanderDisplacement, wanderRandomPointRadius);
            Gizmos.color = Color.white;
            //Gizmos.DrawLine(transform.position, transform.position + wanderRandomPoint);
            Gizmos.DrawLine(transform.position, wanderDisplacement);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + currentVelocity * currentVelocity.magnitude);
        }

    }

}
