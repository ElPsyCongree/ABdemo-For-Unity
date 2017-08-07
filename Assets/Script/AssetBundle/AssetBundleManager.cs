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
* Filename: AssetBundleManager.cs
* Created:  2017/7/21 21:21:22
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace AssetBundleTool {
    public class LoadedAssetBundle {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        internal event Action unload;

        internal void OnUnload() {
            m_AssetBundle.Unload(false);
            if (unload != null)
                unload();
        }

        public LoadedAssetBundle(AssetBundle assetBundle) {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    public static class AssetBundleManager {
        private static AssetBundleManifest m_manifestBundle;

        private static Dictionary<string, AssetBundle> m_bundles = new Dictionary<string, AssetBundle>();
        private static Dictionary<AssetBundle, string> m_bundleNames = new Dictionary<AssetBundle, string>();

        public static AssetBundle GetAssetBundle(string path) {
            AssetBundle result = null;

            if (!m_bundles.TryGetValue(path, out result)) {
                result = AssetBundle.LoadFromFile(path);
                m_bundles.Add(path, result);
                m_bundleNames.Add(result, path);
            }

            return result;
        }

        public static void UnloadM(this AssetBundle ab, bool type) {
            string path;
            if (m_bundleNames.TryGetValue(ab, out path)) {
                ab.Unload(type);
                m_bundles.Remove(path);
                m_bundleNames.Remove(ab);
            }
        }

        #region Simulate
#if UNITY_EDITOR
        static int m_SimulateAssetBundleInEditor = -1;
        const string kSimulateAssetBundles = "SimulateAssetBundles";
        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulateAssetBundleInEditor {
            get {
                if (m_SimulateAssetBundleInEditor == -1)
                    m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;

                return m_SimulateAssetBundleInEditor != 0;
            }
            set {
                int newValue = value ? 1 : 0;
                if (newValue != m_SimulateAssetBundleInEditor) {
                    m_SimulateAssetBundleInEditor = newValue;
                    EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
        }
#endif
        #endregion

        public static UnityEngine.Object LoadAssetBundle(string assetBundleName, string assetName) {
            UnityEngine.Object result = null;

            #region Simulate
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor) {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if (assetPaths.Length == 0) {
                    return null;
                }
                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                result = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                return result;
            }
#endif
            #endregion

            #region normal mode
            string abPath = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());

            if (m_manifestBundle == null) {
                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(abPath, Utility.GetPlatformName()));
                if (bundle != null) {
                    m_manifestBundle = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
                }
                if (m_manifestBundle == null) throw new NullReferenceException("mainifest bundle can't be null");
            }

            //获取依赖文件列表; 
            string path;
            string[] cubedepends = m_manifestBundle.GetAllDependencies(assetBundleName);
            AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];
            for (int index = 0; index < cubedepends.Length; index++) {
                //加载所有的依赖文件;
                path = Path.Combine(abPath, cubedepends[index]);
                dependsAssetbundle[index] = GetAssetBundle(path);
            }

            //加载我们需要的文件;  
            path = Path.Combine(abPath, assetBundleName);
            AssetBundle cubeBundle = GetAssetBundle(path);
            result = cubeBundle.LoadAsset(assetName);

            //for (int index = 0; index < cubedepends.Length; index++) {
            //    //加载所有的依赖文件;  
            //    dependsAssetbundle[index].UnloadM(false);
            //}
            //cubeBundle.UnloadM(false);


            return result;
            #endregion

        }

    }
}