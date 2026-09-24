using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatically adds MeshColliders to all child elements that have a MeshFilter and MeshRenderer.
/// Useful for map roots (like DemoScene) so the player cannot walk through environment elements.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Map/Auto Map Mesh Collider")]
public class AutoMapMeshCollider : MonoBehaviour
{
    [Header("Runtime Settings")]
    [Tooltip("If true, automatically ensures all map children have colliders when entering Play mode in Awake().")]
    public bool addOnAwake = true;

    [Header("Collider Settings")]
    [Tooltip("If true, objects that already have ANY collider (MeshCollider, BoxCollider, etc.) are skipped.")]
    public bool skipExistingColliders = true;

    [Tooltip("If true, only objects with an active MeshRenderer will receive a collider.")]
    public bool requireMeshRenderer = true;

    [Tooltip("If true, static objects are non-convex (concave, so you can walk through doorways and into rooms), while dynamic Rigidbodies are convex (required by PhysX).")]
    public bool autoConvex = true;

    [Header("Exclusions")]
    [Tooltip("Tags to exclude from receiving colliders (e.g., Player).")]
    public string[] excludedTags = new string[] { "Player", "MainCamera" };

    [Tooltip("Names of GameObjects or prefixes to ignore (case-insensitive).")]
    public string[] ignoredNameKeywords = new string[] { "player", "camera", "weapon", "fx", "particle", "light" };

    private void Awake()
    {
        if (addOnAwake)
        {
            ApplyColliders();
        }
    }

    /// <summary>
    /// Applies MeshColliders to all eligible child objects.
    /// Can be called at runtime or from Editor.
    /// </summary>
    /// <returns>Number of colliders added.</returns>
    [ContextMenu("Apply Mesh Colliders Now")]
    public int ApplyColliders()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        int addedCount = 0;
        int skippedCount = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            if (mf.sharedMesh.vertexCount == 0) continue;

            GameObject go = mf.gameObject;

            // Skip if excluded
            if (ShouldExclude(go))
            {
                skippedCount++;
                continue;
            }

            if (requireMeshRenderer)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr == null || !mr.enabled)
                {
                    skippedCount++;
                    continue;
                }
            }

            // Check existing collider
            if (skipExistingColliders && go.GetComponent<Collider>() != null)
            {
                skippedCount++;
                continue;
            }

            MeshCollider mc = go.GetComponent<MeshCollider>();
            if (mc == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    mc = Undo.AddComponent<MeshCollider>(go);
                }
                else
#endif
                {
                    mc = go.AddComponent<MeshCollider>();
                }
                addedCount++;
            }

            mc.sharedMesh = mf.sharedMesh;

            if (autoConvex)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                mc.convex = (rb != null && !rb.isKinematic);
            }
        }

        Debug.Log($"[AutoMapMeshCollider] Processed on '{name}': {addedCount} MeshColliders added, {skippedCount} skipped.");
        return addedCount;
    }

    /// <summary>
    /// Removes MeshColliders from children that don't have a Rigidbody.
    /// </summary>
    [ContextMenu("Remove Added Mesh Colliders")]
    public int RemoveColliders()
    {
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>(true);
        int removedCount = 0;

        foreach (MeshCollider mc in meshColliders)
        {
            if (mc == null) continue;

            // Don't remove if object was explicitly tagged as trigger or has special components
            if (mc.isTrigger) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(mc);
            }
            else
#endif
            {
                Destroy(mc);
            }
            removedCount++;
        }

        Debug.Log($"[AutoMapMeshCollider] Removed {removedCount} MeshColliders from '{name}'.");
        return removedCount;
    }

    private bool ShouldExclude(GameObject go)
    {
        // Don't add to player or character controller
        if (go.GetComponent<CharacterController>() != null || go.GetComponent<PlayerController>() != null)
            return true;

        if (go.GetComponent<Camera>() != null || go.GetComponent<Light>() != null)
            return true;

        // Tag exclusion
        if (excludedTags != null)
        {
            foreach (string t in excludedTags)
            {
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t))
                    return true;
            }
        }

        // Name keyword exclusion
        string lowerName = go.name.ToLower();
        if (ignoredNameKeywords != null)
        {
            foreach (string kw in ignoredNameKeywords)
            {
                if (!string.IsNullOrEmpty(kw) && lowerName.Contains(kw))
                    return true;
            }
        }

        return false;
    }
}
