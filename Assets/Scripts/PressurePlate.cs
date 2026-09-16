using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Header("Requirements")]
    [Tooltip("Layers that count as valid weight on this plate (crates, players, etc).")]
    public LayerMask validMask;
    [Tooltip("Total combined mass required to activate the plate.")]
    public float requiredWeight = 1f;

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    [Header("Fire Behavior")]
    public bool fireOnce = true;

    private bool hasFiredOnce;

    [Header("Feedback (optional)")]
    public Transform visualPlate;
    public float pressedOffset = -0.05f;

    private readonly Dictionary<Collider, float> objectsOnPlate = new Dictionary<Collider, float>();
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

        float weight = GetWeight(other);
        objectsOnPlate[other] = weight;
        Evaluate();
    }

    private void OnTriggerExit(Collider other)
    {
        objectsOnPlate.Remove(other);
        Evaluate();
    }

    private float GetWeight(Collider col)
    {
        var rb = col.attachedRigidbody;
        return rb != null ? rb.mass : 1f; // fallback weight if no Rigidbody found
    }

    private float CurrentWeight()
    {
        float total = 0f;
        foreach (var kvp in objectsOnPlate)
            total += kvp.Value;
        return total;
    }

    private void Evaluate()
    {
        // Clean out anything destroyed/disabled while it was overlapping
        var toRemove = new List<Collider>();
        foreach (var col in objectsOnPlate.Keys)
        {
            if (col == null || !col.gameObject.activeInHierarchy)
                toRemove.Add(col);
        }
        foreach (var col in toRemove)
            objectsOnPlate.Remove(col);

        bool shouldBeActive = CurrentWeight() >= requiredWeight;

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