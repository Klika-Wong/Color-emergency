using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D平台游戏角色移动控制（纯净版）
/// 支持：内部键盘控制 + 外部UDP指令调用
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ==================== 核心组件 ====================
    [Header("核心组件")]
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    // ==================== 移动设置 ====================
    [Header("移动设置")]
    [Tooltip("可跳跃的地面Layer")]
    [SerializeField] private LayerMask jumpableGround;

    [Tooltip("水平移动速度")]
    [SerializeField] private float moveSpeed = 7f;

    [Tooltip("跳跃力度")]
    [SerializeField] private float jumpForce = 12f;

    // ==================== 跳跃手感优化 ====================
    [Header("手感优化")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.1f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float dirX = 0f;

    private enum MovementState { idle, running, jumping, falling }

    [Header("音效")]
    [SerializeField] private AudioSource jumpSoundEffect;

    // ==================== 初始化 ====================
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // ==================== 每帧执行 ====================
    private void Update()
    {
        // 1. 处理内部键盘输入
        HandleInternalKeyboardInput();

        // 2. 物理移动逻辑
        HandleMovement();

        // 3. 跳跃逻辑
        HandleJump();

        // 4. 动画状态
        UpdateAnimationState();
    }

    // ==================== 键盘输入逻辑 ====================
    private void HandleInternalKeyboardInput()
    {
        // A / D 键控制左右移动
        if (Input.GetKey(KeyCode.A))
        {
            SetHorizontalDirection(-1f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            SetHorizontalDirection(1f);
        }
        else if (Input.GetAxisRaw("Horizontal") == 0)
        {
            // 如果键盘没有输入，不强制置为0，允许外部UDP手势传入的指令保持生效
            // 依赖外部调用 SetHorizontalDirection(0) 来停止
        }

        // 空格键控制跳跃
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerJump();
        }
    }

    // ==================== 运动与物理 ====================
    private void HandleMovement()
    {
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
    }

    private void HandleJump()
    {
        // 郊狼时间计算
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 跳跃缓冲计算
        jumpBufferCounter -= Time.deltaTime;

        // 执行跳跃
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            if (jumpSoundEffect != null)
            {
                jumpSoundEffect.Play();
            }

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
        }
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    // =========================================================================
    // ==================== 以下为外部 UDP 手势调用的接口 ========================
    // =========================================================================

    public void SetHorizontalDirection(float direction)
    {
        dirX = Mathf.Clamp(direction, -1f, 1f);
    }

    public void TriggerJump()
    {
        jumpBufferCounter = jumpBuffer;
    }
}