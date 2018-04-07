using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float maxRunMultiplier = 2f;
    [SerializeField]
    private float runAcceleration = 0.02f;
    [SerializeField]
    private float crouchMultiplier = 0.8f;
    [SerializeField]
    private float maxSlideMultiplier = 2.6f;
    [SerializeField]
    private float slideFriction = 0.01f;
    [SerializeField]
    private float wallRunMultiplier = 1.1f;
    [SerializeField]
    private float lookSpeed = 0.5f;
    [SerializeField]
    private float gravity = -1f;
    [SerializeField]
    private float jumpPower = 1f;
    [SerializeField]
    private float maxWallJumpPower = 10f;

    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private Transform bodyCheck;
    [SerializeField]
    private Transform headCheck;
    [SerializeField]
    private Transform feetCheck;

    private CharacterController cc;
    private GameObject cam;
    private GameObject camArm;
    private Animator anim;

    private Vector3 velocity;
    private Vector3 slideDirection;
    private Vector3 wallRunDirection;
    private float runMultiplier = 1;
    private float slideMultiplier;
    private float wallRunSpeed;
    private bool fastMode;
    private bool slideMode;
    private bool crouchMode;
    private bool wallRunMode;
    private GameObject wallRunObject;
    private GameObject wallJumpObject;
    private bool climbMode;
    private GameObject climbedObject;
    private bool slideAfterJump;
    private bool upFromJumpSlide;
    private bool groundedLastFrame;
    private bool airJumped;
    private bool wallJumped;
    private Vector3 wallRunNormal;
    private Vector3 wallClimbNormal;
    private Vector3 wallJumpNormal;
    private float wallJumpPower;

    private void Awake()
    {
        Application.targetFrameRate = 144;
        cc = GetComponent<CharacterController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        camArm = GameObject.FindGameObjectWithTag("CameraArm");
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float forwardInput = Input.GetAxisRaw("Vertical");
        float strafeInput = Input.GetAxisRaw("Horizontal");
        bool runInput = Input.GetButtonDown("Fire3");
        bool crouchInput = Input.GetButton("Crouch");
        bool crouchInputUp = Input.GetButtonUp("Crouch");
        bool jumpInput = Input.GetButtonDown("Jump");

        float lookHorizontal = Input.GetAxisRaw("Mouse X");
        float lookVertical = Input.GetAxisRaw("Mouse Y");

       
        if (runInput)
        {
            if (!slideMode)
            {
                fastMode = !fastMode;
                crouchMode = false;
                anim.SetBool("Crouch Mode", crouchMode);
            }

            if (slideMode || crouchMode && velocity != Vector3.zero)
            {
                fastMode = true;
                crouchMode = false;
                anim.SetBool("Crouch Mode", crouchMode);
                slideMode = false;
                anim.SetBool("Slide Mode", slideMode);
            }
        }


        if (crouchInputUp)
        {
            upFromJumpSlide = false;
        }

        if (crouchInput)
        {
            if (slideAfterJump && slideMode && cc.isGrounded)
            {
                fastMode = true;
                slideMode = false;
                slideAfterJump = false;
                anim.SetBool("Slide Mode", slideMode);
                upFromJumpSlide = true;
            }
            else if (!upFromJumpSlide)
            {
                if (velocity.magnitude > 15f)
                {
                    if (!slideMode)
                    {
                        slideMultiplier = maxSlideMultiplier;
                        slideDirection = velocity.normalized;
                    }
                    slideMode = true;
                    fastMode = false;
                    anim.SetBool("Slide Mode", slideMode);
                }
                else
                {
                    crouchMode = true;
                    anim.SetBool("Crouch Mode", crouchMode);
                }
            }

        }
        else
        {
            if (slideMode)
            {
                if (slideMultiplier > 1.2f)
                {
                    if (cc.isGrounded && !slideAfterJump)
                    {
                        fastMode = true;
                    }
                }
                else
                {
                    if (cc.isGrounded && !slideAfterJump)
                    {
                        fastMode = false;
                    }
                }
                if (cc.isGrounded && !slideAfterJump)
                {
                    slideMode = false;
                    anim.SetBool("Slide Mode", slideMode);
                }
            }
            crouchMode = false;
            anim.SetBool("Crouch Mode", crouchMode);
        }

        if (slideMode && !cc.isGrounded)
            slideAfterJump = true;

        if (slideMultiplier < 1.5f)
            slideAfterJump = false;

        if (crouchMode && velocity != Vector3.zero && cc.isGrounded)
        {
            fastMode = false;
        }

        if (slideMode && cc.isGrounded)
        {
            slideMultiplier = Mathf.Max(slideMultiplier-slideFriction, 0.1f);
        }
        

        if (wallRunMode)
        {
            RaycastHit wallHit;
            bool wallCast = Physics.Raycast(transform.position, -wallRunNormal, out wallHit, cc.radius + cc.skinWidth + 0.1f);
            if (wallCast)
            {
                wallRunNormal = wallHit.normal;
                float wallRunDirectionX = Mathf.Abs(wallRunNormal.z) * Mathf.Sign(velocity.x);
                float wallRunDirectionZ = Mathf.Abs(wallRunNormal.x) * Mathf.Sign(velocity.z);
                wallRunDirection = new Vector3(wallRunDirectionX, 0, wallRunDirectionZ);
                if (jumpInput)
                    wallJumped = true;
            }
            else
            {
                wallRunMode = false;
            }
        }
            

        Vector3 xzVelocity;
        if (slideMode)
        {
            xzVelocity = slideDirection * speed;
        }
        else if (wallRunMode)
        {
            xzVelocity = wallRunDirection * wallRunSpeed;
        }
        else
        {
            xzVelocity = (transform.forward * forwardInput + transform.right * strafeInput).normalized * speed;
        }

        runMultiplier = fastMode ? Mathf.Min(runMultiplier + runAcceleration, maxRunMultiplier) : 1f;
        if (fastMode)
            xzVelocity *= runMultiplier;
        if (crouchMode)
            xzVelocity *= crouchMultiplier;
        if (slideMode)
            xzVelocity *= slideMultiplier;

        Vector3 nextPosition = transform.position + xzVelocity * Time.deltaTime;
        if (Physics.SphereCast(new Ray(nextPosition, Vector3.down), cc.radius, transform.position.y - groundCheck.position.y - cc.radius) && groundedLastFrame)
            velocity.y = (groundCheck.position.y - transform.position.y) / Time.deltaTime;
        else if (!cc.isGrounded)
            velocity.y += gravity;

        if (wallRunMode)
            velocity.y = Mathf.Max(velocity.y, 0);

        if (jumpInput && (!airJumped && cc.collisionFlags != CollisionFlags.Sides || jumpInput && cc.collisionFlags == CollisionFlags.Sides && !wallJumped))
        {
            if (!cc.isGrounded && cc.collisionFlags != CollisionFlags.Sides && !wallRunMode)
            {
                airJumped = true;
                velocity.y = jumpPower;
            }
            if ((cc.collisionFlags == CollisionFlags.Sides || wallRunMode) && !wallJumped)
            {
                wallJumped = true;
                wallJumpPower = maxWallJumpPower;
                velocity.y = jumpPower;
            }
            if (slideMode && slideAfterJump)
            {
                slideDirection = transform.forward * forwardInput + transform.right * strafeInput;
                xzVelocity = slideDirection * speed * slideMultiplier;
                velocity.y = jumpPower;
            }
            if (cc.isGrounded)
                velocity.y = jumpPower;
        }

        if (climbMode)
        {
            Debug.DrawRay(feetCheck.position, -wallClimbNormal, Color.red, 0f);
            Debug.DrawRay(bodyCheck.position, -wallClimbNormal, Color.blue, 0f);

            RaycastHit hit;
            bool bodyCast = Physics.BoxCast(transform.position, new Vector3(cc.radius, 1, cc.radius), -wallClimbNormal, out hit, Quaternion.identity, 1f);
            
            if (bodyCast && hit.collider != null && climbedObject != null)
            {
                if (hit.collider.gameObject == climbedObject)
                {
                    velocity.y = Mathf.Max(velocity.y, 10f);
                    xzVelocity = Vector3.zero;
                }
            }
            else
            {
                climbMode = false;
                velocity.y = 0;
            }
        }



        if (wallJumped)
        {
            xzVelocity += wallJumpNormal * wallJumpPower;
            wallJumpPower = Mathf.Max(wallJumpPower - 0.5f, 0);
        }

        transform.Rotate(Vector3.up, lookHorizontal * lookSpeed);
        cam.transform.Rotate(Vector3.right, -lookVertical * lookSpeed);

        if (Mathf.Abs(cam.transform.localEulerAngles.z) > 0.001)
            cam.transform.localEulerAngles = new Vector3(cam.transform.localEulerAngles.x < 90 ? 90 : 270, 0, 0);



        velocity = new Vector3(xzVelocity.x, velocity.y, xzVelocity.z);
        cc.Move(velocity * Time.deltaTime);

        if (cc.isGrounded)
        {
            velocity.y = 0;
            groundedLastFrame = true;
            airJumped = false;
            wallJumped = false;
        }
        else
        {
            groundedLastFrame = false;
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((cc.collisionFlags & CollisionFlags.Above) != 0)
        {
            velocity.y = -3;
        }
        
        if ((cc.collisionFlags & CollisionFlags.Sides) != 0 && slideMode)
        {
            slideMode = false;
            anim.SetBool("Slide Mode", slideMode);
            fastMode = slideMultiplier > 1.2f;
            slideAfterJump = false;
        }

        if (cc.isGrounded)
        {
            climbedObject = null;
            wallRunObject = null;
            wallJumped = false;
        }
        wallJumpNormal = hit.normal;
        RaycastHit wallHit;
        bool bodyCast = Physics.Raycast(bodyCheck.position, transform.forward, out wallHit, cc.radius+1f);
        RaycastHit bodyHit = wallHit;
        bool headCast = Physics.Raycast(headCheck.position, transform.forward, out wallHit, cc.radius+1f);

        wallClimbNormal = hit.normal;
        Debug.DrawRay(bodyCheck.position, -wallClimbNormal, Color.red, 0.1f);
        Debug.DrawRay(headCheck.position, -wallClimbNormal, Color.yellow, 0.1f);
        RaycastHit headHit = wallHit;
        if (bodyCast && !headCast && Mathf.Abs(Vector3.Angle(transform.forward, -hit.normal)) < 60f)
        {
            if (bodyHit.collider.gameObject != climbedObject)
            {
                climbedObject = bodyHit.collider.gameObject;
                if (hit.gameObject == bodyHit.collider.gameObject)
                {
                    wallClimbNormal = hit.normal;
                }
                climbMode = true;
            }
        }

        wallRunNormal = hit.normal;
        bool feetCastWall = Physics.Raycast(feetCheck.position, -wallRunNormal, cc.radius + 1f);
        bool headCastWall = Physics.Raycast(bodyCheck.position, -wallRunNormal, cc.radius + 1f);
        float approachAngle = Mathf.Abs(Vector3.Angle(velocity, -hit.normal));
        if (cc.collisionFlags == CollisionFlags.Sides && !climbMode && hit.collider.gameObject != wallRunObject
            && approachAngle > 30f && approachAngle < 60f)
        {
            if (feetCastWall && headCastWall)
            {
                wallRunSpeed = speed * wallRunMultiplier;
                wallRunMode = true;
                wallRunObject = hit.collider.gameObject;
                float wallRunDirectionX = Mathf.Abs(hit.normal.z) * Mathf.Sign(velocity.x);
                float wallRunDirectionZ = Mathf.Abs(hit.normal.x) * Mathf.Sign(velocity.z);

                wallRunDirection = new Vector3(wallRunDirectionX, 0, wallRunDirectionZ).normalized;
                StartCoroutine(StopWallRunning());
            }

        }
    }

    IEnumerator StopWallRunning()
    {
        yield return new WaitForSeconds(1f);

        wallRunMode = false;
    }
}