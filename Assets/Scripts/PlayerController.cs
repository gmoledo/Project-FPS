using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float runMultiplier = 2f;
    [SerializeField]
    private float crouchMultiplier = 0.8f;
    [SerializeField]
    private float maxSlideMultiplier = 2.6f;
    [SerializeField]
    private float slideFriction = 0.01f;
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

    private enum PlayerStates {stand, walk, run, jump, fall, crouch, crouchWalk, slide, climb, wallRun}
    private PlayerStates playerState = PlayerStates.stand;

    private CharacterController cc;
    private GameObject cam;
    private GameObject camArm;
    private Animator anim;

    private Vector3 velocity;
    private Vector3 slideDirection;
    private float slideMultiplier;
    private bool fastMode;
    private bool slideMode;
    private bool crouchMode;
    private bool slideAfterJump;
    private bool upFromJumpSlide;
    private bool groundedLastFrame;
    private bool airJumped;
    private bool wallJumped;
    private Vector3 wallNormal;
    private float wallJumpPower;

    private void Awake()
    {
        Application.targetFrameRate = 144;
        cc = GetComponent<CharacterController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        camArm = GameObject.FindGameObjectWithTag("CameraArm");
        anim = camArm.GetComponent<Animator>();
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


        if (velocity == Vector3.zero)
        {
            if (crouchMode)
            {
                playerState = PlayerStates.crouch;
            }
            else
            {
                playerState = PlayerStates.stand;
            }
        }

        if (runInput)
        {
            if (!slideMode)
            {
                fastMode = !fastMode;
                crouchMode = false;
                anim.SetBool("Crouch Mode", crouchMode);
            }

            if (slideMode || playerState == PlayerStates.slide || playerState == PlayerStates.crouchWalk)
            {
                if (cc.isGrounded)
                    playerState = PlayerStates.run;
                fastMode = true;
                crouchMode = false;
                anim.SetBool("Crouch Mode", crouchMode);
                slideMode = false;
                anim.SetBool("Slide Mode", slideMode);
            }
        }

        if (velocity != Vector3.zero && cc.isGrounded)
        {
            if (!slideMode)
            {
                if (fastMode)
                    playerState = PlayerStates.run;
                else
                    playerState = PlayerStates.walk;
            }
            else
            {
                playerState = PlayerStates.slide;
            }
        }

        if (crouchInputUp)
        {
            upFromJumpSlide = false;
        }
        Debug.Log(upFromJumpSlide);
        if (crouchInput)
        {
            if (slideAfterJump && slideMode && cc.isGrounded)
            {
                playerState = PlayerStates.run;
                fastMode = true;
                slideMode = false;
                slideAfterJump = false;
                anim.SetBool("Slide Mode", slideMode);
                upFromJumpSlide = true;
            }
            else if (!upFromJumpSlide)
            {
                Debug.Log("D");
                if (velocity.magnitude > 15f)
                {
                    if (cc.isGrounded)
                        playerState = PlayerStates.slide;
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
                        playerState = PlayerStates.run;
                        fastMode = true;
                    }

                }
                else
                {
                    if (cc.isGrounded && !slideAfterJump)
                    {
                        playerState = PlayerStates.walk;
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

        #region Old Crouch Logic
        //if (crouchInput)
        //{
        //    if (velocity.magnitude > 15f)
        //    {
        //        if (cc.isGrounded)
        //            playerState = PlayerStates.slide;
        //        fastMode = false;
        //        if (!slideMode)
        //        {
        //            slideMultiplier = maxSlideMultiplier;
        //            slideDirection = velocity.normalized;
        //        }
        //        slideMode = true;
        //    }
        //    else
        //    {
        //        if (cc.isGrounded)
        //        {
        //            crouchMode = true;
        //        }
        //        if (!cc.isGrounded)
        //        {

        //        }
        //    }
        //    else if (cc.isGrounded)
        //    {
        //        crouchMode = true;
        //    }
        //    else if (!cc.isGrounded)
        //    {
        //        if (slideMode)
        //        {
        //            fastMode = false;
        //            crouchMode = false;
        //            slideMode = false;
        //        }
        //        else
        //        {
        //            crouchMode = !crouchMode;
        //            if (crouchMode)
        //                fastMode = false;
        //        }
        //    }
        //}
        #endregion

        if (crouchMode && velocity != Vector3.zero && cc.isGrounded)
        {
            playerState = PlayerStates.crouchWalk;
            fastMode = false;

        }

        if (slideMode && cc.isGrounded)
        {
            slideMultiplier = Mathf.Max(slideMultiplier-slideFriction, 0.1f);
        }

        Vector3 xzVelocity;
        if (slideMode)
            xzVelocity = slideDirection * speed;
        else
            xzVelocity = (transform.forward * forwardInput + transform.right * strafeInput).normalized * speed;
        
        if (fastMode)
            xzVelocity *= runMultiplier;
        if (crouchMode)
            xzVelocity *= crouchMultiplier;
        if (slideMode)
            xzVelocity *= slideMultiplier;

        if (crouchMode)
        {
            camArm.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        else if (slideMode)
        {
            camArm.transform.localPosition = new Vector3(0f, -0.7f, 0f);
        }
        else
        {
            camArm.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        }

        RaycastHit hit;
        Vector3 nextPosition = transform.position + xzVelocity * Time.deltaTime;
        if (Physics.SphereCast(nextPosition, cc.radius, Vector3.down, out hit, transform.position.y-groundCheck.position.y-cc.radius) && groundedLastFrame)
        {
            velocity.y = (groundCheck.position.y - transform.position.y) / Time.deltaTime;
        }
        else if (!cc.isGrounded)
        {
            velocity.y += gravity;
        }

        if(jumpInput && !airJumped && cc.collisionFlags != CollisionFlags.Sides || jumpInput && cc.collisionFlags == CollisionFlags.Sides && !wallJumped)
        {
            if (!cc.isGrounded && cc.collisionFlags != CollisionFlags.Sides)
            {
                airJumped = true;
                wallJumped = false;
            }
            if (cc.collisionFlags == CollisionFlags.Sides)
            {
                wallJumped = true;
                wallJumpPower = maxWallJumpPower;
            }
            if (cc.isGrounded && slideMode && slideAfterJump)
            {
                //slideMultiplier = maxSlideMultiplier;
                slideDirection = transform.forward;
                xzVelocity = slideDirection * speed * slideMultiplier;
            }
            velocity.y = jumpPower;
        }

        if (wallJumped)
        {
            Debug.Log(xzVelocity);
            xzVelocity += wallNormal * wallJumpPower;
            Debug.Log(xzVelocity);
            wallJumpPower = Mathf.Max(wallJumpPower - 0.5f, 0);
        }

        if (velocity.y > 0 && !cc.isGrounded)
        {
            playerState = PlayerStates.jump;
        }
        if (velocity.y < 0 && !cc.isGrounded)
        {
            playerState = PlayerStates.fall;
        }

        transform.Rotate(Vector3.up, lookHorizontal * lookSpeed);
        cam.transform.Rotate(Vector3.right, -lookVertical * lookSpeed);

        if (Mathf.Abs(cam.transform.localEulerAngles.z) > 0.001)
            cam.transform.localEulerAngles = new Vector3(cam.transform.localEulerAngles.x < 90 ? 90 : 270, 0, 0);



        velocity = new Vector3(xzVelocity.x, velocity.y, xzVelocity.z);
        cc.Move(velocity * Time.deltaTime);

        if (slideMode && cc.isGrounded && !groundedLastFrame)
        {
            playerState = PlayerStates.slide;
        }

        if (cc.isGrounded)
        {
            velocity.y = 0;
            groundedLastFrame = true;
            airJumped = false;
        }
        else
        {
            groundedLastFrame = false;
        }
        Debug.Log(playerState);
    }

    private void ToggleCrouchingCharacterController(bool crouchMode)
    {

    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        airJumped = false;
        if (cc.collisionFlags == CollisionFlags.Sides && !wallJumped)
            wallNormal = hit.normal;
        if ((cc.collisionFlags & CollisionFlags.Sides) != 0)
        {
            slideMode = false;
            anim.SetBool("Slide Mode", slideMode);
            fastMode = slideMultiplier > 1.2f;
            slideAfterJump = false;
        }
    }
}
