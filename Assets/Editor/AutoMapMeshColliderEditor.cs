using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoMapMeshCollider))]
public class AutoMapMeshColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoMapMeshCollider script = (AutoMapMeshCollider)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("Bake Mesh Colliders To Hierarchy Now", GUILayout.Height(32)))
        {
            int added = script.ApplyColliders();
            EditorUtility.SetDirty(script.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
            EditorUtility.DisplayDialog("Bake Colliders", $"Successfully added MeshColliders to {added} child elements!", "OK");
        }

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Remove Mesh Colliders From Children", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog("Confirm Remove", "Are you sure you want to remove MeshColliders from children of this object?", "Yes, Remove", "Cancel"))
            {
                int removed = script.RemoveColliders();
                EditorUtility.SetDirty(script.gameObject);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                EditorUtility.DisplayDialog("Remove Colliders", $"Removed {removed} MeshColliders.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;
    }
}
