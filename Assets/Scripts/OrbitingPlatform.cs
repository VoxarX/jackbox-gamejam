using UnityEngine;

public class OrbitingPlatform : MonoBehaviour
{
    public Transform pivot;
    public float degreesPerSecond = 30f;
    public Vector3 rotationAxis = Vector3.forward;

    public bool shouldRotate = false;

    private void Update()
    {
        if (shouldRotate)
            transform.RotateAround(pivot.position, pivot.TransformDirection(rotationAxis), degreesPerSecond * Time.deltaTime);
    }

    public void SetRotate(bool shouldRotateIn)
    {
        shouldRotate = shouldRotateIn;
    }
}