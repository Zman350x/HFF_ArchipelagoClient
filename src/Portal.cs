using System;
using System.Collections;

namespace HffArchipelagoClient
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using HumanAPI;
    using TMPro;

    public class Portal : LevelObject
    {
        public LevelSource destination;
        public bool hasTriggered = false;

        private Renderer portalRenderer;
        private float textureAspect;

        public void LateUpdate()
        {
            Tuple<Vector2, Vector2> bounds = getScreenSpaceBounds();
            float objectAspect = (bounds.Item2.x - bounds.Item1.x)/(bounds.Item2.y - bounds.Item1.y);

            float scaleX, scaleY;
            if (textureAspect > objectAspect)
            {
                scaleX = objectAspect / textureAspect;
                scaleY = 1.0f;
            }
            else
            {
                scaleX = 1.0f;
                scaleY = textureAspect / objectAspect;
            }

            portalRenderer.material.SetFloat("_ObjectScreenMinX", bounds.Item1.x);
            portalRenderer.material.SetFloat("_ObjectScreenMinY", bounds.Item1.y);
            portalRenderer.material.SetFloat("_ObjectScreenMaxX", bounds.Item2.x);
            portalRenderer.material.SetFloat("_ObjectScreenMaxY", bounds.Item2.y);
            portalRenderer.material.SetFloat("_ScaleX", scaleX);
            portalRenderer.material.SetFloat("_ScaleY", scaleY);
        }

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
            portalRenderer.material.SetFloat("_IsUnlocked", IsUnlocked ? 1.0f : 0.0f);
        }

        public void OnDestroy()
        {
            destination.UnregisterCallback(OnUnlock);
        }

        private Tuple<Vector2, Vector2> getScreenSpaceBounds()
        {
            Vector3 c = portalRenderer.bounds.center;
            Vector3 e = portalRenderer.bounds.extents;

            Vector3[] corners = new Vector3[8]
            {
                Camera.main.WorldToViewportPoint(c + new Vector3( e.x,  e.y,  e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3(-e.x,  e.y,  e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3( e.x, -e.y,  e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3(-e.x, -e.y,  e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3( e.x,  e.y, -e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3(-e.x,  e.y, -e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3( e.x, -e.y, -e.z)),
                Camera.main.WorldToViewportPoint(c + new Vector3(-e.x, -e.y, -e.z)),
            };

            Vector3 min = corners[0];
            Vector3 max = corners[0];
            foreach (Vector3 corner in corners)
            {
                min = Vector3.Min(min, corner);
                max = Vector3.Max(max, corner);
            }

            return new Tuple<Vector2, Vector2>(min, max);
        }

        public static GameObject CreatePortal(Transform parent, Vector3 position, Vector3 rotation, LevelSource destination)
        {
            GameObject portalParent = Instantiate(ResourceManager.PortalPrefab);
            portalParent.name = $"Portal to {destination.levelData.title}";
            portalParent.transform.SetParent(parent);
            portalParent.transform.localPosition = position;
            portalParent.transform.localEulerAngles = rotation;
            portalParent.transform.localScale = Vector3.one;

            GameObject portalBase = portalParent.GetComponentInChildren<BoxCollider>().gameObject;
            Portal portalComponent = portalBase.AddComponent<Portal>();
            portalComponent.destination = destination;
            destination.RegisterCallback(portalComponent.OnUnlock);

            portalComponent.portalRenderer = portalBase.GetComponent<Renderer>();
            if (destination.levelData.thumbnailTexture != null)
            {
                portalComponent.portalRenderer.material.SetTexture(Shader.PropertyToID("_MainTex"), destination.levelData.thumbnailTexture);
                portalComponent.textureAspect = (float) destination.levelData.thumbnailTexture.width / (float) destination.levelData.thumbnailTexture.height;
            }
            portalComponent.portalRenderer.material.SetFloat("_IsUnlocked", destination.IsUnlocked() ? 1.0f : 0.0f);
            portalComponent.portalRenderer.material.color = Color.white;
            portalComponent.portalRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            portalComponent.portalRenderer.receiveShadows = false;
            portalComponent.portalRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject portalText = portalParent.GetComponentInChildren<RectTransform>().gameObject;
            TextMeshPro textContent = portalText.AddComponent<TextMeshPro>();
            ArchipelagoClient.Instance.StartCoroutine(SetPortalText(textContent, destination.levelData.title));

            return portalParent;
        }

        private static IEnumerator SetPortalText(TextMeshPro textContent, string text)
        {
            yield return null;

            textContent.color = new Color(0.4549f, 0.4549f, 0.4549f, 1.0f);
            textContent.alignment = TextAlignmentOptions.Center;
            textContent.fontSizeMin = 2;
            textContent.fontSizeMax = 4;
            textContent.font = ResourceManager.goodDogFont;
            textContent.fontMaterial = ResourceManager.goodDogFontMaterial;
            textContent.enableWordWrapping = true;
            textContent.enableAutoSizing = true;
            textContent.enableKerning = false;
            textContent.text = text;

            textContent.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1.1f, 0.47f);
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
