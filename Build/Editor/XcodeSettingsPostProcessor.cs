#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Collections.Generic;

namespace LuckyFlow {
    public class XcodeSettingsPostProcessor {
        [PostProcessBuild(999)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject) {
            if (buildTarget != BuildTarget.iOS)
                return;

            string projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxproject = new PBXProject();
            pbxproject.ReadFromFile(projectPath);

            List<string> localizeList = new List<string>();

            localizeList.Add("Base.lproj"); //기본, 영어
     

            string targetGuid = pbxproject.GetUnityMainTargetGuid();


            string unityFolderPath = Application.dataPath + "/Editor/iOSLocalization/";
            string buildPath = "./BuildOutput/MatchBlocks/";
            string fileName = "InfoPlist.strings";

            pbxproject.AddCapability(targetGuid, PBXCapabilityType.InAppPurchase);
            pbxproject.AddCapability(targetGuid, PBXCapabilityType.PushNotifications);

            //디렉터리 생성
            foreach (string data in localizeList) {
                if (Directory.Exists(buildPath + data) == false) {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(buildPath + data);
                    if (directoryInfo == null) {
                        Debug.Log("DirectoryInfo is null");
                    }
                }

                File.Copy(unityFolderPath + data + "/" + fileName,
                          buildPath + data + "/" + fileName);

                string guid = pbxproject.AddFolderReference("./" + data, data);
                if (guid == string.Empty)
                    Debug.Log("guid not exits");

                Debug.Log($"guid:{guid}, targetGuid:{targetGuid}");
                pbxproject.AddFileToBuild(targetGuid, guid);
            }
            
            pbxproject.WriteToFile(projectPath);

            /// Add string setting
            string infoPlistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(infoPlistPath);
            /// // SKAdNetwork IDs integration(for iOS14+
            string key = "SKAdNetworkItems";
            PlistElementArray arraySKAdNetworkItems;
            if (plist.root[key] != null) 
                arraySKAdNetworkItems = plist.root[key].AsArray();
            else 
                arraySKAdNetworkItems = plist.root.CreateArray(key);
            

            // for FAN // https://developers.facebook.com/docs/audience-network/guides/SKAdNetwork 
            var dictSKAdNetworkIdentifier_FAN_1 = arraySKAdNetworkItems.AddDict(); 
            dictSKAdNetworkIdentifier_FAN_1.SetString("SKAdNetworkIdentifier", "v9wttpbfk9.skadnetwork"); // FAN 1 
            var dictSKAdNetworkIdentifier_FAN_2 = arraySKAdNetworkItems.AddDict(); 
            dictSKAdNetworkIdentifier_FAN_2.SetString("SKAdNetworkIdentifier", "n38lu8286q.skadnetwork"); // FAN 2 

            var dictSKAdNetworkIdentifier_UNITY_1 = arraySKAdNetworkItems.AddDict(); 
            dictSKAdNetworkIdentifier_UNITY_1.SetString("SKAdNetworkIdentifier", "32z4fx6l9h.skadnetwork"); // UNITY_1 

            /// Apply editing settings to Info.plist 
            plist.WriteToFile(infoPlistPath); 
        } 
    }
}
#endif