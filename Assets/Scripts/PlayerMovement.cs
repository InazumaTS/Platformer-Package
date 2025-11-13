using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Collider2D playerCollider;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    PlayerInputs playerInput;

    [Header("Basic Movement Stats")]
    [SerializeField]
    float topSpeed = 20f;
    [SerializeField]
    float acceleration = 5f;
    [SerializeField]
    float deceleration = 5f;
    [SerializeField]
    float turnDeceleration = 10f;
    [SerializeField]
    float groundRayCastDistance = 0.1f;

    [Header("Basic Jump Stats")]
    [SerializeField]
    float jumpForce = 10f;
    [SerializeField]
    float JumpCutMultiplier = 1f;
    [SerializeField]
    float fallGravityForce = 2f;
    [SerializeField]
    float fallGravityMultiplier = 2.5f;
    [SerializeField]
    float maxFallSpeed = -20f;
    [SerializeField]
    float jumpBufferDuration = 0.1f;
    [SerializeField]
    bool hasJumpHangTime = false;
    [SerializeField]
    float jumpHangThreshold = 0.2f;
    [SerializeField]
    float jumpHangGravityMultiplier = 0.5f;


    [Header("")]
    [SerializeField]
    float coyoteTimeDuration = 0.2f;
    [SerializeField]
    float ceilingRayCastDistance = 0.1f;

    [Header("Air Movement Stats")]
    [SerializeField]
    float airTopSpeed = 7f;
    [SerializeField]
    float airAcceleration = 5f;
    [SerializeField]
    float airDeceleration = 2f;
    [SerializeField]
    float airTurnDeceleration = 4f;
    [SerializeField]
    bool hasBonusAirSpeed = false;
    [SerializeField]
    float bonusAirSpeed = 12f;
    [SerializeField]
    float bonusAirAcceleration = 8f;
    [SerializeField]
    float bonusAirDeceleration = 5f;
    [SerializeField]
    float bonusAirTurnDeceleration = 7f;

    [Header("Slope Movement")]
    [SerializeField]
    float maxSlopeAngle = 45f;
    [SerializeField]
    float UphillCoeffecient = 0.6f;
    [SerializeField]
    float DownhillCoeffecient = 1.3f;

    //add Kirby based multi-jump, Crash bandicoot based double jump, differing Platformer style wall jumps
    //add moving platform support
    //add slope support

    Vector2 slopeNormal;
    Vector2 slopeDirection;
    Vector2 oldMovementVector;
    float slopeAngle;
    float currentSpeed = 0f;
    float airSpeed = 0f;
    float currentJumpForce;
    float lastGroundedTime;
    float lastJumpPressedTime;
    bool isUphill = false;
    bool isButtonStillHeld =true;
    enum state { idle, running, jumping, falling }
    state currentState;
    void Start()
    {

        playerInput = GetComponent<PlayerInputs>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        playerInput.OnJump += PlayerInput_OnJump;
        playerInput.OnInteract += PlayerInput_OnInteract;

        currentJumpForce = jumpForce;
    }

    private void PlayerInput_OnInteract(object sender, System.EventArgs e)
    {

    }

    private void PlayerInput_OnJump(object sender, System.EventArgs e)
    {
        lastJumpPressedTime = Time.time;
        Debug.DrawRay(transform.position, Vector2.down * (playerCollider.bounds.extents.y + groundRayCastDistance), Color.red, 1f);
        if (Time.time <= lastGroundedTime + coyoteTimeDuration)
        {
            currentState = state.jumping;
        }
    }

    private void Update()
    {

        RaycastHit2D hit = isGrounded();
        slopeNormal = hit.normal;
        slopeAngle = Vector2.Angle(slopeNormal,Vector2.up);
        isUphill = Mathf.Sign(oldMovementVector.x) != Mathf.Sign(hit.normal.x);
        if(slopeAngle>maxSlopeAngle)
            currentState = state.falling;
        if (hit.collider != null)
        {
            lastGroundedTime = Time.time;
        }
        if (hit.collider == null && currentState != state.jumping)
        {
            currentState = state.falling;
        }
        if (lastJumpPressedTime + jumpBufferDuration >= Time.time && hit.collider!=null)
        {

            currentState = state.jumping;
        }


    }
    // Update is called once per frame'
    void FixedUpdate()
    {
        switch(currentState)
        {
            case state.idle:
                IdleState();
                break;
            case state.running:
                RunState();
                break;
            case state.jumping:
                JumpState();
                break;
            case state.falling:
                FallState();
                break;
        }
    }

    private void IdleState()
    {
        if(playerInput.GetMovementVector() != Vector2.zero)
        {
            currentState = state.running;
        }
        else
        {
            currentSpeed = 0f;
        }
        rb.linearVelocity = new Vector2(playerInput.GetMovementVector().x* currentSpeed, rb.linearVelocity.y);
    }

    private void RunState()
    {
        Vector2 adjustedSpeed;
        slopeDirection = Vector2.Perpendicular(slopeNormal).normalized;
        if (Mathf.Sign(oldMovementVector.x) != Mathf.Sign(slopeDirection.x))
            slopeDirection = -slopeDirection;
        currentSpeed = HandleMovement(currentSpeed, acceleration, deceleration, turnDeceleration, topSpeed);
        if (slopeAngle == 0)
            adjustedSpeed = currentSpeed * slopeDirection;
        else if (isUphill)
            adjustedSpeed = currentSpeed * slopeDirection * (UphillCoeffecient);
        else
            adjustedSpeed = currentSpeed * slopeDirection * (DownhillCoeffecient);

            Debug.Log(currentSpeed + "," + adjustedSpeed);

        rb.linearVelocity = adjustedSpeed;
    }

    private void JumpState()
    {
        bool isJumpButtonHeld = playerInput.isJumpButtonHeld();

        airSpeed = currentSpeed;
        airSpeed = HandleMovement(airSpeed, airAcceleration, airDeceleration, airTurnDeceleration, airTopSpeed);
        currentSpeed = airSpeed;

        rb.linearVelocity = new Vector2(oldMovementVector.x * airSpeed, currentJumpForce);

        if (isJumpButtonHeld && hasJumpHangTime && currentJumpForce <= jumpHangThreshold && isButtonStillHeld)
        {
            currentJumpForce -= (fallGravityForce * jumpHangGravityMultiplier) * Time.deltaTime;
        }
        else if (isJumpButtonHeld)
            currentJumpForce -= fallGravityForce * Time.deltaTime;
        else
        {
            currentJumpForce -= fallGravityForce * JumpCutMultiplier * Time.deltaTime;
            isButtonStillHeld = false;
        }
        if (rb.linearVelocityY <= 0)
        {
            currentState = state.falling;
        }

        int playerLayer = gameObject.layer;
        int layerMask = ~(1 << playerLayer);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, playerCollider.bounds.extents.y + ceilingRayCastDistance, layerMask);
        if (hit.collider != null)
        {
            currentState = state.falling;
        }
    }

    private void FallState()
    {
        isButtonStillHeld = true;
        currentJumpForce = jumpForce;
        float fallSpeed = Mathf.MoveTowards(rb.linearVelocity.y, maxFallSpeed, fallGravityForce * fallGravityMultiplier * Time.fixedDeltaTime);
        airSpeed = currentSpeed;
        if(!hasBonusAirSpeed)
        airSpeed = HandleMovement(airSpeed, airAcceleration, airDeceleration, airTurnDeceleration, airTopSpeed);
        else
        airSpeed = HandleMovement(airSpeed, bonusAirAcceleration, bonusAirDeceleration, bonusAirTurnDeceleration, bonusAirSpeed);


        rb.linearVelocity = new Vector2(oldMovementVector.x * airSpeed, fallSpeed);
        currentSpeed = airSpeed;


        RaycastHit2D hit = isGrounded();
        if (hit.collider != null)
        {
            if(playerInput.GetMovementVector() != Vector2.zero)
                currentState = state.running;
            else
                currentState = state.idle;
            
        }   

    }

    private float HandleMovement(float currentSpeed,float acceleration, float deceleration,float turnceleration, float topspeed)
    {
        Vector2 movementInput = playerInput.GetMovementVector();
        float Xinput = movementInput.x;
        float Xvel = rb.linearVelocity.x;
        if (Xinput != 0f && Mathf.Sign(Xinput) == Mathf.Sign(Xvel))
        {
            oldMovementVector = movementInput;
            currentSpeed = Mathf.MoveTowards( currentSpeed, topspeed, acceleration * Time.fixedDeltaTime);
        }
        else if (Xinput != 0f && Mathf.Sign(Xinput) != Mathf.Sign(Xvel) && Xvel == 0)
        {
            oldMovementVector = movementInput;
            currentSpeed = Mathf.MoveTowards(currentSpeed, topspeed, acceleration * Time.fixedDeltaTime);
        }
        else if (Xinput != 0f && Mathf.Sign(Xinput) != Mathf.Sign(Xvel))
        {
            // Apply stronger turn friction
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, turnceleration * Time.fixedDeltaTime);

            // Once slowed enough, flip direction
            if (Mathf.Abs(currentSpeed) <= 0.01f)
                currentSpeed = 0f;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }
        return currentSpeed;
    }


    private RaycastHit2D isGrounded()
    {
        int playerLayer = gameObject.layer;
        int layerMask = ~(1 << playerLayer);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, playerCollider.bounds.extents.y + groundRayCastDistance, layerMask);
        return hit;
    }

    private Vector2 GetSlopeAdjustedVelocity()
    {
        return Vector2.zero;
    }
}
