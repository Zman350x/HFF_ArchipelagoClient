namespace HffArchipelagoClient
{
    using BepInEx;
    using UnityEngine.SceneManagement;

    [BepInPlugin("top.zman350x.hff.archipelagoclient", "Human: Fall Flat Archipelago Client", "0.0.1")]
    [BepInProcess("Human.exe")]
    public sealed class ArchipelagoClient : BaseUnityPlugin
    {
        public static ArchipelagoClient instance;

        private void Awake()
        {
            instance = this;
            SceneManager.sceneLoaded += Barrier.OnSceneLoaded;

            Shell.RegisterCommand("ls", (string x) => {
                for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = SceneManager.GetSceneByBuildIndex(i).name;
                    Shell.print($"{scenePath}: {sceneName}");
                }
            });
            Shell.RegisterCommand("limit", (string x) => {
                InputLimiter.Patch();
            });
            Shell.RegisterCommand("unlimit", (string x) => {
                InputLimiter.Unpatch();
            });
        }

        private void Start()
        {
        }
    }
}
