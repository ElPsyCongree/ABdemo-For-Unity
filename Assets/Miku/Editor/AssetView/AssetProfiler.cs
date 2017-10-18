/*
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
* Filename: AssetProfiler.cs
* Created:  2017/7/25 10:26:52
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Miku.Profiler {
    public class AssetProfiler : EditorWindow {

        #region class
        public class SampleObject {
            public string name;
            public int size;
            public UnityEngine.Object obj;
            public bool isInScene = false;
            public List<GameObject> gameObjects = new List<GameObject>();
            public SampleObject(UnityEngine.Object obj) {
                name = obj.name;
                size = AssetProfiler.GetObjectSize(obj);
                this.obj = obj;
                isInScene = AssetProfiler.IsObjectInScene(obj);
            }
        }

        public class DrawDatas {
            public List<SampleObject> objects;
            public int totalSize = 0;
            public Vector2 scrollPosition;
            public DrawDatas() {
                objects = new List<SampleObject>();
            }
        }
        #endregion

        #region member
        private Vector2 scrollPosition;
        private static Dictionary<Object, SampleObject> m_objectMap = new Dictionary<Object, SampleObject>();
        private static HashSet<Object> m_scenesObjects;
        Dictionary<System.Type, DrawDatas> SampleDataDict = new Dictionary<System.Type, DrawDatas> {
            { typeof(Texture), new DrawDatas()},
            { typeof(Mesh), new DrawDatas()},
            { typeof(Material), new DrawDatas()},
            { typeof(AnimationClip), new DrawDatas()},
            { typeof(AudioClip), new DrawDatas()},
        };
        #endregion

        [MenuItem("Miku/AssetProfiler")]
        static void Create() {
            EditorWindow.GetWindow<AssetProfiler>();
        }

        private void OnGUI() {
            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(position.width));

            if (GUILayout.Button("Take Sample")) {
                GetInSceneAssets();
                GetUsingObjects();
            }

            foreach (var dt in SampleDataDict) {
                DrawTextureSample(dt.Key, dt.Value);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void GetUsingObjects() {
            #region clear
            m_objectMap.Clear();
            foreach (var dt in SampleDataDict) {
                dt.Value.objects.Clear();
                dt.Value.totalSize = 0;
                dt.Value.scrollPosition = Vector2.zero;
            }
            System.GC.Collect();
            //Resources.UnloadUnusedAssets();
            EditorUtility.UnloadUnusedAssetsImmediate();
            #endregion

            #region get using gameObject
            List<GameObject> list = new List<GameObject>();
            GameObject[] gos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in gos) {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                    continue;
                if ((go.hideFlags & HideFlags.DontSaveInBuild) == HideFlags.DontSaveInBuild) {
                    continue;
                }
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;
                list.Add(go);
            }
            #endregion

            foreach (var go in list) {
                if (go.transform.parent != null) continue;
                Object[] des = EditorUtility.CollectDependencies(new Object[] { go });
                foreach (var dt in SampleDataDict) {
                    TakeSamples(des, dt.Key, dt.Value, go);
                }
            }

            foreach (var dt in SampleDataDict) {
                dt.Value.objects.Sort((a, b) => {
                    int aIn = a.isInScene ? 1 : 0;
                    int bIn = b.isInScene ? 1 : 0;
                    int result = 0;
                    if (aIn != bIn) {
                        result = aIn - bIn;
                    }
                    else {
                        result = b.size - a.size;
                    }
                    return result;
                });
            }
        }

        private void GetInSceneAssets() {
            GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            Object[] des = EditorUtility.CollectDependencies(gos);

            m_scenesObjects = new HashSet<Object>(des);
        }
        public static bool IsObjectInScene(Object o) {
            return m_scenesObjects.Contains(o);
        }

        #region take sample
        public static int GetObjectSize(UnityEngine.Object o) {
#if UNITY_5_5_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(o);
#else
            return UnityEngine.Profiler.GetRuntimeMemorySize(o);
#endif
        }
        private string GetShowSize(long size) {
            string result = "";
            int size2 = 1024;
            int size22 = 1024 * 1024;
            int size222 = 1024 * 1024 * 1024;

            if (size / size222 > 0) {
                float f = (float)size / (float)size222;
                result = string.Format("{0:0.0}G", f);
            }
            else if (size / size22 > 0) {
                float f = (float)size / (float)size22;
                result = string.Format("{0:0.0}M", f);
            }
            else {
                float f = (float)size / (float)size2;
                result = string.Format("{0:0.0}k", f);
            }
            return result;
        }
        private void TakeSamples(Object[] des, System.Type t, DrawDatas dt, GameObject go) {
            foreach (var g in des) {
                if (t.IsAssignableFrom(g.GetType())) {
                    SampleObject s;
                    if (!m_objectMap.TryGetValue(g, out s)) {
                        int size = GetObjectSize(g);
                        s = new SampleObject(g);
                        s.gameObjects.Add(go);
                        dt.objects.Add(s);
                        dt.totalSize += size;
                        m_objectMap.Add(g, s);
                    }
                    else {
                        s.gameObjects.Add(go);
                    }
                }
            }
        }
        #endregion

        #region draw

        private void DrawTextureSample(System.Type t, DrawDatas dt) {
            Color c = GUI.color;

            GUI.color = Color.green;
            EditorGUILayout.LabelField(string.Format("Total{0}Size:{1}", t.Name, GetShowSize(dt.totalSize)));
            GUI.color = c;

            dt.scrollPosition = EditorGUILayout.BeginScrollView(dt.scrollPosition, GUILayout.Height(Mathf.Min(200, 20 * dt.objects.Count)));
            foreach (var m in dt.objects) {
                EditorGUILayout.BeginHorizontal();
                GUI.color = !m.isInScene ? Color.red : c;
                EditorGUILayout.LabelField(string.Format("Name:{0}      Size:{1}", m.name, GetShowSize(m.size)));
                GUI.color = c;
                EditorGUILayout.ObjectField(m.obj, m.obj.GetType(), false);
                GameObject obj = m.gameObjects.Count > 0 ? m.gameObjects[0] : null;
                EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                EditorGUILayout.LabelField(string.Format("ref count:{0}", m.gameObjects.Count));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);
        }
        #endregion
    }
}

