using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace HffArchipelagoClient
{
    using UnityEngine;
    using HumanAPI;
    using HarmonyLib;
    using UnityEngine.Events;

    public static class HubWorld
    {
        public static readonly string emptySceneName = "Assets/Scenes/Empty.unity";

        private static GameObject levelObject;
        private static Level level;

        public static void LoadHubWorld()
        {
            Multiplayer.App.instance.LaunchSinglePlayer(ulong.MaxValue - 1, WorkshopItemSource.NotSpecified, 0, 0);
        }

        public static void OnHubWorldLoaded()
        {
            // Create and deactivate a new root game object
            levelObject = new GameObject("Level");
            levelObject.SetActive(false);

            level = levelObject.AddComponent<Level>();

            GameObject spawnpoint = new GameObject("Spawnpoint");
            spawnpoint.transform.SetParent(levelObject.transform);
            spawnpoint.GetComponent<Transform>().localPosition = Vector3.zero;
            spawnpoint.AddComponent<Checkpoint>();
            level.spawnPoint = spawnpoint.transform;

            GameObject fallTrigger = new GameObject("FallTrigger", typeof(BoxCollider));
            fallTrigger.transform.SetParent(levelObject.transform);
            fallTrigger.GetComponent<Transform>().localPosition = new Vector3(0.0f, -30.0f, 0.0f);
            fallTrigger.GetComponent<BoxCollider>().center = new Vector3(0.0f, -20.0f, 0.0f);
            fallTrigger.GetComponent<BoxCollider>().size = new Vector3(400.0f, 50.0f, 400.0f);
            fallTrigger.GetComponent<BoxCollider>().isTrigger = true;
            fallTrigger.AddComponent<FallTrigger>().OnFall = new UnityEvent();

            // Activate the game object only after everything is setup
            levelObject.SetActive(true);
        }

        public static void Patch()
        {
            Harmony.CreateAndPatchAll(typeof(HubWorld), "HubWorld");
        }

        public static void Unpatch()
        {
            Harmony.UnpatchID("HubWorld");
        }

        [HarmonyPatch(typeof(Game), "LoadLevel", MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GameLoadLevelMoveNext(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Get access to the sceneName local variable
            FieldInfo sceneNameField = AccessTools.GetDeclaredFields(originalMethod.DeclaringType).Single(field => field.Name.Contains("<sceneName>"));

            // Replace "!= ulong.MaxValue" with "<= 0xF000000000000000UL"
            codeMatcher.MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_M1)
                ).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I8, -(1L<<60)))
                .RemoveInstruction()
                .SetOpcodeAndAdvance(OpCodes.Cgt_Un);

            // Change the != to == to check when the level type is unspecified
            codeMatcher.Start().MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_S, (SByte) WorkshopItemSource.NotSpecified),
                    new CodeMatch(OpCodes.Beq)
                ).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            // Set the scene name to empty if unspecified level type
            codeMatcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldstr, emptySceneName),
                    new CodeInstruction(OpCodes.Stfld, sceneNameField)
                );

            return codeMatcher.Instructions();
        }
    }
}
