using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D feetCol;
    [SerializeField] private Collider2D bodyCol;

    private Rigidbody2D rb;

    //movement variables
    private Vector2 moveVelocity;
    private bool isFacingRight;

    //collision check variables
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    //jump variables
    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    //apex variables
    [SerializeField] private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    //jump buffer variables
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    //coyote time variables
    private float coyoteTimer;

    private void Awake()
    {
        isFacingRight = true;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        JumpChecks();
        CountTimers();
        CalculateGravity();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
    }

    private void CalculateGravity()
    {
        if (InputManager.AdrenalineIsHeld)
        {
            AdjustedJumpHeight = MoveStats.adrenalineJumpHeight * MoveStats.JumpHeightCompnesationFactor;
            Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(MoveStats.TimeTillJumpApex, 2f);
            InitialJumpVelocity = Mathf.Abs(Gravity) * MoveStats.TimeTillJumpApex;
        }
        else
        {
            AdjustedJumpHeight = MoveStats.jumpHeight * MoveStats.JumpHeightCompnesationFactor;
            Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(MoveStats.TimeTillJumpApex, 2f);
            InitialJumpVelocity = Mathf.Abs(Gravity) * MoveStats.TimeTillJumpApex;
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.AdrenalineIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y); 
        }
                    
    }

    private void TurnCheck (Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }

        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump

    

    private void JumpChecks()
    {
        //When we press the jump button
        if (InputManager.JumpWasPressed)
        {
            jumpBufferTimer = MoveStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        //When we release the jump button
        if (InputManager.JumpWasReleased)
        {
            if (jumpBufferTimer > 0)
            {
                jumpReleasedDuringBuffer = true;
            }

            if (isJumping && VerticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

      

        //Initiate jump with jump buffering and coyote time
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Double jump
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //Air jump after coyotoe time lapsed
        else if (jumpBufferTimer > 0f && isFalling && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        //Landed
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }


    }

    private void InitiateJump(int NumberOfJumpsUsed)
    {
        if (!isJumping)
        {
            isJumping = true;
        }
        jumpBufferTimer = 0f;
        numberOfJumpsUsed = NumberOfJumpsUsed;
        VerticalVelocity = InitialJumpVelocity;
    }

    private void Jump()
    {
        //apply gravity while jumping
        if (isJumping)
        {
            //Check for head bump
            if (bumpedHead)
            {
                isFastFalling = true;
            }
            
            //Gravity on Ascending
            if (VerticalVelocity >= 0f)
            {
                //apex controls
                apexPoint = Mathf.InverseLerp(InitialJumpVelocity, 0f, VerticalVelocity);

                if (apexPoint > MoveStats.ApexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        Debug.Log("Changes variable");
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                            Debug.Log("test");
                        }
                    }
                }


                //Gravity on ascending but not past apex threshold
                else
                {
                    Debug.Log("work");
                    VerticalVelocity += Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                        Debug.Log("false");
                    }
                }

            }

            else if (!isFastFalling)
            {
                VerticalVelocity += Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
                Debug.Log("fastfalling");
            }

            else if (VerticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }
        }

        //jump cut 
        if (isFastFalling)
        {
            Debug.Log("reaches");
            if (fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            fastFallTime += Time.fixedDeltaTime;
        }

        //normal gravity while falling
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            VerticalVelocity += Gravity * Time.fixedDeltaTime;
        }

        //clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        rb.velocity = new Vector2 (rb.velocity.x, VerticalVelocity);

    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCol.bounds.center.x, feetCol.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCol.bounds.size.x, MoveStats.GroundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (groundHit.collider != null)
        {
            isGrounded = true;
        }
        else { isGrounded = false; }

        #region Debug Visualization

        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);

        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCol.bounds.center.x, bodyCol.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCol.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (headHit.collider != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }


        #endregion
    }

    #endregion

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else { coyoteTimer = MoveStats.JumpCoyoteTime; }
    }

    #endregion
}
