using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerOld : MonoBehaviour {

    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float lookSpeed = 0.5f;
    [SerializeField]
    private float gravity = -1f;
    [SerializeField]
    private float jumpPower = 1f;
    [SerializeField]
    private float wallReductionFactor = 0.5f;

    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private GameObject wallCheck;
    [SerializeField]
    private GameObject bulletHit;

    private Vector3 velocity;
    private Vector3 collisionMovementVector;
    private Vector3 wallVelocity;
    private bool collided;
    private bool groundedLastFrame;
    private bool touchingWall;
    private bool wallJumped;
    private bool airJumped;
    private bool stopFalling;
    private bool wallRunning;

    private bool waitingToFall;
    private bool fallingOffWall;
    private float fallStarted = 0f;

    private CharacterController cc;
    private GameObject cam;

	// Use this for initialization
	void Start ()
    {
        Application.targetFrameRate = 144;
        
        cc = GetComponent<CharacterController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
	}

    // Update is called once per frame
    void Update()
    {
        // Get Input
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        float forwardInput = Input.GetAxisRaw("Vertical");
        float strafeInput = Input.GetAxisRaw("Horizontal");

        bool jumped = Input.GetButtonDown("Jump");

        float lookHorizontal = Input.GetAxisRaw("Mouse X");
        float lookVertical = Input.GetAxisRaw("Mouse Y");

        bool fired = Input.GetButtonDown("Fire1");

        // Shoot
        if (fired)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f))
            {
                GameObject impact = Instantiate(bulletHit, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact,10f);

            }
        }


        // Rotate player and camera
        transform.Rotate(Vector3.up, lookHorizontal * lookSpeed);
        cam.transform.Rotate(Vector3.right, -lookVertical * lookSpeed);

        if (Mathf.Abs(cam.transform.localEulerAngles.z) > 0.001)
            cam.transform.localEulerAngles = new Vector3(cam.transform.localEulerAngles.x < 90 ? 90 : 270, 0, 0);


        // Calculate velocity in the xz-plane
        Vector3 xzVelocity = (transform.forward * forwardInput + transform.right * strafeInput) * speed;
        if (Input.GetKey(KeyCode.LeftShift))
            xzVelocity *= 2f;

        // Calculate velocity in y-axis
        if (Physics.Linecast(transform.position, groundCheck.position) && groundedLastFrame)
        {
            velocity.y = (groundCheck.position.y - transform.position.y) / Time.deltaTime;
        }
        else if (!cc.isGrounded)
        {
            if (!cc.isGrounded)
                velocity.y += gravity;
        }

        // Jump logic
        if (jumped && !airJumped)
        {
            velocity.y = jumpPower;
        }

        if (jumped && !cc.isGrounded)
        {
            airJumped = true;
        }

        // Wall movement logic
        //if (cc.collisionFlags == CollisionFlags.Sides)
        //{
        //    if (!waitingToFall)
        //    {
        //        StartCoroutine(WaitToFall());
        //    }

        //    if (velocity.y < 0 && !fallingOffWall)
        //    {
        //        velocity.y *= wallReductionFactor;
        //    }

        //    if (jumped && !wallJumped)
        //    {
        //        wallJumped = true;
        //        velocity.y = jumpPower;
        //        stopFalling = true;
        //        waitingToFall = false;
        //        fallingOffWall = false;
        //    }
        //}

        if (wallRunning)
        {
            velocity.y = 0;
            xzVelocity = wallVelocity;
        }

        // Move player
        velocity = new Vector3(xzVelocity.x, velocity.y, xzVelocity.z);

        collided = false;
        cc.Move(velocity * Time.deltaTime);

        // Extra logic
        if (cc.isGrounded)
        {
            velocity.y = 0;
            groundedLastFrame = true;
            waitingToFall = false;
            fallingOffWall = false;
            wallJumped = false;
            airJumped = false;
        }
        else
        {
            groundedLastFrame = false;
        }
        
    }

    private IEnumerator WaitToFall()
    {
        waitingToFall = true;
        fallStarted = Time.time;

        yield return new WaitForSeconds(1f);
        
        if (!stopFalling)
        {
            fallingOffWall = true;
            wallRunning = false;
        }
        else
        {
            stopFalling = !stopFalling;
        }
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (cc.collisionFlags == CollisionFlags.Sides && !fallingOffWall && !wallRunning && Mathf.Abs(velocity.y) < .1f)
        {
            Debug.Log("a");
            wallRunning = true;
            StartCoroutine(WaitToFall());
            Vector3 xzMoveDirection = new Vector3(velocity.x, 0, velocity.z);
            Vector3 xzNormal = new Vector3(hit.normal.x, 0, hit.normal.z);
            wallVelocity = (xzMoveDirection - xzNormal).normalized * speed;
            //Debug.Log(velocity);
            //Debug.Log(xzNormal);
            //Debug.Log(wallVelocity);
        }
    }

    public void ToggleWallCollision()
    {
        touchingWall = !touchingWall;
    }
}
