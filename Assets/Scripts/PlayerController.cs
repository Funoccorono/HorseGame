using UnityEngine;
using UnityEngine.InputSystem.HID;
using FIMSpace.FSpine;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float time;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float lookOffsetIncrement = 0f;

    private Rigidbody m_rigidbody;
    Vector3 m_EulerAngleVelocity;
    private float AlignRotLerpDelta;
    public float maximumSpeed;
    public float rotationSpeed;
    public float jumpSpeed = 5f;
    public GameObject[] obstacles;
    public float fallMultiplier = 2.5f;
    private float lookTargetOffset = 0f;
    private bool isTurning = false;

    private RaycastHit chest;
    private RaycastHit hip;

    public float jumpButtonGracePeriod;

    private Vector3 movement;

    private Animator animator;

    private float? jumpButtonPressedTime;
    public float gravity = 9.81f;
    public float forceMagnitude = 100f;
    public float movementSpeed = 0;
    public int movementState = 0;
    public int moving = 0;

    float horizontalInput;
    float verticalInput;

    public Transform groundCheck;
    [SerializeField] Transform hipRaycast, chestRaycast;
    [SerializeField] Collider chestCollider, hipCollider;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Vector3 surfaceNormal;
    public bool isGrounded;

    public bool applyGravity = true;
    public bool surfageAlignment = true;

    private PlayerAnimations playerAnimations;

    [SerializeField] private HoofPositionHelper hoofPos;

    private void Awake()
    {
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
        playerAnimations = GetComponent<PlayerAnimations>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            // get collider of each object
            if (obstacle.GetComponent<Collider>() != null)
            {
                //Physics.IgnoreCollision(obstacle.GetComponent<Collider>(), this.gameObject.GetComponent<CapsuleCollider>());
            }
        }

        playerAnimations.SetGait(movementState);
    }

    // Update is called once per frame
    void Update()
    {
        Ray chestRay = new Ray(chestRaycast.transform.position, -transform.up);
        Ray hipRay = new Ray(hipRaycast.transform.position, -transform.up);

        Physics.Raycast(hipRay, out hip, groundMask);
        if (Physics.Raycast(chestRay, out chest, 0.3f, groundMask)) SurfaceAlignment(chest, hip);

        //Gravity(chest, hip);

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (moving < 1) moving = 1;
            if (movementState == -1)
            {
                movementState += 1;
                rotationSpeed += 15;
            }
            else if (movementState < 4)
            {
                movementState += 1;
                rotationSpeed -= 5;
            }
            playerAnimations.SetGait(movementState);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (movementState == 1) moving = 0;
            if (movementState == 0)
            {
                movementState -= 1;
                rotationSpeed -= 15;
            }
            else if (movementState >= 0)
            {
                movementState -= 1;
                rotationSpeed += 5;
            }
            playerAnimations.SetGait(movementState);
        }

        //jump
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            if (Time.time - jumpButtonPressedTime >= 0.5) animator.SetBool("IsJumping", false);
        }

        if (movementState >= 3 && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (Time.time - jumpButtonPressedTime < jumpButtonGracePeriod) return;
            jumpButtonPressedTime = Time.time;
            animator.SetBool("IsJumping", true);
            m_rigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        movement = gameObject.transform.forward * movementSpeed;

        if (horizontalInput < 0)
        {
            lookTargetOffset -= lookOffsetIncrement;
            lookTargetOffset = Mathf.Clamp(lookTargetOffset, -0.75f, 0.75f);
            lookTarget.localPosition = new Vector3(lookTargetOffset, lookTarget.localPosition.y, lookTarget.localPosition.z);
        }

        if (horizontalInput > 0)
        {
            lookTargetOffset += lookOffsetIncrement;
            lookTargetOffset = Mathf.Clamp(lookTargetOffset, -0.75f, 0.75f);
            lookTarget.localPosition = new Vector3(lookTargetOffset, lookTarget.localPosition.y, lookTarget.localPosition.z);
        }

        if (horizontalInput == 0)
        {
            if (lookTargetOffset < 0) lookTargetOffset += lookOffsetIncrement;
            if (lookTargetOffset > 0) lookTargetOffset -= lookOffsetIncrement;
            lookTargetOffset = Mathf.Clamp(lookTargetOffset, -0.75f, 0.75f);
            lookTarget.localPosition = new Vector3(lookTargetOffset, lookTarget.localPosition.y, lookTarget.localPosition.z);
        }

        if ((horizontalInput >= 0.5f || horizontalInput <= -0.5f) && movementState == 0)
        {
            animator.SetBool("IsTurning", true);
            if (isTurning == false)
            {
                isTurning = true;
                playerAnimations.lerpingToZero = true;
                playerAnimations.lerpingToOne = false;
                playerAnimations.layerStartWeight = animator.GetLayerWeight(3);
                playerAnimations.layerEndWeight = 0;
                playerAnimations.elapsedTime = 0;
            }
        }

        else
        {
            animator.SetBool("IsTurning", false);
            if (isTurning == true)
            {
                isTurning = false;
                playerAnimations.lerpingToOne = true;
                playerAnimations.lerpingToZero = false;
                playerAnimations.layerStartWeight = animator.GetLayerWeight(3);
                playerAnimations.layerEndWeight = 1;
                playerAnimations.elapsedTime = 0;
            }
        }

        if (horizontalInput > 0f)
        {
            animator.SetBool("IsTurningRight", true);
        }

        else animator.SetBool("IsTurningRight", false);
    }

    private void FixedUpdate()
    {
        float horizontalInputFixed = Input.GetAxis("Horizontal");

        Gravity(chest, hip);

        if (moving > 0)
        {
            if (movementState == -1) transform.Rotate(-1 * ((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up));
            else transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        if (horizontalInput != 0)
        {
            if (movementState == -1) transform.Rotate(-1 * ((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up));
            else transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        movementDirection.Normalize();

        m_rigidbody.velocity = movement;
    }

    private void Gravity(RaycastHit chest, RaycastHit hip)
    {
        if (applyGravity)
        {
            m_rigidbody.AddForceAtPosition(Vector3.down * gravity * (m_rigidbody.mass), chest.point, ForceMode.Force);
            m_rigidbody.AddForceAtPosition(Vector3.down * gravity * (m_rigidbody.mass), hip.point, ForceMode.Force);
        }
    }

    private void SurfaceAlignment(RaycastHit chest, RaycastHit hip)
    {
        AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, 10f, time * 2.5f * animCurve.Evaluate(time));

        Quaternion AlignRot = Quaternion.FromToRotation(transform.up, (chest.normal + hip.normal).normalized) * transform.rotation;  //Calculate the orientation to Terrain 
        Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
        Quaternion Target = Inverse_Rot * AlignRot;
        Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * animCurve.Evaluate(time)); //Calculate the Delta Align Rotation

        transform.rotation *= Delta;
    }
}
