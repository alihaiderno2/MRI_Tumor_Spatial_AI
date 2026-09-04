using UnityEngine;
using UnityEngine.InputSystem;

public class SurgeonViewportController : MonoBehaviour
{
    [Header("Rotation Sensitivity")]
    [Tooltip("Controls mouse dragging rotation speed")]
    public float rotationSpeed = 0.08f;
    [Tooltip("Smoothness factor (lower = smoother/weightier, higher = sharper)")]
    public float smoothness = 12f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.005f;
    public float minScale = 0.2f;
    public float maxScale = 3.0f;

    private Vector2 previousMousePosition;
    private bool isDragging = false;

    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // 1. Lock 3D Rotation exclusively to RIGHT-CLICK DRAGGING
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            previousMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 delta = currentPos - previousMousePosition;

            // Calculate target rotation changes based on mouse movement
            float yaw = -delta.x * rotationSpeed;
            float pitch = delta.y * rotationSpeed;

            Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, transform.right);

            targetRotation = yawRotation * pitchRotation * targetRotation;
            previousMousePosition = currentPos;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothness);

        float scrollDelta = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            Vector3 targetScale = transform.localScale + Vector3.one * (scrollDelta * zoomSpeed);
            targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
            targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
            targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothness);
        }
    }
}