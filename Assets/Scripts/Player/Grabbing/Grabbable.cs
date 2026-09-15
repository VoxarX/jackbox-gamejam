using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : NetworkBehaviour
{
    [Header("Hold Physics")]
    public float holdStiffness = 400f;   // pull strength
    public float holdDamping = 25f;      // resistance to oscillation
    public float maxHoldForce = 800f;    // hard ceiling so it can't punch through walls
    public float maxHoldSpeed = 8f;      // clamp velocity while held

    private Rigidbody rb;
    private Transform holdPoint;
    private bool isHeld;

    public bool IsHeld => isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);
    }

    public void BeginHold(Transform newHoldPoint)
    {
        holdPoint = newHoldPoint;
        isHeld = true;
    }

    public void Release()
    {
        isHeld = false;
        holdPoint = null;
    }

    public void Throw(Vector3 impulse)
    {
        Release();
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (!isOwner || !isHeld || holdPoint == null)
            return;

        Vector3 toTarget = holdPoint.position - rb.position;

        // Spring force toward hold point, damped by current velocity
        Vector3 springForce = toTarget * holdStiffness;
        Vector3 dampingForce = -rb.linearVelocity * holdDamping;
        Vector3 force = springForce + dampingForce;

        // Clamp so a wall/obstruction can't cause an infinite force spiral
        force = Vector3.ClampMagnitude(force, maxHoldForce);

        rb.AddForce(force, ForceMode.Force);

        // Belt-and-suspenders: clamp velocity so nothing gets flung
        if (rb.linearVelocity.magnitude > maxHoldSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxHoldSpeed;
    }
}