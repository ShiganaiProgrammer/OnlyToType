using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoScrollPlayer : MonoBehaviour
{
    const string JumpCommand = "jump";
    const string JumpCommand2 = "hop";
    const string DashCommand = "dash";
    const string DashCommand2 = "run";

    [SerializeField] float baseSpeed = 4f;
    [SerializeField] float dashSpeed = 8f;
    [SerializeField] float dashDuration = 5f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] float coyoteTime = 0.15f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    TypingCommandInput commandInput;
    TypingInputReader inputReader;

    [SerializeField] LayerMask groundLayerMask = 1 << 6;

    float dashTimer;
    float groundedTimer;
    bool isGrounded;
    float prevX;

    public float Distance { get; private set; }
    public float CurrentSpeed => dashTimer > 0f ? dashSpeed : baseSpeed;
    public bool IsDashing => dashTimer > 0f;
    public float DashTimeRemaining => dashTimer;
    public TypingCommandInput CommandInput => commandInput;
    public bool IsGrounded => isGrounded;
    public event Action OnHitObstacle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        commandInput = new TypingCommandInput();
        commandInput.RegisterCommand(JumpCommand, TryJump);
        commandInput.RegisterCommand(JumpCommand2, TryJump);
        commandInput.RegisterCommand(DashCommand, TryDash);
        commandInput.RegisterCommand(DashCommand2, TryDash);

        inputReader = new TypingInputReader();
        inputReader.OnCharTyped += commandInput.ProcessChar;
        inputReader.Enable();
    }

    void OnDestroy()
    {
        if (inputReader == null) return;
        inputReader.OnCharTyped -= commandInput.ProcessChar;
        inputReader.Dispose();
    }

    void Update()
    {
        if (dashTimer > 0f)
            dashTimer -= Time.deltaTime;

        CheckGrounded();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        var velocity = rb.linearVelocity;
        velocity.x = CurrentSpeed;
        rb.linearVelocity = velocity;
        float dx = rb.position.x - prevX;
        if (dx > 0f)
            Distance += dx;
        prevX = rb.position.x;
    }

    void CheckGrounded()
    {
        var feet = (Vector2)transform.position + groundCheckOffset;
        var hits = Physics2D.OverlapCircleAll(feet, groundCheckRadius, groundLayerMask);

        bool onGround = false;
        foreach (var hit in hits)
        {
            if (IsValidGround(hit))
            {
                onGround = true;
                break;
            }
        }

        if (!onGround)
        {
            var boxHit = Physics2D.BoxCast(
                feet,
                new Vector2(0.4f, 0.05f),
                0f,
                Vector2.down,
                0.2f,
                groundLayerMask);

            onGround = boxHit.collider != null && IsValidGround(boxHit.collider);
        }

        if (onGround)
            groundedTimer = coyoteTime;
        else if (groundedTimer > 0f)
            groundedTimer -= Time.deltaTime;

        isGrounded = groundedTimer > 0f;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsValidGround(collision.collider)) return;
        if (rb.linearVelocity.y > 0.1f) return;

        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundedTimer = coyoteTime;
                isGrounded = true;
                return;
            }
        }
    }

    bool IsValidGround(Collider2D col)
    {
        if (col == null || col.gameObject == gameObject) return false;
        return ((1 << col.gameObject.layer) & groundLayerMask) != 0;
    }

    void TryJump()
    {
        if (!isGrounded) return;

        groundedTimer = 0f;
        var velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    void TryDash()
    {
        dashTimer = dashDuration;
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        if (IsDashing)
            spriteRenderer.color = new Color(1f, 0.85f, 0.4f);
        else if (!isGrounded)
            spriteRenderer.color = new Color(0.7f, 0.85f, 1f);
        else
            spriteRenderer.color = Color.white;
    }

    public bool IsFallen(float deathY)
    {
        return transform.position.y < deathY;
    }

    public void ResetRun()
    {
        Distance = 0f;
        dashTimer = 0f;
        commandInput.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }
}
