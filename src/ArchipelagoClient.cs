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

        private void Awake()
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Harmony.CreateAndPatchAll(typeof(ArchipelagoClient), "ArchipelagoClient");
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
            IsActive = true;
            Multiplayer.App.instance.LaunchSinglePlayer(0, WorkshopItemSource.BuiltIn, 0, 0);
        }

        public static void ArchipelagoEnd()
        {
            InputLimiter.Unpatch();
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
