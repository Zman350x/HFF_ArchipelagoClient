using System.Linq;
using System.Reflection;
using System.IO;

namespace HffArchipelagoClient
{
    using UnityEngine;

    public static class ArchipelagoResourceManager
    {
        public static GameObject PortalPrefab { get; private set; }
        public static ComputeShader BoundsCompute { get; private set; }

        public static Material portalGoodDogFontMaterial;

        public static Texture2D HubWorldThumbnail;
        public static Texture2D LockTexture;

        static ArchipelagoResourceManager()
        {
            // Load AssetBundle
            string bundleName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("archipelago"));
            Stream bundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(bundleName);
            AssetBundle bundle = AssetBundle.LoadFromStream(bundleStream);

            // AssetBundle Assets
            bundle.LoadAsset<Shader>("assets/textmesh pro/required/shaders/tmp_sdf-surface.shader");
            bundle.LoadAsset<Shader>("assets/shaders/screenspacecover.shader");
            BoundsCompute = bundle.LoadAsset<ComputeShader>("assets/shaders/computebounds.compute");
            portalGoodDogFontMaterial = bundle.LoadAsset<Material>("assets/fonts/gooddog sdf.mat");
            PortalPrefab = bundle.LoadAsset<GameObject>("assets/portal.prefab");

            // Load Hub World Thumbnail
            string hubWorldThumbnailName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("hub_world_thumbnail.png"));
            Stream hubWorldThumbnailStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(hubWorldThumbnailName);
            byte[] hubWorldThumbnailBytes = new byte[hubWorldThumbnailStream.Length];
            hubWorldThumbnailStream.Read(hubWorldThumbnailBytes, 0, hubWorldThumbnailBytes.Length);
            HubWorldThumbnail = new Texture2D(1, 1);
            HubWorldThumbnail.LoadImage(hubWorldThumbnailBytes);
            HubWorldThumbnail.name = "HubWorldThumbnail";

            // Load Hub Lock Texture
            string lockTextureName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("lock.png"));
            Stream lockTextureStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(lockTextureName);
            byte[] lockTextureBytes = new byte[lockTextureStream.Length];
            lockTextureStream.Read(lockTextureBytes, 0, lockTextureBytes.Length);
            LockTexture = new Texture2D(1, 1);
            LockTexture.LoadImage(lockTextureBytes);
            LockTexture.name = "LockTexture";
        }
    }
}
