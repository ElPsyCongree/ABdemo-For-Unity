using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject go = AssetBundleManager.LoadAssetBundle("test.hd", "Cube") as GameObject;
        if (go != null) {
            Instantiate(go);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
