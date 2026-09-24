using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class AnimationOptimizerTool
{
    private const string WalkingBackwardFbx = "Assets/Bot/Y Bot@Walking Backward.fbx";
    private const string JumpingFbx = "Assets/Bot/Y Bot@Jumping.fbx";
    private const string WalkingBackwardAnim = "Assets/Bot/Animation/Walking Backward.anim";
    private const string JumpingAnim = "Assets/Bot/Animation/Jumping.anim";
    private const string ControllerPath = "Assets/Bot/Animation/YBotController.controller";

    static AnimationOptimizerTool()
    {
        EditorApplication.delayCall += AutoFixAnimations;
    }

    [MenuItem("Tools/Animation/Fix and Optimize Animations")]
    public static void AutoFixAnimations()
    {
        bool changed = false;

        // 1. Extract Walking Backward if not exists or empty
        AnimationClip walkingBackwardClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkingBackwardAnim);
        if (walkingBackwardClip == null)
        {
            walkingBackwardClip = ExtractClipFromFbx(WalkingBackwardFbx, WalkingBackwardAnim, "Walking Backward");
            if (walkingBackwardClip != null) changed = true;
        }

        // 2. Extract Jumping if not exists or empty
        AnimationClip jumpingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(JumpingAnim);
        if (jumpingClip == null)
        {
            jumpingClip = ExtractClipFromFbx(JumpingFbx, JumpingAnim, "Jumping");
            if (jumpingClip != null) changed = true;
        }

        // 3. Hook into YBotController
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller != null)
        {
            bool controllerModified = false;

            // Search root state machine and blend trees
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == "Jumping" && jumpingClip != null)
                    {
                        if (state.state.motion != jumpingClip)
                        {
                            state.state.motion = jumpingClip;
                            controllerModified = true;
                            Debug.Log("[AnimationOptimizer] Assigned Jumping clip to Jumping state!");
                        }
                    }
                    else if (state.state.motion is BlendTree blendTree)
                    {
                        var children = blendTree.children;
                        for (int i = 0; i < children.Length; i++)
                        {
                            // Check for backward walking slot (position Y == -5)
                            if (Mathf.Approximately(children[i].position.y, -5f) || children[i].position.y < -1f)
                            {
                                if (children[i].motion != walkingBackwardClip && walkingBackwardClip != null)
                                {
                                    children[i].motion = walkingBackwardClip;
                                    controllerModified = true;
                                    Debug.Log("[AnimationOptimizer] Assigned Walking Backward clip to BlendTree!");
                                }
                            }
                        }
                        if (controllerModified)
                        {
                            blendTree.children = children;
                        }
                    }
                }
            }

            if (controllerModified)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                Debug.Log("[AnimationOptimizer] YBotController successfully updated with valid clips!");
                changed = true;
            }
        }

        if (changed)
        {
            AssetDatabase.Refresh();
        }
    }

    private static AnimationClip ExtractClipFromFbx(string fbxPath, string targetAnimPath, string targetClipName)
    {
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (subAssets == null || subAssets.Length == 0)
        {
            Debug.LogWarning($"[AnimationOptimizer] Could not load FBX at {fbxPath}");
            return null;
        }

        AnimationClip foundClip = null;
        foreach (var sub in subAssets)
        {
            if (sub is AnimationClip clip && !clip.name.Contains("__preview__"))
            {
                foundClip = clip;
                break;
            }
        }

        if (foundClip == null)
        {
            Debug.LogWarning($"[AnimationOptimizer] No AnimationClip found inside {fbxPath}");
            return null;
        }

        AnimationClip copy = Object.Instantiate(foundClip);
        copy.name = targetClipName;
        AssetDatabase.CreateAsset(copy, targetAnimPath);
        EditorUtility.SetDirty(copy);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AnimationOptimizer] Extracted clip '{foundClip.name}' to '{targetAnimPath}'.");
        return copy;
    }
}
