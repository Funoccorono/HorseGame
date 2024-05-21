using UnityEngine;
using UnityEngine.InputSystem.HID;

public class ThirdPersonController : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    Vector3 m_EulerAngleVelocity;
    private float AlignRotLerpDelta;
    public float maximumSpeed;
    public float rotationSpeed;
    public float jumpSpeed = 5f;
    public GameObject[] obstacles;
    public float fallMultiplier = 2.5f;

    private RaycastHit chest;
    private RaycastHit hip;

    public float jumpButtonGracePeriod;

    public Transform cameraTransform;

    private Animator animator;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float time;

    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    public float gravity = -9.81f;
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
                Physics.IgnoreCollision(obstacle.GetComponent<Collider>(), this.gameObject.GetComponent<CapsuleCollider>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        Ray chestRay = new Ray(chestRaycast.transform.position, -transform.up);
        Ray hipRay = new Ray(hipRaycast.transform.position, -transform.up);

        Physics.Raycast(hipRay, out hip, groundMask);
        if (Physics.Raycast(chestRay, out chest, groundMask)) SurfaceAlignment(chest, hip);

        //Gravity(chest, hip);

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (moving < 1) moving = 1;
            if (movementState < 4) movementState += 1;
            SetGait();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            //speed -= 10;
            if (movementState == 1) moving = 0;
            if (movementState >= 1) movementState -= 1;
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
            //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        if (horizontalInput != 0)
        {
            //transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInput * Vector3.up);
            if (isGrounded)
            {
                transform.Rotate((rotationSpeed * Time.deltaTime) * horizontalInputFixed * Vector3.up);
                SurfaceAlignment(chest, hip);
            }
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        if (Input.GetAxisRaw("Horizontal") < 0) //Turning left
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
        }

        //speed = inputMagnitude * maximumSpeed;
        //movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
        movementDirection.Normalize();
    }

    private void Gravity(RaycastHit chest, RaycastHit hip)
    {
        m_rigidbody.AddForceAtPosition(Vector3.down * 9.81f * (m_rigidbody.mass / 2), chest.point, ForceMode.Force);
        m_rigidbody.AddForceAtPosition(Vector3.down * 9.81f * (m_rigidbody.mass / 2), hip.point, ForceMode.Force);
    }

    private void SurfaceAlignment(RaycastHit chest, RaycastHit hip)
    {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, 10f, time * 2.5f * animCurve.Evaluate(time));

            Quaternion AlignRot = Quaternion.FromToRotation(transform.up, (chest.normal + hip.normal).normalized) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * animCurve.Evaluate(time)); //Calculate the Delta Align Rotation

            transform.rotation *= Delta; 
        //}
    }

    private void SetGait()
    {
        if (movementState == 1)
        {
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsTrotting", false);
            //speed = 10;
        }
        else if (movementState == 2)
        {
            animator.SetBool("IsTrotting", true);
            animator.SetBool("IsCantering", false);
            speed = 4;
        }
        else if (movementState == 3)
        {
            animator.SetBool("IsCantering", true);
            animator.SetBool("IsGalloping", false);
            speed = 6;
        }
        else if (movementState == 4)
        {
            animator.SetBool("IsGalloping", true);
            speed = 12;
        }
        else
        {
            animator.SetBool("IsWalking", false);
            speed = 0;
        }
    }
}
