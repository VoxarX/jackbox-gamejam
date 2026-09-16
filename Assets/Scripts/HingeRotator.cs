using UnityEngine;

public class HingeRotator : MonoBehaviour
{
    [Header("Rotation Targets (local euler angles)")]
    public Vector3 closedRotation;
    public Vector3 openRotation;

    [Header("Movement")]
    public float rotateSpeed = 90f; // degrees per second

    private Quaternion targetRotation;

    private void Awake()
    {
        targetRotation = Quaternion.Euler(closedRotation);
        transform.localRotation = targetRotation;
    }

    private void Update()
    {
        if (transform.localRotation == targetRotation)
            return;

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    // Wire this straight into PressurePlate's onActivated
    public void Open()
    {
        targetRotation = Quaternion.Euler(openRotation);
    }

    // Wire this into onDeactivated if you want it to close again
    public void Close()
    {
        targetRotation = Quaternion.Euler(closedRotation);
    }

    // Handy if you just want to force a snap with no lerp (e.g. testing)
    public void SetRotationInstant(Vector3 euler)
    {
        targetRotation = Quaternion.Euler(euler);
        transform.localRotation = targetRotation;
    }
}