using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Builds a single multi-state AnimatorController for Female Zombie greybox AI.
    /// The vendor pack ships one controller per clip, which breaks chase/melee blending.
    /// </summary>
    [InitializeOnLoad]
    public static class PzFemaleZombieAnimatorBuilder
    {
        private const string OutputDir = "Assets/_ProjectZone/Art/Animators";
        private const string OutputPath = OutputDir + "/PZ_FemaleZombie.controller";

        private const string IdleFbx = "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Idle01.FBX";
        private const string WalkFbx = "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Walk01Forward.FBX";
        private const string RunFbx = "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Run01ForwardInPlace.FBX";
        private const string AttackFbx = "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_AttackInPlace01.FBX";
        private const string DeathFbx = "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Death01.FBX";

        static PzFemaleZombieAnimatorBuilder()
        {
            EditorApplication.delayCall += EnsureControllerExists;
        }

        private static void EnsureControllerExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OutputPath) != null)
                return;

            Build();
        }

        [MenuItem("Project Zone/Build Female Zombie Animator")]
        public static void BuildFromMenu()
        {
            string path = Build();
            EditorUtility.DisplayDialog(
                "Female Zombie Animator",
                string.IsNullOrEmpty(path)
                    ? "Build failed — check the Console."
                    : $"Built:\n{path}",
                "OK");
        }

        /// <summary>Batchmode entry: Unity -executeMethod ProjectZone.Editor.PzFemaleZombieAnimatorBuilder.Build</summary>
        public static string Build()
        {
            AnimationClip idle = LoadClip(IdleFbx, "ZombieFemale_Idle01");
            AnimationClip walk = LoadClip(WalkFbx, "ZombieFemale_Walk01Forward");
            AnimationClip run = LoadClip(RunFbx, "ZombieFemale_Run01ForwardInPlace");
            AnimationClip attack = LoadClip(AttackFbx, "ZombieFemale_AttackInPlace01");
            AnimationClip death = LoadClip(DeathFbx, "ZombieFemale_Death01");

            if (idle == null || walk == null || run == null || attack == null || death == null)
            {
                Debug.LogError(
                    "[ProjectZone] Female Zombie animator build failed — missing animation clips. " +
                    $"idle={idle != null}, walk={walk != null}, run={run != null}, attack={attack != null}, death={death != null}");
                return null;
            }

            if (!AssetDatabase.IsValidFolder("Assets/_ProjectZone/Art"))
                AssetDatabase.CreateFolder("Assets/_ProjectZone", "Art");
            if (!AssetDatabase.IsValidFolder(OutputDir))
                AssetDatabase.CreateFolder("Assets/_ProjectZone/Art", "Animators");

            if (File.Exists(OutputPath))
                AssetDatabase.DeleteAsset(OutputPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine root = controller.layers[0].stateMachine;

            AnimatorState idleState = root.AddState("Idle", new Vector3(300f, 0f, 0f));
            idleState.motion = idle;

            AnimatorState walkState = root.AddState("Walk", new Vector3(300f, 80f, 0f));
            walkState.motion = walk;

            AnimatorState runState = root.AddState("Run", new Vector3(300f, 160f, 0f));
            runState.motion = run;

            AnimatorState attackState = root.AddState("Attack", new Vector3(560f, 80f, 0f));
            attackState.motion = attack;

            AnimatorState deathState = root.AddState("Death", new Vector3(560f, 200f, 0f));
            deathState.motion = death;

            root.defaultState = idleState;

            // Locomotion
            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 2.6f, "Speed");
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.12f;

            AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.Less, 2.5f, "Speed");
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.12f;

            AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 2.6f, "Speed");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.12f;

            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;

            // Attack from locomotion
            AddAttackTransition(idleState, attackState);
            AddAttackTransition(walkState, attackState);
            AddAttackTransition(runState, attackState);

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.85f;
            attackToIdle.duration = 0.1f;
            attackToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");

            AnimatorStateTransition attackToWalk = attackState.AddTransition(walkState);
            attackToWalk.hasExitTime = true;
            attackToWalk.exitTime = 0.85f;
            attackToWalk.duration = 0.1f;
            attackToWalk.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");

            // Death from anywhere
            AddAnyDeathTransition(root, deathState);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ProjectZone] Built Female Zombie animator: {OutputPath}");
            return OutputPath;
        }

        private static void AddAttackTransition(AnimatorState from, AnimatorState attack)
        {
            AnimatorStateTransition t = from.AddTransition(attack);
            t.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            t.hasExitTime = false;
            t.duration = 0.05f;
        }

        private static void AddAnyDeathTransition(AnimatorStateMachine root, AnimatorState death)
        {
            AnimatorStateTransition t = root.AddAnyStateTransition(death);
            t.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
            t.hasExitTime = false;
            t.duration = 0.05f;
            t.canTransitionToSelf = false;
        }

        private static AnimationClip LoadClip(string fbxPath, string preferredName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            AnimationClip fallback = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not AnimationClip clip)
                    continue;
                if (clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    continue;
                if (clip.name == preferredName)
                    return clip;
                fallback ??= clip;
            }

            return fallback;
        }
    }
}
