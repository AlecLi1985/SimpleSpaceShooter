using UnityEngine;

public class LoadAssetBundles : MonoBehaviour
{
    AssetBundle loadedAssetBundle;
    public string path;
    public string assetName;

    void Start()
    {
        LoadAssetBundle(path);
        InstantiateObjectFromAssetBundle(assetName);
    }

    void LoadAssetBundle(string bundlePath)
    {
        loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);

        Debug.Log(loadedAssetBundle == null ? "Failed to load asset bundle" : "Asset Bundle loaded");
    }

    void InstantiateObjectFromAssetBundle(string name)
    {
        var prefab = loadedAssetBundle.LoadAsset(name);
        Instantiate(prefab);
    }
}
