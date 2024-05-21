using UnityEngine;
using UnityEngine.InputSystem.HID;
using FIMSpace.FSpine;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float time;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip[] stepSounds;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float lookOffsetIncrement = 0f;
    [SerializeField] private FSpineAnimator spineAnimator;
    [SerializeField] private Transform fLeft, fRight, rLeft, rRight;
    [SerializeField] private ParticleSystem tracks;
    [SerializeField] private ParticleSystem rLeftTracks;
    [SerializeField] private float trackOffset = 0f;

    private Rigidbody m_rigidbody;
    Vector3 m_EulerAngleVelocity;
    private float AlignRotLerpDelta;
    public float maximumSpeed;
    public float rotationSpeed;
    public float jumpSpeed = 5f;
    public GameObject[] obstacles;
    public float fallMultiplier = 2.5f;
    private float lookTargetOffset = 0f;
    private float originalValue;
    private ParticleSystem.MainModule psMain;

    private RaycastHit chest;
    private RaycastHit hip;

    public float jumpButtonGracePeriod;

    public Transform cameraTransform;

    private Vector3 movement;

    private Animator animator;

    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    public float gravity = 9.81f;
    public float forceMagnitude = 100f;
    public float speed = 0;
    public float movementState = 0;
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

    [SerializeField] private HoofPositionHelper hoofPos;

    private void Awake()
    {
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set the angular velocity of the Rigidbody (rotating around the Y axis, 100 deg/sec)
        m_EulerAngleVelocity = new Vector3(0, 100, 0);

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

        originalValue = spineAnimator.AngleLimit;
        psMain = tracks.main;
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
            //moving = 1;
            //movementState = 3;
            if (moving < 1) moving = 1;
            if (movementState < 4)
            {
                movementState += 1;
                rotationSpeed -= 5;
            }
            SetGait();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            //movementState = 0;
            //moving = 0;
            //speed -= 10;
            if (movementState == 1) moving = 0;
            if (movementState >= 1)
            {
                movementState -= 1;
                rotationSpeed += 5;
            }
            SetGait();
        }

        //jump
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            if (Time.time - jumpButtonPressedTime >= 0.5) animator.SetBool("IsJumping", false);
        }

        /*if (!isGrounded)
        {
            animator.SetBool("IsJumping", true);
        }*/


        if (movementState >= 3 && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (Time.time - jumpButtonPressedTime < jumpButtonGracePeriod) return;
            jumpButtonPressedTime = Time.time;
            animator.SetBool("IsJumping", true);
            m_rigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        movement = gameObject.transform.forward * speed;

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
        }

        else animator.SetBool("IsTurning", false);

        if (horizontalInput > 0f)
        {
            animator.SetBool("IsTurningRight", true);
        }

        else animator.SetBool("IsTurningRight", false);

        //Setting AngleLimit gradually to 0 when not moving forward
        if (movementState == 0 && spineAnimator.AngleLimit != 0)
        {
            spineAnimator.AngleLimit -= 0.1f;
            spineAnimator.AngleLimit = Mathf.Clamp(spineAnimator.AngleLimit, 0, originalValue);
        }

        if (movementState != 0 && spineAnimator.AngleLimit != originalValue)
        {
            spineAnimator.AngleLimit += 0.1f;
            spineAnimator.AngleLimit = Mathf.Clamp(spineAnimator.AngleLimit, 0, originalValue);
        }

        /*if (moving > 0)
        {
            //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        if (horizontalInput != 0)
        {
            //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }*/

        //-------------------
        //SIDEWAYS MOVEMENT
        /*
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        if (horizontalInput < 0) //Turning left
        {
            animator.SetFloat("Blend", 0, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Left", true);
        }
        else if (horizontalInput > 0) //Turning right
        {
            animator.SetFloat("Blend", 1, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Right", true);
        }
        else
        {
            animator.SetFloat("Blend", 0.5f, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Right", false);
            animator.SetBool("Turning_Left", false);
        }

        //speed = inputMagnitude * maximumSpeed;
        //movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
        movementDirection.Normalize();
        */

        //------------------

        /*private void OnControllerColliderHit(ControllerColliderHit hit)
        {
        var rigidBody = hit.collider.attachedRigidbody;

        if (rigidBody != null)
        {
            var forceDirection = hit.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();

            rigidBody.AddForceAtPosition(forceDirection * forceMagnitude, transform.position, ForceMode.Impulse);

            rigidBody.GetComponent<ObstacaleCollider>().DisableCollider();
        }

        private void OnApplicationFocus(bool focus)
        {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }*/
    }

    private void FixedUpdate()
    {
        float horizontalInputFixed = Input.GetAxis("Horizontal");

        Gravity(chest, hip);

        if (moving > 0)
        {
            transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        if (horizontalInput != 0)
        {
            transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        /*if (Input.GetAxisRaw("Horizontal") < 0) //Turning left
        {
            animator.SetFloat("Blend", 0, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Left", true);
        }
        else if (Input.GetAxisRaw("Horizontal") > 0) //Turning right
        {
            animator.SetFloat("Blend", 1, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Right", true);
        }
        else
        {
            animator.SetFloat("Blend", 0.5f, 0.5f, Time.deltaTime);
            animator.SetBool("Turning_Right", false);
            animator.SetBool("Turning_Left", false);
        }*/

        //m_rigidbody.MovePosition(Vector3.forward * 2);

        //speed = inputMagnitude * maximumSpeed;
        //movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
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

    private void SetGait()
    {
        if (movementState == 1)
        {
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsTrotting", false);
            speed = 1.5f;
        }
        else if (movementState == 2)
        {
            animator.SetBool("IsTrotting", true);
            animator.SetBool("IsCantering", false);
            speed = 3;
        }
        else if (movementState == 3)
        {
            animator.SetBool("IsCantering", true);
            animator.SetBool("IsGalloping", false);
            speed = 5;
        }
        else if (movementState == 4)
        {
            animator.SetBool("IsGalloping", true);
            speed = 10;
        }
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsCantering", false);
            speed = 0;
        }
    }

    public void Step1()
    {
        audioSources[0].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[0].PlayOneShot(audioSources[0].clip);
    }

    public void Step2()
    {
        audioSources[1].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[1].PlayOneShot(audioSources[1].clip);
    }

    public void Step3()
    {
        audioSources[2].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[2].PlayOneShot(audioSources[2].clip);
    }

    public void Step4()
    {
        audioSources[3].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[3].PlayOneShot(audioSources[3].clip);
    }
    
    public void FLeft()
    {
        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(fLeft.position.x, chestRaycast.position.y + trackOffset, fLeft.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position
            
        };

        tracks.Emit(ptrack, 1);
    }

    public void FRight()
    {
        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(fRight.position.x, chestRaycast.position.y + trackOffset, fRight.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void RLeft()
    {
        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(rLeft.position.x, hipRaycast.position.y + trackOffset, rLeft.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        //Debug.Log(rLeft.transform.position);
        hoofPos.PrintPosition();

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void RRight()
    {
        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(rRight.position.x, hipRaycast.position.y + trackOffset, rRight.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void StartJump()
    {
        speed += 2;
    }

    public void EndJump()
    {
        speed -= 2;
    }
}
