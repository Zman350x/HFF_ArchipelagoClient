using System.Linq;

namespace HffArchipelagoClient
{
    using UnityEngine;
    using HumanAPI;
    using TMPro;

    public class Portal : LevelObject
    {
        LevelSource destination;
        Renderer portalRenderer;
        bool hasTriggered = false;

        private static TMP_FontAsset font;
        private static Material fontMaterial;

        static Portal()
        {
            font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .Where(font => font.name == "Menu SDF").First();
            fontMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .Where(material => material.name == "Menu SDF Material").First();
        }

        public static void CreatePortal(Transform parent, Vector3 position, Vector3 rotation, LevelSource destination)
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
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Human>() == null)
                return;

            if (destination.IsUnlocked() && !hasTriggered)
            {
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
    }
}
