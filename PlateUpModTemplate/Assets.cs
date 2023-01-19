using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlateUpModTemplate
{
    public static class Assets
    {
        private static readonly string[] KnownExtensions = { "png", "exe", "txt", "xcf", "bat" };
        private static readonly List<AssetBundle> AssetBundles = new List<AssetBundle>();
        private static readonly Dictionary<string, int> AssetIndices = new Dictionary<string, int>();
        private static readonly List<string> SoundBanksToLoad = new List<string>();
        private static readonly List<string> FoundSoundBanks = new List<string>();
        private static readonly Dictionary<string, string> DisplayRuleSetOverrides = new Dictionary<string, string>();

        internal static void PopulateAssets()
        {
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            foreach (var resource in resourceNames)
            {
                ResourceType resourceType = GetResourceType(resource);

                switch (resourceType)
                {
                    case ResourceType.AssetBundle:
                        // DebugClass.Log($"Loading AssetBundle {resource}");
                        LoadAssetBundle(resource);
                        break;
                    case ResourceType.SoundBank:
                        FoundSoundBanks.Add(resource);
                        break;
                    case ResourceType.DisplayRuleSetOverride:
                        DisplayRuleSetOverrides.Add(GetFileName(resource).Split('.')[0], resource);
                        break;
                    case ResourceType.Other:
                        // DebugClass.Log($"Loading Other {resource}");
                        // The majority of this stuff is manually loaded as needed.
                        break;
                }
            }
        }

        private static string GetFileName(string resource)
        {
            string[] split = resource.Split('.');

            return split[split.Length - 2] + "." + split[split.Length - 1];
        }

        internal static void AddSoundBank(string name)
        {
            SoundBanksToLoad.Add($"{name}");
        }

        private static ResourceType GetResourceType(string resourceName)
        {
            string[] split = resourceName.Split('.');

            if (split.Length <= 0)
                throw new Exception($"Invalid asset found: {resourceName}");

            string lastItem = split[split.Length - 1];

            if (lastItem == "bnk")
                return ResourceType.SoundBank;

            if (lastItem == "drso")
                return ResourceType.DisplayRuleSetOverride;

            if (Array.IndexOf(KnownExtensions, lastItem) >= 0)
                return ResourceType.Other;

            return ResourceType.AssetBundle;
        }

        private enum ResourceType
        {
            AssetBundle,
            SoundBank,
            DisplayRuleSetOverride,
            Other
        }

        private static void LoadAssetBundle(string location)
        {
            using var assetBundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(location);
            AssetBundle assetBundle = AssetBundle.LoadFromStream(assetBundleStream);

            int index = AssetBundles.Count;
            AssetBundles.Add(assetBundle);

            foreach (var assetName in assetBundle.GetAllAssetNames())
            {
                string path = assetName.ToLower();
                if (path.StartsWith("assets/"))
                    path = path.Remove(0, "assets/".Length);
                AssetIndices[path] = index;
            }

        }


        [Obsolete("AssetBundles are loaded automatically, calling this does literally nothing")]
        public static void AddBundle(string assetBundleLocation)
        {
            // Empty method because I don't want to go and remove stuff right now.
        }

        public static T Load<T>(string assetName) where T : UnityEngine.Object
        {
            if (assetName.Contains(":"))
            {
                string[] path = assetName.Split(':');

                assetName = path[1].ToLower();
            }
            if (assetName.StartsWith("assets/"))
                assetName = assetName.Remove(0, "assets/".Length);
            int index = AssetIndices[assetName];

            T asset = AssetBundles[index].LoadAsset<T>($"assets/{assetName}");

            if (asset is Material material)
            {
                //if (material.shader.name.StartsWith("MoistToolkit/StubbedShader"))
                    //material.shader = Addressables.LoadAssetAsync<Shader>($"RoR2/Base/Shaders/{material.shader.name.Substring(27)}.shader").WaitForCompletion();
            }

            return asset;
        }

        public static Stream LoadDisplayRuleSetOverride(string overrideName)
        {
            string path = DisplayRuleSetOverrides[overrideName];

            return Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        }
    }
}
