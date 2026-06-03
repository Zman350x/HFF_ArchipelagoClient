using System;

namespace HffArchipelagoClient
{
    using BepInEx;
    using HarmonyLib;
    using ZmanBase;
    using UnityEngine.SceneManagement;

    [BepInPlugin("top.zman350x.hff.archipelagoclient", "Human: Fall Flat Archipelago Client", "0.1.0")]
    [BepInDependency("top.zman350x.hff.zmanbase")]
    [BepInProcess("Human.exe")]
    public sealed class ArchipelagoClient : BaseUnityPlugin
    {
        public static ArchipelagoClient Instance { get; private set; }
        public static bool IsActive { get; private set; } = false;

        internal static new BepInEx.Logging.ManualLogSource Logger;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadingTools.StartupEvent += OnStartup;
            Harmony.CreateAndPatchAll(typeof(ArchipelagoClient), "ArchipelagoClient");
            ArchipelagoLoadingTools.Patch();
        }

        private static void OnStartup()
        {
            MenuButtonTools.AddButton("SelectPlayersMenu", "ArchipelagoButton", "ARCHIPELAGO", 3, ArchipelagoStart);
        }

        public static void ArchipelagoStart()
        {
            if (!MenuSystem.CanInvoke)
                return;

            LevelSource.FetchEnabledLevels(new WorkshopItemSource[] {
                WorkshopItemSource.BuiltIn,
                WorkshopItemSource.EditorPick,
                WorkshopItemSource.BuiltInLobbies
            });
            ControlLockSource.FetchEnabledControlLocks(new ControlType[] {
                ControlType.GRAB_LEFT,
                ControlType.GRAB_RIGHT,
                ControlType.JUMP,
                ControlType.PLAY_DEAD,
                ControlType.LOOK_LEFT,
                ControlType.LOOK_RIGHT,
                ControlType.LOOK_UP,
                ControlType.LOOK_DOWN,
                ControlType.SHOOT_FIREWORKS
            });

            InputLimiter.Patch();
            Shell.RegisterCommand("hub", new Action(HubWorld.LoadHubWorld), "hub\r\nReturn to the Archipelago hub");
            MenuButtonTools.AddButton("PauseMenu", "HubButton", "RETURN TO ARCHIPELAGO HUB", 7, () => {
                    Game.instance.Resume();
                    MenuSystem.instance.HideMenus();
                    HubWorld.LoadHubWorld();
            });

            HubWorld.LoadHubWorld();
            IsActive = true;
        }

        public static void ArchipelagoEnd()
        {
            InputLimiter.Unpatch();
            ZmanBaseMod.Commands.UnRegisterCommand("hub", new Action(HubWorld.LoadHubWorld));
            MenuButtonTools.DestroyButton("PauseMenu", "HubButton");
            MenuButtonTools.EnableDisableButton("PauseMenu", "ExitButton", true);

            IsActive = false;
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsActive && !HubWorld.loadingHubWorld)
            {
                MenuButtonTools.EnableDisableButton("PauseMenu", "HubButton", true);
                MenuButtonTools.EnableDisableButton("PauseMenu", "ExitButton", false);
                Barrier.OnSceneLoaded(scene, mode);
                Portal.OnSceneLoaded(scene, mode);
            }

            HubWorld.loadingHubWorld = false;
        }

        [HarmonyPatch(typeof(Multiplayer.App), "ExitGame")]
        [HarmonyPostfix]
        private static void ExitGame()
        {
            ArchipelagoEnd();
        }
    }
}
