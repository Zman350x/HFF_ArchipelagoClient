using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace HffArchipelagoClient
{
    using HarmonyLib;
    using HumanAPI;

    public static class LoadingTools
    {
        public const string emptySceneName = "Assets/Scenes/Empty.unity";

        public static void LoadLevel(WorkshopLevelMetadata levelData)
        {
            if (Game.currentLevel != null)
                Game.currentLevel.gameObject.SetActive(false);
            Multiplayer.App.instance.LaunchSinglePlayer(levelData.workshopId, levelData.levelType, 0, 0);
        }

        public static void Patch()
        {
            Harmony.CreateAndPatchAll(typeof(LoadingTools), "LoadingTools");
        }

        public static void Unpatch()
        {
            Harmony.UnpatchID("LoadingTools");
        }

        [HarmonyPatch(typeof(Game), "LoadLevel", MethodType.Enumerator)]
        [HarmonyTranspiler]
        [HarmonyEmitIL("./dumps")]
        private static IEnumerable<CodeInstruction> GameLoadLevelMoveNext(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Get access to the sceneName local variable
            FieldInfo sceneNameField = AccessTools.GetDeclaredFields(originalMethod.DeclaringType).Single(field => field.Name.Contains("<sceneName>"));
            FieldInfo levelNumberField = AccessTools.GetDeclaredFields(originalMethod.DeclaringType).Single(field => field.Name.Contains("levelNumber"));
            FieldInfo levelTypeField = AccessTools.GetDeclaredFields(originalMethod.DeclaringType).Single(field => field.Name.Contains("levelType"));

            // Replace "!= ulong.MaxValue" with "<= 0xF000000000000000UL"
            // Normally the code adds a bundle exclusion for the main menu, which has a level number of `-1` (MaxValue when unsigned)
            // This just extends that exclusion so the hub world can have a level number of `-2`, and reserves room to add more runtime levels if needed
            codeMatcher.MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, levelNumberField),
                    new CodeMatch(OpCodes.Ldc_I4_M1)
                ).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I8, -(1L<<60)))
                .RemoveInstruction() // There is an unneeded type conversion
                .SetOpcodeAndAdvance(OpCodes.Cgt_Un);

            // Change the != to == to check when the level type is unspecified
            codeMatcher.Start().MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, levelTypeField),
                    new CodeMatch(OpCodes.Ldc_I4_S, (SByte) WorkshopItemSource.NotSpecified),
                    new CodeMatch(OpCodes.Beq)
                ).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            // Set the scene name to empty if unspecified level type
            codeMatcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldstr, emptySceneName),
                    new CodeInstruction(OpCodes.Stfld, sceneNameField)
                );

            // Find the level type switch statement
            codeMatcher.Start().MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, levelTypeField),
                    new CodeMatch(OpCodes.Switch)
                );

            // Advance to the label
            Label lobbySwitchLabel = (codeMatcher.Operand as Label[])[(int) WorkshopItemSource.BuiltInLobbies];
            while (codeMatcher.Remaining > 0)
            {
                if (codeMatcher.Labels.Contains(lobbySwitchLabel))
                    break;

                codeMatcher.Advance(1);
            }

            // Add code to load lobbies
            codeMatcher.SetAndAdvance(OpCodes.Ldarg_0, null) // Set instead of insert to keep the labels aligned
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, levelNumberField),
                    new CodeInstruction(OpCodes.Call, typeof(WorkshopRepository).GetMethod("GetLobbyFilename", new Type[] { typeof(ulong) })),
                    new CodeInstruction(OpCodes.Stfld, sceneNameField)
                );

            return codeMatcher.Instructions();
        }
    }
}
