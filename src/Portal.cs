using System.Linq;

namespace HffArchipelagoClient
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using HumanAPI;
    using TMPro;

    public class Portal : LevelObject
    {
        LevelSource destination;
        Renderer portalRenderer;
        bool hasTriggered = false;

        private static TMP_FontAsset font;
        private static Material fontMaterial;

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Player")
                return;

            if (destination.IsUnlocked() && !hasTriggered)
            {
                if (destination.levelData.levelType == WorkshopItemSource.NotSpecified &&
                    destination.levelData.workshopId == ulong.MaxValue - 1)
                    HubWorld.LoadHubWorld();
                else
                    LoadingTools.LoadLevel(destination.levelData);

                hasTriggered = true;
            }
        }

        public void OnUnlock(bool IsUnlocked)
        {
            portalRenderer.material.SetColor("_Color", IsUnlocked ? Color.green : Color.red);
        }

        public void OnDestroy()
        {
            destination.UnregisterCallback(OnUnlock);
        }

        static Portal()
        {
            font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .Where(font => font.name == "Menu SDF").First();
            fontMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .Where(material => material.name == "Menu SDF Material").First();
        }

        public static GameObject CreatePortal(Transform parent, Vector3 position, Vector3 rotation, LevelSource destination)
        {
            GameObject portalParent = new GameObject($"Portal to {destination.levelData.title}");
            portalParent.transform.SetParent(parent);
            portalParent.transform.localPosition = position;
            portalParent.transform.localEulerAngles = rotation;
            portalParent.transform.localScale = Vector3.one;

            GameObject portalBody = new GameObject("PortalBody", typeof(BoxCollider));
            portalBody.transform.SetParent(portalParent.transform);
            portalBody.transform.localPosition = new Vector3(0.0f, 0.9f, 0.0f);
            portalBody.transform.localRotation = Quaternion.identity;
            portalBody.transform.localScale = Vector3.one;
            portalBody.GetComponent<BoxCollider>().center = Vector3.zero;
            portalBody.GetComponent<BoxCollider>().size = new Vector3(1.0f, 1.8f, 1.0f);
            portalBody.GetComponent<BoxCollider>().isTrigger = true;
            Portal portalComponent = portalBody.AddComponent<Portal>();
            portalComponent.destination = destination;

            GameObject portalSpawn = new GameObject("PortalSpawnpoint");
            portalSpawn.transform.SetParent(portalParent.transform);
            portalSpawn.transform.localPosition = new Vector3(0.0f, 0.0f, -3.0f);
            portalSpawn.transform.rotation = Quaternion.identity;
            portalSpawn.transform.localScale = Vector3.one;

            // NOTE: Everything after here is temporary

            GameObject portalRendererObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portalRendererObj.name = "PortalRenderer";
            portalRendererObj.transform.SetParent(portalParent.transform);
            portalRendererObj.transform.localPosition = new Vector3(0.0f, 0.9f, 0.0f);
            portalRendererObj.transform.localRotation = Quaternion.identity;
            portalRendererObj.transform.localScale = new Vector3(1.0f, 1.8f, 1.0f);
            portalRendererObj.GetComponent<Collider>().enabled = false;

            portalComponent.portalRenderer = portalRendererObj.GetComponent<Renderer>();
            StandardShaderUtils.ChangeRenderMode(portalComponent.portalRenderer.material, StandardShaderUtils.BlendMode.Opaque);
            portalComponent.portalRenderer.material.SetColor("_Color", destination.IsUnlocked() ? Color.green : Color.red);
            portalComponent.portalRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            portalComponent.portalRenderer.receiveShadows = false;

            destination.RegisterCallback(portalComponent.OnUnlock);

            GameObject portalText = new GameObject("TextMeshPro Text", typeof(RectTransform),
                                                                       typeof(MeshRenderer),
                                                                       typeof(TextMeshPro));

            portalText.transform.SetParent(portalParent.transform);
            portalText.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            portalText.transform.localScale = Vector3.one;
            portalText.transform.localPosition = new Vector3(0.0f, 2.5f, 0.0f);

            TextMeshPro textContent = portalText.GetComponent<TextMeshPro>();
            textContent.color = Color.black;
            textContent.fontSizeMin = 10;
            textContent.fontSize = 10;
            textContent.fontSizeMax = 10;
            textContent.font = font;
            textContent.fontMaterial = fontMaterial;
            textContent.enableWordWrapping = false;
            textContent.enableAutoSizing = true;
            textContent.enableKerning = false;
            textContent.alignment = TextAlignmentOptions.Center;
            textContent.text = destination.levelData.title;

            return portalParent;
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Vector3? position = null;
            Vector3? rotation = null;

            switch (scene.path)
            {
                // Main levels
                case "Assets/Scenes/Levels/Intro.unity":
                    position = new Vector3(-15.0f, 0.0f, -4.5f);
                    rotation = new Vector3(0.0f, 270.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Push.unity":
                    position = new Vector3(-11.25f, -0.3f, -8.5f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Carry.unity":
                    position = new Vector3(-12.0f, 0.0f, -4.0f);
                    rotation = new Vector3(0.0f, 270.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Climb.unity":
                    position = new Vector3(-12.0f, 0.0f, -15.0f);
                    rotation = new Vector3(0.0f, 240.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Break.unity":
                    position = new Vector3(9.0f, 3.0f, -4.5f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Siege.unity":
                    position = new Vector3(94.9f, -10.0f, -42.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/River.unity":
                    position = new Vector3(-97.9f, 0.95f, 320.45f);
                    rotation = new Vector3(5.0f, 230.0f, 354.0f);
					break;
                case "Assets/Scenes/Levels/Power.unity":
                    position = new Vector3(-57.0f, 0.0f, -73.0f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Aztec.unity":
                    position = new Vector3(66.1f, -0.9f, -60.3f);
                    rotation = new Vector3(0.0f, 278.0f, 0.0f);
					break;
                case "Assets/Scenes/Levels/Halloween.unity":
                    position = new Vector3(-0.8f, 14.04f, 40.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/SteamExperimental/Steam_merged.unity":
                    position = new Vector3(-35.3064f, 0.28f, -42.5f);
                    rotation = new Vector3(0.0f, 0.0f, 357.2f);
					break;
                case "Assets/Scenes/Experiments/IceExperimental/Ice_merged.unity":
                    position = new Vector3(58.0f, 24.61f, -98.0f);
                    rotation = new Vector3(0.4f, 283.0f, 355.5f);
					break;

                // Extra Dreams
                case "Assets/ContestLevels/ThermalAssets/Thermal.unity":
                    position = new Vector3(-7.1f, 1.21f, -3.3f);
                    rotation = new Vector3(0.0f, 240.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/FactoryAssets/Factory.unity":
                    position = new Vector3(11.07f, 6.238f, -4.3f);
                    rotation = new Vector3(0.0f, 90.0f, 359.3f);
                    break;
                case "Assets/ContestLevels/GolfAssets/Golf.unity":
                    position = new Vector3(-19.0f, 10.52f, -16.2f);
                    rotation = new Vector3(0.0f, 245.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/CityAssets/City.unity":
                    position = new Vector3(-128.0f, 56.95f, -26.25f);
                    rotation = new Vector3(0.0f, 270.0f, 0.0f);
					break;
                case "Assets/ContestLevels/ForestAssets/Forest.unity":
                    position = new Vector3(26.8f, 5.3f, -34.4f);
                    rotation = new Vector3(0.0f, 170.0f, 0.0f);
					break;
                case "Assets/ContestLevels/LabAssets/Lab.unity":
                    position = new Vector3(-192.54f, 1.5f, -88.7f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
					break;
                case "Assets/ContestLevels/LumberAssets/Lumber.unity":
                    position = new Vector3(6.0f, 0.0f, 7.5f);
                    rotation = new Vector3(0.0f, 90.0f, 0.0f);
					break;
                case "Assets/ContestLevels/RedRockAssets/RedRock.unity":
                    position = new Vector3(-40.35f, -2.795f, -38.43f);
                    rotation = new Vector3(0.0f, 270.0f, 0.0f);
					break;
                case "Assets/ContestLevels/TowerAssets/Tower.unity":
                    position = new Vector3(80.16f, 43.5194f, 53.45f);
                    rotation = new Vector3(0.0f, 167.0f, 0.0f);
					break;
                case "Assets/ContestLevels/MiniatureAssets/Miniature.unity":
                    position = new Vector3(-8.5f, 0.0f, -9.2f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
					break;
                case "Assets/ContestLevels/CopperWorldAssets/CopperWorld.unity":
                    position = new Vector3(-5.95f, 0.0f, -6.75f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/NavalAssests/Naval_Ben.unity":
                    position = new Vector3(65.7f, 2.6427f, -5.6f);
                    rotation = new Vector3(0.0f, 259.0f, 0.0f);
					break;
                case "Assets/ContestLevels/UnderwaterAssets/OceanAdventure.unity":
                    position = new Vector3(-4.87f, 15.23f, 28.47f);
                    rotation = new Vector3(351.0f, 69.6124f, 350.681f);
					break;
                case "Assets/ContestLevels/DockyardAssets/Dockyard.unity":
                    position = new Vector3(-63.545f, -4.2234f, -43.23f);
                    rotation = new Vector3(0.0f, 180.0f, 0.0f);
					break;
                case "Assets/ContestLevels/MuseumAssets/Museum.unity":
                    position = new Vector3(38.3f, 0.85f, 27.5f);
                    rotation = new Vector3(0.0f, 90.0f, 0.0f);
					break;
                case "Assets/ContestLevels/HikeAssets/Scenes/Hike.unity":
                    position = new Vector3(-110.77f, -35.025f, -128.93f);
                    rotation = new Vector3(0.0f, 331.4221f, 0.0f);
					break;
                case "Assets/ContestLevels/CandylandAssets/Candyland.unity":
                    position = new Vector3(-279.5f, 52.96f, -158.92f);
                    rotation = new Vector3(359.0f, 353.0f, 6.0f);
					break;
                case "Assets/ContestLevels/FacilityAssets/Facility.unity":
                    position = new Vector3(-62.75f, 31.0f, 67.25f);
                    rotation = new Vector3(0.0f, 235.0f, 0.0f);
					break;
                case "Assets/ContestLevels/Punk/SteamPunk.unity":
                    position = new Vector3(-62.78f, -4.585f, -42.0f);
                    rotation = new Vector3(0.0f, 315.0f, 0.0f);
					break;
                case "Assets/ContestLevels/VikingAssets/Viking.unity":
                    position = new Vector3(-75.66f, 0.53f, -32.46f);
                    rotation = new Vector3(0.0f, 215.0f, 2.0f);
                    break;

                // Lobbies
                case "Assets/WorkShop/Scenes/Levels/WorkshopLobby.unity":
                    position = new Vector3(-17.0f, -4.425f, -13.0f);
                    rotation = new Vector3(0.0f, 345.0f, 0.0f);
					break;
                case "Assets/Scenes/Lobby.unity":
                    position = new Vector3(14.5f, 3.0f, 49.5f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Special/Xmas.unity":
                    position = new Vector3(2.3f, 0.08f, 7.45f);
                    rotation = new Vector3(0.0f, 328.0f, 0.0f);
					break;
                case "Assets/Scenes/Lobbies/Zodiac.unity":
                    position = new Vector3(-1.8f, -36.0f, 16.8f);
                    rotation = new Vector3(0.0f, 270.0f, 0.0f);
					break;
                default:
                    break;
            }

            if (position.HasValue && rotation.HasValue)
                CreatePortal(FindObjectOfType<Level>().gameObject.transform,
                             position.Value,
                             rotation.Value,
                             HubWorld.hubLevelSource);
        }
    }
}
