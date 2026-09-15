using PurrNet;
using UnityEngine;

public class PlayerGrabber : NetworkBehaviour
{
    [Header("Grab Settings")]
    public Transform holdPoint;
    public Transform cameraTransform;
    public float grabRange = 3f;
    public float throwForce = 12f;
    public LayerMask grabbableMask;

    private InputSystem_Actions input;
    private Grabbable held;

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        if (!isOwner)
        {
            enabled = false;
            return;
        }

        // Guard against OnSpawned firing more than once for the same object
        if (input != null)
        {
            input.Player.Disable();
            input.Dispose();
        }

        input = new InputSystem_Actions();
        input.Player.Grab.performed += _ => OnGrabPressed();
        input.Player.Throw.performed += _ => OnThrowPressed();
        input.Player.Enable();
    }

    private void OnDestroy()
    {
        if (input == null) return;
        input.Player.Disable();
        input.Dispose();
    }

    private void OnGrabPressed()
    {
        if (held == null) TryGrab();
        else ReleaseHeld();
    }

    private void TryGrab()
    {
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, grabRange, grabbableMask))
            return;

        var grabbable = hit.collider.GetComponent<Grabbable>();
        if (grabbable == null || grabbable.IsHeld) return;

        RequestGrab(grabbable);
    }

    [ServerRpc(requireOwnership: false)]
    private void RequestGrab(Grabbable target, RPCInfo info = default)
    {
        if (target == null || target.IsHeld) return;
        // Server hands ownership of the object to whoever asked for it
        target.GiveOwnership(info.sender);
        // Tell that same client (now the owner) to actually start holding it
        TargetBeginHold(info.sender, target);
    }

    [TargetRpc]
    private void TargetBeginHold(PlayerID target, Grabbable grabbable)
    {
        grabbable.BeginHold(holdPoint);
        held = grabbable;
    }

    private void ReleaseHeld()
    {
        var released = held;
        held = null;
        released.Release();
    }

    private void OnThrowPressed()
    {
        if (held == null) return;
        held.Throw(cameraTransform.forward * throwForce);
        held = null;
    }
}