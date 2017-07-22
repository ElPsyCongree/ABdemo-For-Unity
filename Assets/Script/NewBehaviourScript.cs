using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    GameObject g;
    GameObject go;
    float time = 0;

    // Use this for initialization
    void Start() {
        g = AssetBundleManager.LoadAssetBundle("test.hd", "Cube") as GameObject;
        go = Instantiate(g);

        for (int i = 0, imax = 1000; i < imax; i++) {
            Instantiate(g);
        }
        //Instantiate(g);
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
    }

    [ContextMenu("click")]
    private void click() {
        Material m = go.GetComponent<MeshRenderer>().sharedMaterial;
        Resources.UnloadAsset(m.mainTexture);
    }

    [ContextMenu("resume")]
    private void resume() {
        go.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = AssetBundleManager.LoadAssetBundle("test-t.hd", "faLanCheng") as Texture;
    }
}
