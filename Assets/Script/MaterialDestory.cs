﻿/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________

                我们的未来没有BUG              
* ==============================================================================
* Filename: MaterialDestory.cs
* Created:  2017/7/22 18:09:17
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTick {
    const string resPath = "Atlases/SciFi";

    private Texture m_t;
    private UIAtlas m_atlas;

    public UIAtlas atlas {
        get {
            return m_atlas;
        }
        set {
            if (m_atlas != null) {
                throw new System.Exception("don't reset value");
            }
            m_t = value.spriteMaterial.mainTexture;
            m_atlas = value;
            m_tick = 1;
        }
    }
    public int tick {
        get {
            return m_tick;
        }
    }
    public string path;
    private string m_name;
    private int m_tick;

    public MaterialTick(UIAtlas atlas) {
        this.atlas = atlas;
        m_name = m_t.name;
        path = resPath + "/" + m_name;
        m_tick = 1;
    }

    public void Remain() {
        if (m_tick == 0) {
            if (m_t != null) throw new Exception("m_t must be null");
            //TODO if you use ab change to load by ab
            m_t = Resources.Load<Texture>(path);
            m_atlas.spriteMaterial.mainTexture = m_t;
        }
        ++m_tick;
    }
    public void Release() {
        --m_tick;
        if (m_tick <= 0) {
            m_tick = 0;
            if (m_t != null) {
                Resources.UnloadAsset(m_t);
                m_t = null;
            }
        }
    }
}

public class MaterialDestory : MonoBehaviour {

    private static
    Dictionary<string, MaterialTick> m_TextureSet = new Dictionary<string, MaterialTick>();


    private void Start() {
        StartCoroutine(CollectMaterial());
    }

    private  IEnumerator CollectMaterial() {
        yield return null;
        yield return null;

        UIPanel panel = GetComponent<UIPanel>();
        List<UIWidget> widgets = panel.widgets;
        foreach (var w in widgets) {
            if (!(w is UISprite)) continue;
            UISprite sp = w as UISprite;

            UIAtlas atlas = sp.atlas;
            string name = atlas.name;

            MaterialTick mt = null;
            if (m_TextureSet.TryGetValue(name, out mt)) {
                if (mt.tick <= 0) {
                    mt.atlas = atlas;
                }
            }
            else {
                m_TextureSet.Add(name, new MaterialTick(atlas));
            }

        }
    }

    [ContextMenu("destory")]
    private void destory() {
        //gameObject.SetActive(false);
        foreach (var m in m_TextureSet) {
            m.Value.Release();
        }
    }

    [ContextMenu("resume")]
    private void resume() {
        foreach (var m in m_TextureSet) {
            m.Value.Remain();
        }

        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }


}
