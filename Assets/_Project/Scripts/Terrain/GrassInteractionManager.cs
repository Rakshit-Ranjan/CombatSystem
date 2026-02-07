using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages grass interactions by tracking colliders and sending their positions to the grass shader.
/// Attach this to any GameObject in your scene (only one instance needed).
/// </summary>
public class GrassInteractionManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Maximum number of simultaneous interactions")]
    [Range(1, 10)]
    public int maxInteractors = 10;
    
    [Tooltip("How often to update interactions (seconds). Lower = more responsive but higher cost")]
    [Range(0.01f, 0.1f)]
    public float updateInterval = 0.033f; // ~30 updates per second
    
    [Tooltip("Layers that can interact with grass")]
    public LayerMask interactionLayers = -1; // All layers by default
    
    [Header("Detection Settings")]
    [Tooltip("Radius around camera to detect interactors")]
    public float detectionRadius = 50f;
    
    [Tooltip("Only objects moving faster than this will interact")]
    public float minimumVelocity = 0.1f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.green;

    [Header("Foot Interactions")]
    public Transform leftFoot;
    public Transform rightFoot;
    public float footRadius = 0.35f;
    private Vector3 lastLeftFootPos;
    private Vector3 lastRightFootPos;

    
    private Vector4[] interactorPositions = new Vector4[10];
    private int interactorCount = 0;
    private float lastUpdateTime = 0f;
    
    private Dictionary<Collider, Vector3> lastPositions = new Dictionary<Collider, Vector3>();
    private Dictionary<Collider, Vector3> smoothedPositions = new();
    private Dictionary<Collider, float> smoothedVelocities = new();
    private Camera mainCamera;

    private Vector3 averagedVelocity;


    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("GrassInteractionManager: No main camera found!");
        }
        if (leftFoot) lastLeftFootPos = leftFoot.position;
        if (rightFoot) lastRightFootPos = rightFoot.position;
    }
    
    
    void Update() {
        // Throttle updates for performance
        if (Time.time - lastUpdateTime < updateInterval)
            return;
        
        lastUpdateTime = Time.time;
        
        UpdateInteractors();
        SendToShader();
    }
    
    void UpdateInteractors()
    {
        interactorCount = 0;
        averagedVelocity = Vector3.zero;
        int velocityCount = 0;

        AddFootInteractor(leftFoot, ref lastLeftFootPos);
        AddFootInteractor(rightFoot, ref lastRightFootPos);

        Vector3 searchCenter = mainCamera != null
            ? mainCamera.transform.position
            : transform.position;

        Collider[] nearbyColliders = Physics.OverlapSphere(
            searchCenter,
            detectionRadius,
            interactionLayers
        );

        System.Array.Sort(nearbyColliders, (a, b) =>
        {
            float da = Vector3.Distance(searchCenter, a.transform.position);
            float db = Vector3.Distance(searchCenter, b.transform.position);
            return da.CompareTo(db);
        });

        foreach (Collider col in nearbyColliders)
        {
            if (interactorCount >= maxInteractors)
                break;

            if (col == null)
                continue;

            if (col.bounds.min.y > 5f)
                continue;

            Vector3 currentPos = col.transform.position;

            // --- velocity (smoothed) ---
            Vector3 lastPos = lastPositions.TryGetValue(col, out var lp)
                ? lp
                : currentPos;

            float rawSpeed = (currentPos - lastPos).magnitude / updateInterval;

            float prevSpeed = smoothedVelocities.TryGetValue(col, out var sv)
                ? sv
                : 0f;

            float smoothSpeed = Mathf.Lerp(prevSpeed, rawSpeed, 0.25f);

            // kill micro jitter
            if (smoothSpeed < 0.05f)
                smoothSpeed = 0f;

            smoothedVelocities[col] = smoothSpeed;
            lastPositions[col] = currentPos;

            // --- proximity rule (Genshin-style) ---
            bool allowIdle =
                Vector3.Distance(searchCenter, currentPos) < 2.0f;

            if (!allowIdle && smoothSpeed < minimumVelocity)
                continue;

            // --- smooth position ---
            Vector3 prevSmoothPos =
                smoothedPositions.TryGetValue(col, out var sp)
                    ? sp
                    : currentPos;

            Vector3 smoothPos = Vector3.Lerp(
                prevSmoothPos,
                currentPos,
                0.35f
            );

            smoothedPositions[col] = smoothPos;

            float radius = CalculateColliderRadius(col);

            interactorPositions[interactorCount] = new Vector4(
                smoothPos.x,
                smoothPos.y,
                smoothPos.z,
                radius
            );

            interactorCount++;

            averagedVelocity += Vector3.forward * smoothSpeed;
            velocityCount++;
        }

        // --- finalize velocity magnitude only ---
        float finalSpeed =
            velocityCount > 0
                ? averagedVelocity.magnitude / velocityCount
                : 0f;

        Shader.SetGlobalVector(
            "_InteractorVelocity",
            new Vector4(0f, 0f, 0f, finalSpeed)
        );

        // --- cleanup ---
        List<Collider> toRemove = new List<Collider>();
        foreach (var kvp in lastPositions)
        {
            if (kvp.Key == null ||
                !System.Array.Exists(nearbyColliders, c => c == kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var c in toRemove)
        {
            lastPositions.Remove(c);
            smoothedPositions.Remove(c);
            smoothedVelocities.Remove(c);
        }
    }

    
    float CalculateColliderRadius(Collider col)
    {
        // Estimate radius based on collider type
        if (col is SphereCollider sphere)
        {
            return sphere.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.z);
        }
        else if (col is CapsuleCollider capsule)
        {
            return capsule.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.z);
        }
        else if (col is BoxCollider box)
        {
            Vector3 size = box.size;
            return Mathf.Max(size.x * col.transform.lossyScale.x, size.z * col.transform.lossyScale.z) * 0.5f;
        }
        else
        {
            // For other collider types, use bounds
            Bounds bounds = col.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.z);
        }
    }
    
    void SendToShader()
    {
        // Send to global shader parameters
        Shader.SetGlobalVectorArray("_InteractorPositions", interactorPositions);
        Shader.SetGlobalInt("_InteractorCount", interactorCount);
        Shader.SetGlobalVector(
            "_InteractorVelocity",
            new Vector4(
                averagedVelocity.x,
                0f,
                averagedVelocity.z,
                averagedVelocity.magnitude
            )
        );
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying)
            return;
        
        Gizmos.color = gizmoColor;
        
        // Draw detection radius
        if (mainCamera != null)
        {
            Gizmos.DrawWireSphere(mainCamera.transform.position, detectionRadius);
        }
        
        // Draw interaction zones
        for (int i = 0; i < interactorCount; i++)
        {
            Vector3 pos = new Vector3(
                interactorPositions[i].x,
                interactorPositions[i].y,
                interactorPositions[i].z
            );
            float radius = interactorPositions[i].w;
            
            Gizmos.DrawWireSphere(pos, radius);
            
            // Draw vertical line to show position
            Gizmos.DrawLine(pos, pos + Vector3.up * 2f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
            return;
        
        // Draw interaction radius around this object when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void AddFootInteractor(Transform foot, ref Vector3 lastPos)
    {
        if (!foot || interactorCount >= maxInteractors)
            return;

        Vector3 currentPos = foot.position;

        float speed =
            (currentPos - lastPos).magnitude / Mathf.Max(updateInterval, 0.0001f);

        lastPos = currentPos;

        // Smooth speed (kills jitter)
        speed = Mathf.Lerp(0f, speed, 0.35f);

        // Always allow proximity interaction
        interactorPositions[interactorCount] = new Vector4(
            currentPos.x,
            currentPos.y,
            currentPos.z,
            footRadius
        );

        interactorCount++;

        averagedVelocity += Vector3.forward * speed;
    }
}