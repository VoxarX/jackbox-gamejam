using UnityEngine;
using UnityEngine.Events;

public class Additioner : MonoBehaviour
{
    [Header("Target")]
    public float targetCount = 1f;

    [Header("Events")]
    public UnityEvent onReached;

    private float current;
    private bool hasFired;

    public void Add(float someFloat)
    {
        if (hasFired) return;

        current += someFloat;

        if (current >= targetCount)
        {
            hasFired = true;
            onReached.Invoke();
            Debug.Log("Invoked");
        }
    }

    // Optional: call this if you need to reuse the same additioner more than once
    public void ResetCounter()
    {
        current = 0f;
        hasFired = false;
    }
}