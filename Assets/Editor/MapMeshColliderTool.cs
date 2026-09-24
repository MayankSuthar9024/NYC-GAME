using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapMeshColliderTool : EditorWindow
{
    public enum TargetScope
    {
        ActiveScene,
        SelectionOnly
    }

    private TargetScope scope = TargetScope.ActiveScene;
    private bool skipExistingColliders = true;
    private bool requireMeshRenderer = true;
    private bool autoConvex = true;
    private bool ignorePlayerAndCamera = true;
    private bool ignoreUI = true;

    // Prefab settings
    private bool processBuildings = true;
    private bool processEnvironments = true;
    private bool processProps = true;
    private bool processVehicles = true;
    private bool processGeneric = true;

    private Vector2 scrollPos;

    [MenuItem("Tools/Map/Map Collider Tool Window", false, 0)]
    [MenuItem("Window/Map/Map Collider Tool", false, 100)]
    public static void ShowWindow()
    {
        MapMeshColliderTool window = GetWindow<MapMeshColliderTool>("Map Colliders");
        window.minSize = new Vector2(420, 520);
        window.Show();
    }

    [MenuItem("Tools/Map/Add Mesh Colliders to Active Scene", false, 1)]
    public static void AddCollidersToActiveSceneQuick()
    {
        int added = ProcessSceneGameObjects(TargetScope.ActiveScene, true, true, true, true, true);
        EditorUtility.DisplayDialog("Map Colliders", $"Complete! Added MeshColliders to {added} objects in active scene.", "OK");
    }

    [MenuItem("Tools/Map/Add Mesh Colliders to Selected Objects", false, 2)]
    public static void AddCollidersToSelectedQuick()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Selection Required", "Please select at least one GameObject in the Hierarchy first.", "OK");
            return;
        }

        int added = ProcessSceneGameObjects(TargetScope.SelectionOnly, true, true, true, true, true);
        EditorUtility.DisplayDialog("Map Colliders", $"Complete! Added MeshColliders to {added} objects under selection.", "OK");
    }

    [MenuItem("Tools/Map/Add Mesh Colliders to Polygon Prefabs", false, 3)]
    public static void AddCollidersToPrefabsQuick()
    {
        if (!EditorUtility.DisplayDialog("Process Prefabs",
            "This will add MeshCollider components directly to all environment, building, prop, vehicle, and generic prefabs in Assets/PolygonBattleRoyale/Prefabs/.\n\nContinue?",
            "Yes, Update Prefabs", "Cancel"))
        {
            return;
        }

        int prefabsModified = ProcessPolygonPrefabs(true, true, true, true, true, true, true, true);
        EditorUtility.DisplayDialog("Prefabs Updated", $"Successfully updated {prefabsModified} prefabs with MeshColliders!", "OK");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Map Mesh Collider Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Quickly adds MeshColliders to environment elements so the player cannot walk or fall through them.\n" +
            "Static objects use concave meshes so doors/interiors are accessible.",
            MessageType.Info);

        EditorGUILayout.Space(10);
        DrawSceneSection();

        EditorGUILayout.Space(15);
        DrawPrefabSection();

        EditorGUILayout.Space(15);
        DrawQuickSetupSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawSceneSection()
    {
        EditorGUILayout.LabelField("1. Scene GameObjects", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        scope = (TargetScope)EditorGUILayout.EnumPopup("Target Scope", scope);
        skipExistingColliders = EditorGUILayout.Toggle("Skip If Collider Exists", skipExistingColliders);
        requireMeshRenderer = EditorGUILayout.Toggle("Require MeshRenderer", requireMeshRenderer);
        autoConvex = EditorGUILayout.Toggle("Auto Convex (Dynamic Rigidbody)", autoConvex);
        ignorePlayerAndCamera = EditorGUILayout.Toggle("Ignore Player & Camera", ignorePlayerAndCamera);
        ignoreUI = EditorGUILayout.Toggle("Ignore UI Elements", ignoreUI);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Analyze Scene Meshes", GUILayout.Height(24)))
        {
            AnalyzeScene();
        }

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.35f);
        if (GUILayout.Button(scope == TargetScope.ActiveScene ? "Add Mesh Colliders to Active Scene" : "Add Mesh Colliders to Selected", GUILayout.Height(34)))
        {
            int added = ProcessSceneGameObjects(scope, skipExistingColliders, requireMeshRenderer, autoConvex, ignorePlayerAndCamera, ignoreUI);
            EditorUtility.DisplayDialog("Done", $"Added MeshColliders to {added} objects.", "OK");
        }

        GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
        if (GUILayout.Button("Remove Mesh Colliders from Scene", GUILayout.Height(24)))
        {
            if (EditorUtility.DisplayDialog("Confirm Removal", "Are you sure you want to remove MeshColliders from objects in the scene?", "Yes, Remove", "Cancel"))
            {
                int removed = RemoveCollidersFromScene(scope);
                EditorUtility.DisplayDialog("Removed", $"Removed {removed} MeshColliders.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void DrawPrefabSection()
    {
        EditorGUILayout.LabelField("2. Polygon Prefabs (Project Assets)", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Add MeshColliders directly to the prefab assets:", EditorStyles.miniLabel);

        processBuildings = EditorGUILayout.Toggle("Buildings (12)", processBuildings);
        processEnvironments = EditorGUILayout.Toggle("Environments (41)", processEnvironments);
        processProps = EditorGUILayout.Toggle("Props (91)", processProps);
        processVehicles = EditorGUILayout.Toggle("Vehicles (22)", processVehicles);
        processGeneric = EditorGUILayout.Toggle("Generic (22)", processGeneric);

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button("Add Mesh Colliders to Selected Prefabs", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog("Update Prefab Assets",
                "This will add MeshCollider to the selected prefab assets in PolygonBattleRoyale. All instances in current and future scenes will immediately inherit them.",
                "Proceed", "Cancel"))
            {
                int count = ProcessPolygonPrefabs(processBuildings, processEnvironments, processProps, processVehicles, processGeneric, skipExistingColliders, requireMeshRenderer, autoConvex);
                EditorUtility.DisplayDialog("Prefabs Updated", $"Successfully updated {count} prefabs with MeshColliders!", "OK");
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickSetupSection()
    {
        EditorGUILayout.LabelField("3. Quick Scene Root Helper", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Attach 'AutoMapMeshCollider' to 'DemoScene' in Hierarchy:", EditorStyles.miniLabel);

        if (GUILayout.Button("Attach AutoMapMeshCollider to 'DemoScene'", GUILayout.Height(28)))
        {
            GameObject demoSceneGo = GameObject.Find("DemoScene");
            if (demoSceneGo == null)
            {
                EditorUtility.DisplayDialog("Not Found", "Could not find GameObject named 'DemoScene' in active scene. Please select your map root object manually in Hierarchy and use Component > Map > Auto Map Mesh Collider.", "OK");
            }
            else
            {
                AutoMapMeshCollider comp = demoSceneGo.GetComponent<AutoMapMeshCollider>();
                if (comp == null)
                {
                    comp = Undo.AddComponent<AutoMapMeshCollider>(demoSceneGo);
                    EditorUtility.SetDirty(demoSceneGo);
                    EditorSceneManager.MarkSceneDirty(demoSceneGo.scene);
                    EditorUtility.DisplayDialog("Attached", "Attached AutoMapMeshCollider component to 'DemoScene'. You can now inspect it and click 'Bake Mesh Colliders To Hierarchy Now' or let it run automatically on Awake.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Already Exists", "'DemoScene' already has AutoMapMeshCollider attached.", "OK");
                }
                Selection.activeGameObject = demoSceneGo;
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void AnalyzeScene()
    {
        MeshFilter[] meshFilters = FindObjectsByType<MeshFilter>(FindObjectsInactive.Include);
        int total = 0;
        int withColliders = 0;
        int withoutColliders = 0;

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            total++;
            if (mf.GetComponent<Collider>() != null)
                withColliders++;
            else
                withoutColliders++;
        }

        EditorUtility.DisplayDialog("Scene Mesh Analysis",
            $"Total Meshes in Scene: {total}\n" +
            $"With Collider: {withColliders}\n" +
            $"Without Collider (Player can pass through): {withoutColliders}",
            "OK");
    }

    public static int ProcessSceneGameObjects(
        TargetScope targetScope,
        bool skipExisting,
        bool needMeshRenderer,
        bool convexAuto,
        bool skipPlayerCamera,
        bool skipUI)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();

        if (targetScope == TargetScope.ActiveScene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                meshFilters.AddRange(root.GetComponentsInChildren<MeshFilter>(true));
            }
        }
        else
        {
            if (Selection.gameObjects != null)
            {
                foreach (GameObject sel in Selection.gameObjects)
                {
                    meshFilters.AddRange(sel.GetComponentsInChildren<MeshFilter>(true));
                }
            }
        }

        int addedCount = 0;
        int total = meshFilters.Count;

        try
        {
            for (int i = 0; i < total; i++)
            {
                MeshFilter mf = meshFilters[i];
                if (mf == null) continue;

                if (i % 25 == 0)
                {
                    EditorUtility.DisplayProgressBar("Adding Mesh Colliders", $"Processing {i}/{total}: {mf.gameObject.name}", (float)i / total);
                }

                if (mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0) continue;

                GameObject go = mf.gameObject;

                if (skipPlayerCamera)
                {
                    if (go.CompareTag("Player") || go.GetComponent<CharacterController>() != null || go.GetComponent<PlayerController>() != null)
                        continue;
                    if (go.GetComponent<Camera>() != null || go.GetComponent<Light>() != null)
                        continue;
                }

                if (skipUI)
                {
                    if (go.GetComponent<RectTransform>() != null || go.GetComponent<CanvasRenderer>() != null)
                        continue;
                }

                if (needMeshRenderer)
                {
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mr == null || !mr.enabled)
                        continue;
                }

                if (skipExisting && go.GetComponent<Collider>() != null)
                    continue;

                MeshCollider mc = go.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = Undo.AddComponent<MeshCollider>(go);
                    addedCount++;
                }

                mc.sharedMesh = mf.sharedMesh;

                if (convexAuto)
                {
                    Rigidbody rb = go.GetComponent<Rigidbody>();
                    mc.convex = (rb != null && !rb.isKinematic);
                }

                EditorUtility.SetDirty(go);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (addedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        Debug.Log($"[MapMeshColliderTool] Added MeshColliders to {addedCount} objects in scene.");
        return addedCount;
    }

    public static int RemoveCollidersFromScene(TargetScope targetScope)
    {
        List<MeshCollider> colliders = new List<MeshCollider>();

        if (targetScope == TargetScope.ActiveScene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (GameObject root in activeScene.GetRootGameObjects())
            {
                colliders.AddRange(root.GetComponentsInChildren<MeshCollider>(true));
            }
        }
        else
        {
            if (Selection.gameObjects != null)
            {
                foreach (GameObject sel in Selection.gameObjects)
                {
                    colliders.AddRange(sel.GetComponentsInChildren<MeshCollider>(true));
                }
            }
        }

        int removed = 0;
        foreach (MeshCollider mc in colliders)
        {
            if (mc == null) continue;
            if (mc.isTrigger) continue; // Keep triggers like zones

            Undo.DestroyObjectImmediate(mc);
            removed++;
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        Debug.Log($"[MapMeshColliderTool] Removed {removed} MeshColliders.");
        return removed;
    }

    public static int ProcessPolygonPrefabs(
        bool buildings,
        bool environments,
        bool props,
        bool vehicles,
        bool generic,
        bool skipExisting,
        bool needMeshRenderer,
        bool convexAuto)
    {
        List<string> targetFolders = new List<string>();
        string basePath = "Assets/PolygonBattleRoyale/Prefabs";

        if (buildings) targetFolders.Add(Path.Combine(basePath, "Buildings"));
        if (environments) targetFolders.Add(Path.Combine(basePath, "Environments"));
        if (props) targetFolders.Add(Path.Combine(basePath, "Props"));
        if (vehicles) targetFolders.Add(Path.Combine(basePath, "Vehicles"));
        if (generic) targetFolders.Add(Path.Combine(basePath, "Generic"));

        List<string> prefabPaths = new List<string>();
        foreach (string folder in targetFolders)
        {
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*.prefab", SearchOption.AllDirectories);
                prefabPaths.AddRange(files);
            }
        }

        int modifiedCount = 0;
        int total = prefabPaths.Count;

        try
        {
            for (int i = 0; i < total; i++)
            {
                string path = prefabPaths[i].Replace('\\', '/');
                EditorUtility.DisplayProgressBar("Updating Prefab Colliders", $"[{i + 1}/{total}] {Path.GetFileName(path)}", (float)i / total);

                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                if (contents == null) continue;

                bool modifiedThisPrefab = false;
                MeshFilter[] mfs = contents.GetComponentsInChildren<MeshFilter>(true);

                foreach (MeshFilter mf in mfs)
                {
                    if (mf == null || mf.sharedMesh == null || mf.sharedMesh.vertexCount == 0) continue;
                    GameObject go = mf.gameObject;

                    if (needMeshRenderer)
                    {
                        MeshRenderer mr = go.GetComponent<MeshRenderer>();
                        if (mr == null || !mr.enabled) continue;
                    }

                    if (skipExisting && go.GetComponent<Collider>() != null) continue;

                    MeshCollider mc = go.GetComponent<MeshCollider>();
                    if (mc == null)
                    {
                        mc = go.AddComponent<MeshCollider>();
                        modifiedThisPrefab = true;
                    }

                    mc.sharedMesh = mf.sharedMesh;
                    if (convexAuto)
                    {
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        mc.convex = (rb != null && !rb.isKinematic);
                    }
                }

                if (modifiedThisPrefab)
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    modifiedCount++;
                }

                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"[MapMeshColliderTool] Added MeshColliders to {modifiedCount} prefabs in Assets/PolygonBattleRoyale/Prefabs.");
        return modifiedCount;
    }
}
