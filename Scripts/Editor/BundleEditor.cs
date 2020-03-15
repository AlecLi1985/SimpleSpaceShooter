using UnityEngine;
using UnityEditor;

public class BundleEditor : Editor
{
    [MenuItem("Assets/Build Asset Bundles")]

    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles(@"E:\Projects\Unity\Simple Space Shooter - URP\AssetBundles", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
    }

}
