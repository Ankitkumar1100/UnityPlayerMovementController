using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 12f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.4f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 14f;
    public float wallSlideSpeed = 2f;
    public LayerMask wallLayer;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public float wallCheckRadius = 0.25f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float moveInput;
    bool isGrounded;
    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    int facingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Facing Direction Memory
        if (moveInput != 0)
            facingDirection = (int)Mathf.Sign(moveInput);

        // Flip Sprite
        if (moveInput > 0)
            sr.flipX = false;
        else if (moveInput < 0)
            sr.flipX = true;

        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Wall Checks
        bool onWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        bool onWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        bool onWall = onWallRight || onWallLeft;

        HandleJump(onWall, onWallRight);
        HandleDash();
        BetterGravity();
        HandleWallSlide(onWall);

        // Animation Control
        anim.SetBool("isRunning", Mathf.Abs(moveInput) > 0.1f);
        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isDashing", isDashing);
        anim.SetFloat("runSpeed", Mathf.Abs(rb.velocity.x));
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            float targetSpeed = moveInput * moveSpeed;
            rb.velocity = new Vector2(
                Mathf.Lerp(rb.velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime),
                rb.velocity.y
            );
        }
    }

    void HandleJump(bool onWall, bool onWallRight)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            else if (onWall)
            {
                float pushDir = onWallRight ? -1 : 1;
                rb.velocity = new Vector2(pushDir * wallJumpForceX, wallJumpForceY);
            }
        }
    }

    void HandleWallSlide(bool onWall)
    {
        if (onWall && !isGrounded && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    void BetterGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void HandleDash()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            rb.velocity = new Vector2(facingDirection * dashSpeed, 0);

            if (dashTimer <= 0)
                isDashing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        if (wallCheckRight != null)
            Gizmos.DrawWireSphere(wallCheckRight.position, wallCheckRadius);

        if (wallCheckLeft != null)
            Gizmos.DrawWireSphere(wallCheckLeft.position, wallCheckRadius);
    }
}
