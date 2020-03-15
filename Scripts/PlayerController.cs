using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerController> HitEvent;

    public float health = 200f;
    public GameObject explosionObject;

    public float currentHealth { get; set; }
    public bool isDead { get; set; }


    public bool isGamePaused { get; set; }

    public string engineThrottleSound;
    public string engineDethrottleSound;
    public string engineIdleSound;
    public string engineOnSound;
    public string engineOffSound;
    public string explosionSound;

    public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);
    public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);

    public float forceMultiplier = 100f;
    //[Range(1f, 100f)]
    //public float steeringSensitivity = 100f;

    public bool enableMouseAndKeyboardControl = true;
    public bool enableControllerControl = false;
    public bool enableKeyboardOnlyControl = false;

    public bool enableVirtualJoystickControl = true;
    public bool enableAutopilotControl = false;

    public float mouseWorldDistance = 1000f;
    public PID pitchPID;
    public PID yawPID;
    public PID rollPID;

    public bool canShoot { get; set; }
    bool fire = false;

    public Rigidbody rb { get; set; }
    Weapon weapon;

    PlayerControls controls;

    float cachedDeltaTime;

    bool throttle = false;
    bool dethrottle = false;
    public float forwardThrottle { get; set; }
    float m_ThrottleSpeed = 0.5f;
    float sideThrottle = 0.0f;

    Vector3 linearInput = Vector3.zero;
    Vector3 appliedLinearForce = Vector3.zero;

    //determine the position of a virtual mouse on the screen and use to calculate a world position
    [HideInInspector]
    public Vector3 mousePos;
    Vector3 mouseWorldPos;
    Vector3 mouseLocalPos;

    Vector2 controllerPos;
    bool convertMousePosX = false;
    bool convertMousePosY = false;

    float roll;

    Vector3 angularInput = Vector3.zero;
    Vector3 appliedAngularForce = Vector3.zero;

    Vector3 screenCentre = Vector3.zero;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Fire1.performed += ctx => fire = true;
        controls.Gameplay.Fire1.canceled += ctx => fire = false;

        controls.Gameplay.Throttle.performed += ctx =>
        {
            throttle = true;
            ThrottleOn();
        };
        controls.Gameplay.Throttle.canceled += ctx =>
        {
            throttle = false;
            ThrottleOff();
        };

        controls.Gameplay.Dethrottle.performed += ctx =>
        {
            dethrottle = true;
            DethrottleOn();
        };
        controls.Gameplay.Dethrottle.canceled += ctx =>
        {
            dethrottle = false;
            DethrottleOff();
        };

        //mouse input
        controls.Gameplay.Yaw.performed += ctx => mousePos.x = ctx.ReadValue<float>();
        controls.Gameplay.Pitch.performed += ctx => mousePos.y = ctx.ReadValue<float>();

        //controller stick input
        controls.Gameplay.Yaw2.started += ctx => convertMousePosX = true;
        controls.Gameplay.Yaw2.performed += ctx => controllerPos.x = ctx.ReadValue<float>();
        controls.Gameplay.Yaw2.canceled += ctx =>
        {
            convertMousePosX = false;
            mousePos.x = Screen.width * 0.5f;
        };
        controls.Gameplay.Pitch2.started += ctx => convertMousePosY = true;
        controls.Gameplay.Pitch2.performed += ctx => controllerPos.y = ctx.ReadValue<float>();
        controls.Gameplay.Pitch2.canceled += ctx =>
        {
            convertMousePosY = false;
            mousePos.y = Screen.height * 0.5f;
        };

        mousePos.z = mouseWorldDistance;

        controls.Gameplay.Roll.performed += ctx => roll = ctx.ReadValue<float>();
        controls.Gameplay.Roll.canceled += ctx => roll = 0f;

    }

    void OnEnable()
    {
        controls.Gameplay.Enable();    
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //move cursor to centre first
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.Confined;

        rb = GetComponent<Rigidbody>();

        weapon = GetComponentInChildren<Weapon>();
        canShoot = true;

        forwardThrottle = 0.0f;

        currentHealth = health;
        isDead = false;

        cachedDeltaTime = Time.deltaTime;

        SoundManager.instance.PlaySound(engineIdleSound);

        Projectile.HitEvent += OnHit;
    }

    // Update is called once per frame
    void Update()
    {
        if(cachedDeltaTime == 0f)
        {
            cachedDeltaTime = Time.deltaTime;
        }

        if (!isGamePaused)
        {
            float deltaTime = Time.deltaTime == 0 ? cachedDeltaTime == 0f ? 0.02f : cachedDeltaTime : Time.deltaTime ; //????

            if (convertMousePosX)
            {
                mousePos.x = Screen.width * 0.5f + (controllerPos.x * Screen.width * 0.5f);
            }

            if(convertMousePosY)
            {
                mousePos.y = Screen.height * 0.5f + (controllerPos.y * Screen.height * 0.5f);
            }

            mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            mouseLocalPos = transform.InverseTransformVector(mouseWorldPos - transform.position).normalized;

            //sideThrottle = Input.GetAxis("Horizontal");
            //linearInput.x = sideThrottle;

            //if (enableVirtualJoystickControl)
            //{
            //    float pitch = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);
            //    float yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);

            //    angularInput.x = -Mathf.Clamp(pitch, -1f, 1f);
            //    angularInput.y = Mathf.Clamp(yaw, -1f, 1f);
            //}

            float target = forwardThrottle;

            if (throttle)
            {
                target = 1.0f;
            }
            else if (dethrottle)
            {
                target = 0.1f;
            }


            forwardThrottle = Mathf.MoveTowards(forwardThrottle, target, deltaTime * m_ThrottleSpeed);
            linearInput.z = forwardThrottle;

            angularInput.x += pitchPID.Update(-mouseLocalPos.y, angularInput.x, deltaTime);
            angularInput.y += yawPID.Update(mouseLocalPos.x, angularInput.y, deltaTime);

            angularInput.y = Mathf.Clamp(angularInput.y, -1f, 1f);
            angularInput.x = Mathf.Clamp(angularInput.x, -1f, 1f);

            angularInput.z += rollPID.Update(-roll, angularInput.z, deltaTime);
            angularInput.z = Mathf.Clamp(angularInput.z, -1f, 1f);
                
            appliedLinearForce = Vector3.Scale(linearForce, linearInput);
            appliedAngularForce = Vector3.Scale(angularForce, angularInput);

            screenCentre.x = Screen.width * 0.5f;
            screenCentre.y = Screen.height * 0.5f;

            if (canShoot)
            {
                if (fire)
                {
                    weapon.Fire(Camera.main.ScreenPointToRay(screenCentre).direction);
                }
                else
                {
                    weapon.ResetFire();
                }
            }

            cachedDeltaTime = Time.deltaTime;

        }
    }

    void ThrottleOn()
    {
        if(!isGamePaused)
        {
            SoundManager.instance.PlaySound(engineThrottleSound);
            SoundManager.instance.PlaySound(engineOnSound);
        }
    }

    void ThrottleOff()
    {
        if (!isGamePaused)
        {
            SoundManager.instance.StopSound(engineThrottleSound);
            SoundManager.instance.PlaySound(engineOffSound);
        }
    }

    void DethrottleOn()
    {
        if (!isGamePaused)
        {
            SoundManager.instance.PlaySound(engineDethrottleSound);
            SoundManager.instance.PlaySound(engineOnSound);
        }
    }

    void DethrottleOff()
    {
        if (!isGamePaused)
        {
            SoundManager.instance.StopSound(engineDethrottleSound);
            SoundManager.instance.PlaySound(engineOffSound);
        }
    }

    private void FixedUpdate()
    {
        rb.AddRelativeForce(appliedLinearForce * forceMultiplier, ForceMode.Force);
        rb.AddRelativeTorque(appliedAngularForce * forceMultiplier, ForceMode.Force);
    }

    void OnHit(float damage, Transform hitTransform)
    {
        if (hitTransform == transform)
        {
            currentHealth -= damage;
            HitEvent.Invoke(this);
        }

        if (currentHealth <= 0f)
        {
            isDead = true;

            if(explosionObject != null)
            {
                Instantiate(explosionObject, transform.position, transform.rotation);
            }

            SoundManager.instance.PlaySound(explosionSound);

            SoundManager.instance.StopSound(engineIdleSound);
            SoundManager.instance.StopSound(engineThrottleSound);
            SoundManager.instance.StopSound(engineDethrottleSound);

            OnDestroy();
        }
    }

    public void OnDestroy()
    {
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

    }
}
