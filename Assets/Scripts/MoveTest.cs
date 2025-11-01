using UnityEngine;

public class FlyCameraGod : MonoBehaviour
{
    [Header("Speed (m/s)")]
    public float moveSpeed = 12f;         // tốc độ cơ bản
    public float boostMultiplier = 4f;    // giữ Shift
    public float slowMultiplier = 0.35f;  // giữ Ctrl

    [Header("Mouse Look")]
    public float lookSensitivity = 300f;  // độ nhạy (độ/giây cho mỗi 1 mouse delta)
    public bool holdRightMouseToLook = false; // true = giữ RMB mới xoay
    public bool invertY = false;
    public float pitchClamp = 89f;

    float yaw, pitch;

    void Start()
    {
        var e = transform.rotation.eulerAngles;
        yaw = e.y; pitch = e.x;

        if (!holdRightMouseToLook) LockCursor(true);
    }

    void Update()
    {
        // ---- Look (không smooth) ----
        bool canLook = !holdRightMouseToLook || Input.GetMouseButton(1);
        if (canLook)
        {
            float mx = Input.GetAxisRaw("Mouse X");
            float my = Input.GetAxisRaw("Mouse Y");
            float signY = invertY ? 1f : -1f;

            yaw += mx * lookSensitivity * Time.deltaTime;
            pitch += signY * my * lookSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // Toggle khoá chuột
        if (Input.GetKeyDown(KeyCode.Tab))
            LockCursor(!(Cursor.lockState == CursorLockMode.Locked));

        // Reset hướng nhìn
        if (Input.GetKeyDown(KeyCode.R))
        {
            yaw = 0f; pitch = 0f;
            transform.rotation = Quaternion.identity;
        }

        // ---- Move (không smooth) ----
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        float upDown = 0f;
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) upDown += 1f;
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Q)) upDown -= 1f;

        Vector3 dir = (transform.right * h + transform.forward * v + Vector3.up * upDown);
        if (dir.sqrMagnitude > 1f) dir.Normalize(); // tránh vượt tốc khi bấm nhiều phím

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;
        if (Input.GetKey(KeyCode.LeftControl)) speed *= slowMultiplier;

        transform.position += dir * speed * Time.deltaTime;

        // Con lăn chỉnh tốc nhanh
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
            moveSpeed = Mathf.Max(0.5f, moveSpeed * (scroll > 0 ? 1.15f : 0.87f));
    }

    void LockCursor(bool yes)
    {
        Cursor.lockState = yes ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !yes;
    }
}
