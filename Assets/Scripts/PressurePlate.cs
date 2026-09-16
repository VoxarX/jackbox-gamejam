using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("Layers that count as valid weight on this plate (crates, players, etc).")]
    public LayerMask validMask;
    [Tooltip("Minimum number of valid objects simultaneously on the plate to activate it.")]
    public int requiredCount = 1;

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    [Header("Fire Behavior")]
    public bool fireOnce = true;

    private bool hasFiredOnce;

    [Header("Feedback (optional)")]
    public Transform visualPlate;
    public float pressedOffset = -0.05f;

    private readonly HashSet<Collider> objectsOnPlate = new HashSet<Collider>();
    private bool isActive;
    private Vector3 visualRestLocalPos;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (visualPlate != null)
            visualRestLocalPos = visualPlate.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & validMask) == 0)
            return;

        objectsOnPlate.Add(other);
        Evaluate();
    }

    private void OnTriggerExit(Collider other)
    {
        objectsOnPlate.Remove(other);
        Evaluate();
    }

    private void Evaluate()
    {
        objectsOnPlate.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);

        bool shouldBeActive = objectsOnPlate.Count >= requiredCount;

        if (shouldBeActive && !isActive)
        {
            isActive = true;
            if (visualPlate != null)
                visualPlate.localPosition = visualRestLocalPos + Vector3.up * pressedOffset;

            if (!fireOnce || !hasFiredOnce)
            {
                hasFiredOnce = true;
                onActivated.Invoke();
            }
        }
        else if (!shouldBeActive && isActive)
        {
            isActive = false;
            if (visualPlate != null)
                visualPlate.localPosition = visualRestLocalPos;

            if (!fireOnce)
                onDeactivated.Invoke();
        }
    }
}