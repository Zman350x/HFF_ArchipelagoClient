using System;

namespace HffArchipelagoClient
{
    using UnityEngine;
    using HumanAPI;
    using UnityEngine.Events;

    public static class HubWorld
    {

        private static GameObject hubLevelObject;
        private static Level hubLevel;
        private static LevelSource hubLevelSource = new LevelSource(
            new WorkshopLevelMetadata {
                itemType = WorkshopItemType.Level,
                workshopId = ulong.MaxValue - 1,
                title = "Archipelago Hub",
                levelType = WorkshopItemSource.NotSpecified
            }, true);

        public static void LoadHubWorld()
        {
            LoadingTools.LoadLevel(hubLevelSource.levelData);
        }

        public static void OnHubWorldLoaded()
        {
            // Create and deactivate a new root game object
            hubLevelObject = new GameObject("Level");
            hubLevelObject.SetActive(false);

            // Add level component; must be done while the game object is disabled
            hubLevel = hubLevelObject.AddComponent<Level>();

            // Add things to the scene
            AddRequiredLevelObjects();
            AddLighting();
            AddGeometry();

            int portals = LevelSource.EnabledLevels.Count;

            for (int i = 0; i < portals; ++i)
            {
                double angle = (2 * Math.PI * i) / portals;
                Vector3 position = new Vector3((float) Math.Sin(angle), 0.0f, (float) Math.Cos(angle));
                Vector3 rotation = new Vector3(0.0f, (float) (Mathf.Rad2Deg * angle), 0.0f);
                Portal.CreatePortal(hubLevelObject.transform, position * 30, rotation, LevelSource.EnabledLevels[i]);
            }

            // Activate the level game object only after everything is setup
            hubLevelObject.SetActive(true);
        }

        private static void AddRequiredLevelObjects()
        {
            GameObject spawnpoint = new GameObject("Spawnpoint");
            spawnpoint.transform.SetParent(hubLevelObject.transform);
            spawnpoint.transform.localPosition = Vector3.zero;
            spawnpoint.transform.localRotation = Quaternion.identity;
            spawnpoint.transform.localScale = Vector3.one;
            spawnpoint.AddComponent<Checkpoint>();
            hubLevel.spawnPoint = spawnpoint.transform;

            GameObject fallTrigger = new GameObject("FallTrigger", typeof(BoxCollider));
            fallTrigger.transform.SetParent(hubLevelObject.transform);
            fallTrigger.transform.localPosition = new Vector3(0.0f, -30.0f, 0.0f);
            fallTrigger.transform.localRotation = Quaternion.identity;
            fallTrigger.transform.localScale = Vector3.one;
            fallTrigger.GetComponent<BoxCollider>().center = new Vector3(0.0f, -20.0f, 0.0f);
            fallTrigger.GetComponent<BoxCollider>().size = new Vector3(400.0f, 50.0f, 400.0f);
            fallTrigger.GetComponent<BoxCollider>().isTrigger = true;
            fallTrigger.AddComponent<FallTrigger>().OnFall = new UnityEvent();
        }

        private static void AddLighting()
        {
            GameObject directionalLight = new GameObject("DirectionalLight", typeof(Light));
            directionalLight.transform.SetParent(hubLevelObject.transform);
            directionalLight.transform.localPosition = new Vector3(0.0f, 3.0f, 0.0f);
            directionalLight.transform.localEulerAngles = new Vector3(50.0f, 330.0f, 0.0f);
            directionalLight.transform.localScale = Vector3.one;
            directionalLight.GetComponent<Light>().shadows = LightShadows.Soft;
            directionalLight.GetComponent<Light>().type = LightType.Directional;
        }

        private static void AddGeometry()
        {
            GameObject tempFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            tempFloor.name = "TempFloor";
            tempFloor.transform.SetParent(hubLevelObject.transform);
            Renderer tempFloorRenderer = tempFloor.GetComponent<Renderer>();
            StandardShaderUtils.ChangeRenderMode(tempFloorRenderer.material, StandardShaderUtils.BlendMode.Opaque);
            tempFloorRenderer.material.SetColor("_Color", new Color(0.0f, 0.4f, 0.0f, 1.0f));
            tempFloor.transform.localPosition = Vector3.zero;
            tempFloor.transform.localRotation = Quaternion.identity;
            tempFloor.transform.localScale = Vector3.one * 7;
            tempFloor.GetComponent<Collider>().enabled = true;
        }
    }
}
