using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    public Transform cameraRoot; // Drag CameraTarget vào đây
    private CharacterController cc;

    [Header("Move")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.0f;
    public float rotationSpeed = 12f; // quay mượt về hướng di chuyển
    public float acceleration = 12f;  // mượt tốc độ
    Vector2 moveInput;
    bool sprintHeld;

    [Header("Jump/Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -9.81f;
    public float groundedGravity = -2f; // giữ dính đất
    public float coyoteTime = 0.1f;
    public LayerMask groundMask = ~0;
    public float slopeLimit = 60f;

    float verticalVel;
    float coyoteTimer;
    bool jumpPressed;

    // cache
    Transform cam;
    float currentSpeed;

    // Input System callbacks (PlayerInput -> Behavior: Invoke Unity Events)
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) { /* xử lý bởi CinemachineInputProvider */ }
    public void OnJump(InputValue value)
    {
        if (value.isPressed) jumpPressed = true;
    }
    public void OnSprint(InputValue value)
    {
        sprintHeld = value.isPressed;
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cam = Camera.main != null ? Camera.main.transform : null;
        if (cc) cc.slopeLimit = slopeLimit;
        // Khóa chuột ngay từ đầu:
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (cam == null) { cam = Camera.main ? Camera.main.transform : null; }

        // --- Ground check ---
        bool grounded = cc.isGrounded;
        if (grounded)
        {
            if (verticalVel < 0f) verticalVel = groundedGravity;
            coyoteTimer = coyoteTime; // reset cửa sổ nhảy
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // --- Move plan (theo hướng camera) ---
        Vector3 camF = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 desiredDir = (camF * moveInput.y + camR * moveInput.x);
        desiredDir = desiredDir.sqrMagnitude > 1e-4f ? desiredDir.normalized : Vector3.zero;

        float targetSpeed = (sprintHeld ? sprintSpeed : walkSpeed) * desiredDir.magnitude;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 1f - Mathf.Exp(-acceleration * Time.deltaTime));

        Vector3 horizontal = desiredDir * currentSpeed;

        // --- Rotate character hướng di chuyển (souls-like) ---
        if (desiredDir.sqrMagnitude > 1e-4f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));
        }

        // --- Jump & Gravity ---
        if (jumpPressed && coyoteTimer > 0f)
        {
            jumpPressed = false;
            coyoteTimer = 0f;
            verticalVel = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
        else
        {
            jumpPressed = false;
        }

        verticalVel += gravity * Time.deltaTime;

        // --- Apply ---
        Vector3 velocity = horizontal + Vector3.up * verticalVel;
        cc.Move(velocity * Time.deltaTime);

        // Ngăn drift dọc khi dừng
        if (grounded && desiredDir == Vector3.zero && Mathf.Abs(currentSpeed) < 0.05f)
            currentSpeed = 0f;
    }
}
