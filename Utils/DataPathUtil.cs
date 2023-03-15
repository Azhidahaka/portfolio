using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataPathUtil {
    public static string GetPersistentDataPath(string pPath, bool autoCreate = true) {
#if UNITY_IOS
            return GetIOSPath(autoCreate) + pPath; 
#else
            return Application.persistentDataPath + pPath;
#endif
    }

    private static string GetIOSPath(bool autoCreate = true) {
        string path = Path.Combine(Application.persistentDataPath, "LuckyFlow_MatchBlocks");

        if (autoCreate && Directory.Exists(path) == false) 
            Directory.CreateDirectory(path);
        return path;
    }

    public static string GetPersistentDataPath(bool autoCreate = true) {
#if UNITY_IOS
        return GetIOSPath(autoCreate);
#else
        return Application.persistentDataPath;
#endif
    }
}
