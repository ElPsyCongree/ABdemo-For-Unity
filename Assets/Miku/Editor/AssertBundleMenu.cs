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
* Filename: AssertBundleMenu.cs
* Created:  2017/7/21 21:15:54
* Author:   HaYaShi ToShiTaKa
* Purpose:  
* ==============================================================================
*/

using System.IO;
using UnityEditor;

namespace AssetBundleTool {
    public class AssertBundleMenu {

        #region build ab
        [MenuItem("Assets/AssetBundles/Build AssetBundles")]
        static public void BuildAssetBundles() {
            BuildAssetBundles(null);
        }
        public static void BuildAssetBundles(AssetBundleBuild[] builds) {
            // Choose the output path according to the build target.
            string outputPath = CreateAssetBundleDirectory();

            var options = BuildAssetBundleOptions.None;

            bool shouldCheckODR = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
#if UNITY_TVOS
            shouldCheckODR |= EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS;
#endif
            if (shouldCheckODR) {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if (PlayerSettings.iOS.useOnDemandResources)
                    options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
#if ENABLE_IOS_APP_SLICING
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
            }

            if (builds == null || builds.Length == 0) {
                //@TODO: use append hash... (Make sure pipeline works correctly with it.)
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else {
                BuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            }
        }
        static public string CreateAssetBundleDirectory() {
            // Choose the output path according to the build target.
            string outputPath = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return outputPath;
        }
        #endregion

        #region SimulationMode
        const string kSimulationMode = "Assets/AssetBundles/Simulation Mode";

        [MenuItem(kSimulationMode)]
        public static void ToggleSimulationMode() {
            AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        }

        [MenuItem(kSimulationMode, true)]
        public static bool ToggleSimulationModeValidate() {
            Menu.SetChecked(kSimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
            return true;
        }
        #endregion

    }
}
