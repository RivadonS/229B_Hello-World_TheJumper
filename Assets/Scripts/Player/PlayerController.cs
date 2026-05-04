using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float iceMoveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground & Ice Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask iceLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isOnIce;
    private bool isSprinting;
    private Vector2 moveInput;

    [Header("Game State")]
    public bool hasKey = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("🚨 จับได้แล้ว! สคริปต์ PlayerController ไปโผล่อยู่ที่ Object ชื่อ: '" + gameObject.name + "' แต่มันไม่มี Rigidbody2D ครับ!");
        }
    }

    void Update()
    {
        if (groundCheck == null) return;

        bool touchGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool touchIce = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, iceLayer);

        isGrounded = touchGround || touchIce;
        isOnIce = touchIce && !touchGround;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            float currentMoveSpeed = moveSpeed;
            float currentIceSpeed = iceMoveSpeed;

            if (isSprinting)
            {
                currentMoveSpeed *= sprintMultiplier;
                currentIceSpeed *= sprintMultiplier;
            }

            if (isOnIce)
            {
                if (Mathf.Abs(moveInput.x) > 0.1f)
                {
                    rb.linearVelocity = new Vector2(moveInput.x * currentIceSpeed, rb.linearVelocity.y);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, rb.linearVelocity.y);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log("กำลังกดปุ่มทิศทาง: " + moveInput);
    }

    public void OnJump(InputValue value)
    {
        Debug.Log("กดปุ่มกระโดด! เท้าติดพื้นไหม?: " + isGrounded);

        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
        Debug.Log("กำลังกดวิ่ง: " + isSprinting);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            hasKey = true;
            Destroy(collision.gameObject);
            Debug.Log("Got the Key!");
        }

        if (collision.CompareTag("Trap") || collision.CompareTag("Lava"))
        {
            Die();
        }

        if (collision.CompareTag("Win"))
        {
            if (hasKey)
            {
                Debug.Log("Level Cleared!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.Log("You need the key first!");
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}