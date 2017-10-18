using AssetBundleTool;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    GameObject g;
    GameObject go1;
    GameObject go2;
    float time = 0;

    // Use this for initialization
    void Start() {
        //g = AssetBundleManager.LoadAssetBundle("test.hd", "Cube") as GameObject;
        //go1 = Instantiate(g);

        //g = AssetBundleManager.LoadAssetBundle("test.hd", "Cube1") as GameObject;
        //go2 = Instantiate(g);

        StartCoroutine(load());
        StartCoroutine(load());
    }

    Dictionary<string, AssetBundleCreateRequest> m_dict = new Dictionary<string, AssetBundleCreateRequest>();
    IEnumerator load() {

        string path = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
        path = Path.Combine(path, "test.hd");

        AssetBundle ab;
        var t = LoadFromFileAsync(path);
        while (!t.isDone) {
            yield return null;
        }
        ab = t.assetBundle;
        print("name：" + ab.GetInstanceID());


        StartCoroutine(load2());
    }

    IEnumerator load2() {
        string path = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
        path = Path.Combine(path, "test.hd");

        AssetBundle ab;
        var t = LoadFromFileAsync(path);
        while (!t.isDone) {
            yield return null;
        }
        ab = t.assetBundle;
        print("name：" + ab.GetInstanceID());
    }

    AssetBundleCreateRequest LoadFromFileAsync(string path) {
        AssetBundleCreateRequest ret = null;
        if (!m_dict.TryGetValue(path, out ret)) {
            ret = AssetBundle.LoadFromFileAsync(path);
            m_dict.Add(path, ret);
        }
        return ret;
    }

    // Update is called once per frame
    void Update() {
        float ft = Time.fixedTime;
        if (ft - time < 5) {
            return;
        }
        time = ft;
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();

        int size = 0;
        foreach (Texture t in textures) {
            if (t.hideFlags == HideFlags.None) {
                Debug.Log(t.name);
                size += Profiler.GetRuntimeMemorySize(t);
            }
        }
        Debug.Log(" using: " + (size >> 10) + "kb");
        Resources.UnloadUnusedAssets();
    }

    [ContextMenu("click")]
    private void click() {
        Material m = go1.GetComponent<MeshRenderer>().sharedMaterial;
        Resources.UnloadAsset(m.mainTexture);
    }

    [ContextMenu("resume")]
    private void resume() {

        //go1.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = AssetBundleManager.LoadAssetBundle("test-t.hd", "SciFi Atlas") as Texture;
    }
}
