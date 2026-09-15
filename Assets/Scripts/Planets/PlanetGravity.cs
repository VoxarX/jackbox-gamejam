using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public Transform planet;
    public bool alignToPlanet = true;
    public float gravityConstant = 9.8f;
    public float alignmentSpeed = 5f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // TODO: Make this find the closest planet
        if (planet == null)
            planet = GameObject.FindWithTag("Planet")?.transform;
    }

    void FixedUpdate()
    {
        // If the script is disabled by the Controller, do nothing
        if (planet == null || !this.enabled) return;

        Vector3 toCenter = (planet.position - transform.position).normalized;

        // Apply Force
        rb.AddForce(toCenter * gravityConstant, ForceMode.Acceleration);

        // Align Rotation
        if (alignToPlanet)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -toCenter) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * alignmentSpeed);
        }
    }
}