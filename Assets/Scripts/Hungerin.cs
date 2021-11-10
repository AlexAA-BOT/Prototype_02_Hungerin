using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hungerin : MonoBehaviour
{
    [SerializeField] private Rigidbody m_RigidBody;
    [SerializeField] private Transform m_Target;
    [SerializeField] LineRenderer m_LineRenderer;

    [Space]
    [SerializeField] EssencialProperties m_EssencialProperties;

    [Space]
    [Header("Movement physics")]
    [SerializeField] private float speed = 300f;
    
    [Space]
    [Header("Jump physics")]
    [SerializeField] private float jumpSpeed = 30f;
    [SerializeField] private float jumpForwardSpeed = 3f;
    
    [Space]
    [Header("Ground physics")]
    [SerializeField] private Transform m_Grounded;
    private bool isGrounded;
    [SerializeField] private float groundRadius = 0.5f;
    [SerializeField] private LayerMask groundMask;
    
    [Space]
    [Header("Rotation physics")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    
    [Space]
    [Header("Gravity physics")]
    [SerializeField] private float gravityScale = 40.0f;
    [SerializeField] private float globalGravity = -9.81f;

    [Space]
    [Header("Eat physics")]
    [SerializeField] private float maxTongueDistance = 10.0f;
    [SerializeField] [Range(0.5f, 3.5f)] private float maxSize = 2.5f;
    [SerializeField] [Range(0.5f, 3.5f)] private float minSize = 0.5f;
    [SerializeField] private float launchDirectionAgainstMassForce = 2000f;
    [SerializeField] private float launchUpDirectionAgainstMassForce = 1000f;
    private bool eatInputButton = false;
    private float scalarMultiplier = 0.01f;
    private Vector3 initialScale = new Vector3(1f,1f,1f);
    private bool playerIsForced = false;

    [Space]
    [Header("Spit physics")]
    [SerializeField] Transform m_SpitSpawn;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    //[SerializeField] private LayerMask[] eatenObjectsMask;
    private bool spitInputButton = false;
    //private GameObject[] eatenObjects = null;

    private void Awake()
    {
        m_RigidBody.mass = m_EssencialProperties.weight;
    }

    private void Start()
    {
        m_LineRenderer.enabled = false;
    }

    // Here we control the player
    private void Update()
    {
        if (Input.GetButtonDown("LaunchTongue"))
        {
            eatInputButton = true;
        }
        if (Input.GetButtonDown("Spit"))
        {
            spitInputButton = true;
        }
    }

    // Here we make physics
    private void FixedUpdate()
    {
        NormalMovement();
        UseTongue();
        UseSpit();
    }

    private void NormalMovement()
    {
        // Inputs movement

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 directionRotation = m_Target.position - transform.position;
        directionRotation = directionRotation.normalized;

        // Rotation on forward direction
        float targetAngle = Mathf.Atan2(directionRotation.x, directionRotation.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        isGrounded = Physics.CheckSphere(m_Grounded.position, groundRadius, groundMask);
        
        if (isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                m_RigidBody.AddForce(new Vector3(direction.x * jumpForwardSpeed, jumpSpeed, direction.z * jumpForwardSpeed), ForceMode.Impulse);
            }

            if (!playerIsForced)
            {
                m_RigidBody.velocity = direction * speed * Time.fixedDeltaTime;
            }
        }
        else
        {
            Vector3 gravity = globalGravity * gravityScale * Vector3.up;
            m_RigidBody.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    private void UseTongue()
    {
        if (eatInputButton)
        {
            Vector3 direction = m_Target.position - transform.position;
            Ray raycastTarget = new Ray(transform.position, direction.normalized);
            RaycastHit hit;

            if (Physics.Raycast(raycastTarget, out hit, maxTongueDistance))
            {
                DrawLineRenderer(hit.point);

                if (hit.collider.tag == "CanBeEaten")
                {
                    // If player is bigger than this gameobject
                    if(m_EssencialProperties.largeSize >= hit.collider.gameObject.GetComponent<Objects>().GetLargeSize())
                    {
                        hit.collider.gameObject.GetComponent<Objects>().MoveToPlayer(transform.position);
                        
                        SumSize(hit.collider.gameObject.GetComponent<Objects>().GetLargeSize());
                        MaxScalarSize(hit.collider.gameObject.GetComponent<Objects>().GetLargeSize());
                        SumWeight(hit.collider.gameObject.GetComponent<Objects>().GetWeight());
                        SetNewMass(m_EssencialProperties.weight);
                    }
                    else
                    {
                        playerIsForced = true;
                        LaunchToDirection(hit.collider.gameObject.transform.position);
                        StartCoroutine("PlayerIsForced");
                    }
                }
                eatInputButton = false;
            }
        }
    }
    private void UseSpit()
    {
        if (spitInputButton)
        {
            if(transform.localScale.x >= minSize)
            {
                // Spit
                GameObject bullet = Instantiate(bulletPrefab, m_SpitSpawn.position, m_SpitSpawn.rotation);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

                // Spit force
                Vector3 direction = m_Target.position - m_SpitSpawn.position;
                bulletRb.AddForce(direction.normalized * bulletSpeed, ForceMode.Impulse);

                SumSize(-2f);
                MinScalarSize(-2f);
                SumWeight(-2f);
                SetNewMass(m_EssencialProperties.weight);
            }

            spitInputButton = false;
        }
    }

    private void MaxScalarSize(float _largeSize)
    {
        if (transform.localScale.x <= maxSize)
        {
            transform.localScale +=
            new Vector3(
                _largeSize * scalarMultiplier,
                _largeSize * scalarMultiplier,
                _largeSize * scalarMultiplier);

            if (transform.localScale.x >= maxSize)
            {
                transform.localScale =
                    new Vector3(
                        maxSize,
                        maxSize,
                        maxSize);
            }
        }
    }
    private void MinScalarSize(float _largeSize)
    {
        if (transform.localScale.x >= minSize)
        {
            transform.localScale +=
            new Vector3(
                _largeSize * scalarMultiplier,
                _largeSize * scalarMultiplier,
                _largeSize * scalarMultiplier);

            if (transform.localScale.x <= minSize)
            {
                transform.localScale =
                    new Vector3(
                        minSize,
                        minSize,
                        minSize);
            }
        }
    }


    private void SumSize(float sizeEaten)
    {
        m_EssencialProperties.largeSize += sizeEaten;
    }
    private void SumWeight(float weightEaten)
    {
        m_EssencialProperties.weight += weightEaten;
    }
    private void SetNewMass(float weightEaten)
    {
        m_RigidBody.mass = weightEaten;
    }

    private void LaunchToDirection(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction = direction.normalized;
        
        m_RigidBody.AddForce(direction * launchDirectionAgainstMassForce + Vector3.up * launchUpDirectionAgainstMassForce, ForceMode.Acceleration);
    }

    private void DrawLineRenderer(Vector3 point)
    {
        m_LineRenderer.enabled = true;
        m_LineRenderer.SetPosition(0, transform.position);
        m_LineRenderer.SetPosition(1, point);

        StartCoroutine("DisableTongue");
    }

    IEnumerator DisableTongue()
    {
        yield return new WaitForSeconds(1f);
        m_LineRenderer.enabled = false;
    }    
    IEnumerator PlayerIsForced()
    {
        yield return new WaitForSeconds(0.15f);
        playerIsForced = false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_Grounded.transform.position, groundRadius);

        Gizmos.DrawLine(transform.position, m_Target.position);
    }
}
