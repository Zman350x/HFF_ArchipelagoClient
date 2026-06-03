using System;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace HffArchipelagoClient
{
    using HarmonyLib;

    public static class ArchipelagoLoadingTools
    {
        public static void Patch()
        {
            Harmony.CreateAndPatchAll(typeof(ArchipelagoLoadingTools), "ArchipelagoLoadingTools");
        }

        public static void Unpatch()
        {
            Harmony.UnpatchID("ArchipelagoLoadingTools");
        }

        [HarmonyPatch(typeof(Game), "Fall")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GameFall(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Match the "currentLevelType == EditorPick" if statement
            codeMatcher.MatchStartForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, typeof(Game).GetField("currentLevelType")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Bne_Un),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stfld, typeof(Game).GetField("passedLevel"))
                );

            // Add the additional condition that the ArchipelagoClient must be inactive
            Label label = (Label) codeMatcher.InstructionAt(3).operand;
            codeMatcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, typeof(ArchipelagoClient).GetMethod("get_IsActive", new Type[] {})),
                    new CodeInstruction(OpCodes.Brtrue, label)
                );

            return codeMatcher.Instructions();
        }

        [HarmonyPatch(typeof(Game), "PassLevel", MethodType.Enumerator)]
        [HarmonyPrefix]
        private static void GamePassLevel(ref bool __runOriginal)
        {
            // Return to hub after beating level if active
            if (ArchipelagoClient.IsActive)
            {
                // Except for Ice -> Reprise
                if (Game.instance.currentLevelType == WorkshopItemSource.BuiltIn &&
                    Game.instance.currentLevelNumber == 11)
                    return;

                Game.currentLevel.CompleteLevel();
                HubWorld.LoadHubWorld();
                StatsAndAchievements.Save();
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(GameSave), "PassCheckpointCampaign")]
        [HarmonyPatch(typeof(GameSave), "PassCheckpointEditorPick")]
        [HarmonyPatch(typeof(GameSave), "PassCheckpointWorkshop")]
        [HarmonyPrefix]
        private static void GameSavePassCheckpoint(ref bool __runOriginal)
        {
            if (ArchipelagoClient.IsActive)
                __runOriginal = false;
        }
    }
}
