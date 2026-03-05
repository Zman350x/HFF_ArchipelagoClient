using System;

namespace HffArchipelagoClient
{
    using BepInEx;
    using HarmonyLib;
    using UnityEngine.SceneManagement;

    [BepInPlugin("top.zman350x.hff.archipelagoclient", "Human: Fall Flat Archipelago Client", "0.0.1")]
    [BepInProcess("Human.exe")]
    public sealed class ArchipelagoClient : BaseUnityPlugin
    {
        public static ArchipelagoClient Instance { get; private set; }
        public static bool IsActive { get; private set; } = false;

        public static CommandRegistry commands;

        internal static new BepInEx.Logging.ManualLogSource Logger;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            commands = (CommandRegistry) AccessTools.DeclaredField(typeof(Shell), "commands").GetValue(null);

            SceneManager.sceneLoaded += OnSceneLoaded;
            Harmony.CreateAndPatchAll(typeof(ArchipelagoClient), "ArchipelagoClient");
            HubWorld.Patch();
        }

        private static void OnStartup()
        {
            MenuButtonTools.AddButton("SelectPlayersMenu", "ArchipelagoButton", "ARCHIPELAGO", 3, ArchipelagoStart);
        }

        public static void ArchipelagoStart()
        {
            if (!MenuSystem.CanInvoke)
                return;

            InputLimiter.Patch();
            Shell.RegisterCommand("hub", new Action(HubWorld.LoadHubWorld), "hub\r\nGo to the Archipelago hub");
            HubWorld.LoadHubWorld();

            IsActive = true;
        }

        public static void ArchipelagoEnd()
        {
            InputLimiter.Unpatch();
            commands.UnRegisterCommand("hub", new Action(HubWorld.LoadHubWorld));

            IsActive = false;
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path == "Assets/Scenes/Startup.unity")
                OnStartup();

            if (IsActive)
            {
                Barrier.OnSceneLoaded(scene, mode);
            }
        }

        [HarmonyPatch(typeof(Multiplayer.App), "ExitGame")]
        [HarmonyPostfix]
        private static void ExitGame()
        {
            ArchipelagoEnd();
        }
    }
}
