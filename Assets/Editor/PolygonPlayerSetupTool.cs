using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PolygonPlayerSetupTool : EditorWindow
{
    private static readonly string[] CharacterPrefabNames = new string[]
    {
        "Character_MilitaryMale_01",
        "Character_MercenaryMale_01",
        "Character_GhillieSuit_01",
        "Character_MilitaryFemale_01",
        "Character_MercenaryFemale_01",
        "Character_BusinessMale_01",
        "Character_SportyMale_01",
        "Character_SportyMale_02",
        "Character_SportyFemale_01",
        "Character_SportyFemale_02",
        "Character_GothFemale_01",
        "Character_SportsBraFemale_01",
        "Character_70sFemale_01",
        "Character_RedneckMale_01",
        "Character_ToplessMale_01"
    };

    private static readonly string[] CharacterDisplayNames = new string[]
    {
        "Military Male (Default)",
        "Mercenary Male",
        "Ghillie Suit Sniper",
        "Military Female",
        "Mercenary Female",
        "Business Male",
        "Sporty Male 01",
        "Sporty Male 02",
        "Sporty Female 01",
        "Sporty Female 02",
        "Goth Female",
        "Sports Bra Female",
        "70s Female",
        "Redneck Male",
        "Topless Male"
    };

    private int selectedCharacterIndex = 0;
    private const string AnimatorControllerPath = "Assets/Bot/Animation/YBotController.controller";
    private const string CharactersModelPath = "Assets/PolygonBattleRoyale/Models/Characters/Characters.fbx";
    private const string PrefabBasePath = "Assets/PolygonBattleRoyale/Prefabs/Characters/";
    private const string SavedPlayerPrefabPath = "Assets/PolygonBattleRoyale/Prefabs/Player_Polygon.prefab";

    [MenuItem("Tools/Player/Open Player Model Switcher Window", false, 10)]
    public static void ShowWindow()
    {
        PolygonPlayerSetupTool window = GetWindow<PolygonPlayerSetupTool>("Player Switcher");
        window.minSize = new Vector2(380, 420);
        window.Show();
    }

    [MenuItem("Tools/Player/Switch Player to Polygon Military Male", false, 0)]
    public static void SwitchToMilitaryMale()
    {
        SetupPlayerInActiveScene("Character_MilitaryMale_01");
    }

    [MenuItem("Tools/Player/Switch Player to Polygon Mercenary Male", false, 1)]
    public static void SwitchToMercenaryMale()
    {
        SetupPlayerInActiveScene("Character_MercenaryMale_01");
    }

    [MenuItem("Tools/Player/Switch Player to Polygon Ghillie Suit", false, 2)]
    public static void SwitchToGhillieSuit()
    {
        SetupPlayerInActiveScene("Character_GhillieSuit_01");
    }

    [InitializeOnLoadMethod]
    private static void AutoSetupPlayerOnCompile()
    {
        EditorApplication.delayCall += () =>
        {
            // If the scene currently contains YBot or no Polygon player yet, automatically switch
            GameObject ybot = GameObject.Find("YBot");
            if (ybot != null)
            {
                Debug.Log("[PolygonPlayerSetup] Found old 'YBot' in active scene. Automatically converting to Polygon Character_MilitaryMale_01...");
                SetupPlayerInActiveScene("Character_MilitaryMale_01");
            }
        };
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Polygon Player Model Switcher", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select any character model from the Polygon Battle Royale pack.\n" +
            "Clicking 'Apply' replaces the player in the active scene while preserving position, movement, animations, and camera tracking.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        selectedCharacterIndex = EditorGUILayout.Popup("Select Character", selectedCharacterIndex, CharacterDisplayNames);

        EditorGUILayout.Space(15);
        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.35f);
        if (GUILayout.Button($"Apply '{CharacterDisplayNames[selectedCharacterIndex]}' to Scene Player", GUILayout.Height(36)))
        {
            SetupPlayerInActiveScene(CharacterPrefabNames[selectedCharacterIndex]);
        }

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button("Create / Update Reusable 'Player_Polygon.prefab'", GUILayout.Height(28)))
        {
            CreatePolygonPlayerPrefab(CharacterPrefabNames[selectedCharacterIndex]);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(15);

        // Display info about active player
        GameObject currentPlayer = FindCurrentPlayer();
        if (currentPlayer != null)
        {
            EditorGUILayout.LabelField("Current Scene Player:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Name: {currentPlayer.name}");
            EditorGUILayout.LabelField($"Position: {currentPlayer.transform.position}");
            if (GUILayout.Button("Ping Player in Hierarchy", GUILayout.Height(22)))
            {
                Selection.activeGameObject = currentPlayer;
                EditorGUIUtility.PingObject(currentPlayer);
            }
        }
    }

    public static GameObject SetupPlayerInActiveScene(string characterPrefabName)
    {
        string prefabPath = Path.Combine(PrefabBasePath, characterPrefabName + ".prefab").Replace('\\', '/');
        GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (characterPrefab == null)
        {
            Debug.LogError($"[PolygonPlayerSetup] Could not find prefab at: {prefabPath}");
            return null;
        }

        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorControllerPath);
        if (animController == null)
        {
            Debug.LogError($"[PolygonPlayerSetup] Could not find AnimatorController at: {AnimatorControllerPath}");
        }

        // Find existing player to copy position/rotation
        GameObject oldPlayer = FindCurrentPlayer();
        Vector3 spawnPos = new Vector3(-6.3f, 0f, -19.2f);
        Quaternion spawnRot = Quaternion.identity;

        if (oldPlayer != null)
        {
            spawnPos = oldPlayer.transform.position;
            spawnRot = oldPlayer.transform.rotation;
        }

        // Instantiate the Polygon character
        GameObject newPlayer = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
        Undo.RegisterCreatedObjectUndo(newPlayer, "Create Polygon Player");

        newPlayer.name = "Player";
        newPlayer.transform.position = spawnPos;
        newPlayer.transform.rotation = spawnRot;
        newPlayer.tag = "Player";

        // Setup CharacterController
        CharacterController cc = newPlayer.GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = Undo.AddComponent<CharacterController>(newPlayer);
        }
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        cc.skinWidth = 0.08f;
        cc.minMoveDistance = 0.001f;

        // Setup Animator
        Animator animator = newPlayer.GetComponent<Animator>();
        if (animator == null)
        {
            animator = Undo.AddComponent<Animator>(newPlayer);
        }
        if (animController != null)
        {
            animator.runtimeAnimatorController = animController;
        }
        animator.applyRootMotion = false;

        // Setup PlayerController
        PlayerController pc = newPlayer.GetComponent<PlayerController>();
        if (pc == null)
        {
            pc = Undo.AddComponent<PlayerController>(newPlayer);
        }
        pc.speed = 5.0f;
        pc.backwardSpeedMultiplier = 0.6f;
        pc.jumpForce = 5.0f;
        pc.gravity = -9.81f;
        pc.controller = cc;
        pc.animator = animator;
        pc.animationDampTime = 0.1f;
        pc.maxVelocityZ = 7f;
        pc.maxBackwardVelocityZ = 5f;
        pc.maxVelocityX = 3f;

        // Hook up Camera
        HookupCamera(newPlayer.transform);

        // Delete old player if present
        if (oldPlayer != null && oldPlayer != newPlayer)
        {
            Undo.DestroyObjectImmediate(oldPlayer);
        }

        // Mark scene dirty and save
        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Selection.activeGameObject = newPlayer;
        EditorGUIUtility.PingObject(newPlayer);

        Debug.Log($"[PolygonPlayerSetup] Successfully replaced player model with '{characterPrefabName}' in scene '{activeScene.name}'!");

        // Also create/update the reusable prefab
        CreatePolygonPlayerPrefab(characterPrefabName);

        return newPlayer;
    }

    private static void HookupCamera(Transform playerTransform)
    {
        FirstPersonCamara[] fpcList = Object.FindObjectsByType<FirstPersonCamara>(FindObjectsInactive.Include);
        foreach (FirstPersonCamara fpc in fpcList)
        {
            Undo.RecordObject(fpc, "Update Camera Target");
            fpc.player = playerTransform;
            fpc.cameraOffset = new Vector3(0f, 1.6f, 0f);
            EditorUtility.SetDirty(fpc);
            Debug.Log($"[PolygonPlayerSetup] Linked FirstPersonCamara on '{fpc.name}' to new player.");
        }

        // If no FirstPersonCamara found, check Camera.main
        if (fpcList.Length == 0 && Camera.main != null)
        {
            FirstPersonCamara fpc = Camera.main.GetComponent<FirstPersonCamara>();
            if (fpc != null)
            {
                Undo.RecordObject(fpc, "Update Camera Target");
                fpc.player = playerTransform;
                EditorUtility.SetDirty(fpc);
            }
        }
    }

    private static GameObject FindCurrentPlayer()
    {
        // 1. By tag
        try
        {
            GameObject tagged = GameObject.FindWithTag("Player");
            if (tagged != null) return tagged;
        }
        catch { }

        // 2. By PlayerController
        PlayerController pc = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (pc != null) return pc.gameObject;

        // 3. By exact name
        GameObject byName = GameObject.Find("YBot");
        if (byName != null) return byName;

        byName = GameObject.Find("Player");
        if (byName != null) return byName;

        return null;
    }

    public static void CreatePolygonPlayerPrefab(string characterPrefabName)
    {
        string sourcePrefabPath = Path.Combine(PrefabBasePath, characterPrefabName + ".prefab").Replace('\\', '/');
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
        if (sourcePrefab == null) return;

        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorControllerPath);

        // Instantiate temporary object to build prefab
        GameObject tempGo = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
        tempGo.name = "Player_Polygon";
        tempGo.tag = "Player";

        CharacterController cc = tempGo.GetComponent<CharacterController>();
        if (cc == null) cc = tempGo.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        cc.skinWidth = 0.08f;
        cc.minMoveDistance = 0.001f;

        Animator anim = tempGo.GetComponent<Animator>();
        if (anim == null) anim = tempGo.AddComponent<Animator>();
        if (animController != null) anim.runtimeAnimatorController = animController;
        anim.applyRootMotion = false;

        PlayerController pc = tempGo.GetComponent<PlayerController>();
        if (pc == null) pc = tempGo.AddComponent<PlayerController>();
        pc.speed = 5.0f;
        pc.backwardSpeedMultiplier = 0.6f;
        pc.jumpForce = 5.0f;
        pc.gravity = -9.81f;
        pc.controller = cc;
        pc.animator = anim;

        PrefabUtility.SaveAsPrefabAsset(tempGo, SavedPlayerPrefabPath);
        DestroyImmediate(tempGo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PolygonPlayerSetup] Saved ready-to-use prefab to '{SavedPlayerPrefabPath}'.");
    }
}
