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
#if UNITY_EDITOR
using UnityEditor;
#endif

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

public class AssetBundleManager {
    private static AssetBundleManifest m_manifestBundle;

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
        string[] cubedepends = m_manifestBundle.GetAllDependencies(assetBundleName);
        AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];
        for (int index = 0; index < cubedepends.Length; index++) {
            //加载所有的依赖文件;  
            dependsAssetbundle[index] = AssetBundle.LoadFromFile(Path.Combine(abPath, cubedepends[index]));
        }
        //加载我们需要的文件;  
        AssetBundle cubeBundle = AssetBundle.LoadFromFile(Path.Combine(abPath, assetBundleName));

        result = cubeBundle.LoadAsset(assetName);

        for (int index = 0; index < cubedepends.Length; index++) {
            //加载所有的依赖文件;  
            dependsAssetbundle[index].Unload(false);
        }
        cubeBundle.Unload(false);


        return result;
        #endregion

    }

}
