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
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/FactoryAssets/Factory.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/GolfAssets/Golf.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/CityAssets/City.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/ForestAssets/Forest.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/LabAssets/Lab.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/LumberAssets/Lumber.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/RedRockAssets/RedRock.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/TowerAssets/Tower.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/MiniatureAssets/Miniature.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/CopperWorldAssets/CopperWorld.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case "Assets/ContestLevels/NavalAssests/Naval_Ben.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/UnderwaterAssets/OceanAdventure.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/DockyardAssets/Dockyard.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/MuseumAssets/Museum.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/HikeAssets/Scenes/Hike.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/CandylandAssets/Candyland.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/FacilityAssets/Facility.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/Punk/SteamPunk.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/ContestLevels/VikingAssets/Viking.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
                    break;

                // Lobbies
                case "Assets/WorkShop/Scenes/Levels/WorkshopLobby.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Lobby.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Special/Xmas.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
					break;
                case "Assets/Scenes/Lobbies/Zodiac.unity":
                    position = new Vector3(0.0f, 0.0f, 0.0f);
                    rotation = new Vector3(0.0f, 0.0f, 0.0f);
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
