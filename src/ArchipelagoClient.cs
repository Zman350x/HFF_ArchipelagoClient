namespace HffArchipelagoClient
{
    using BepInEx;

    [BepInPlugin("top.zman350x.hff.archipelagoclient", "Human: Fall Flat Archipelago Client", "0.0.1")]
    [BepInProcess("Human.exe")]
    public sealed class ArchipelagoClient : BaseUnityPlugin
    {
        public static ArchipelagoClient instance;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
        }

        private void Start()
        {
        }
    }
}
