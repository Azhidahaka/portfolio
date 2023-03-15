using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserUtil {
    public static bool IsNicknameEmpty() {
        if (UserDataModel.instance.userProfile.nickname == null ||
            UserDataModel.instance.userProfile.nickname == "")
            return true;
        return false;
    }

    public static void CheckLastLogin(UserData.SimpleUserDTO simpleUserInfo) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (simpleUserInfo == null)
            return;
        if (string.IsNullOrEmpty(simpleUserInfo.lastLogin))
            Debug.LogError($"{simpleUserInfo.nickname}::lastLogin is null");
        else if (simpleUserInfo.lastLogin.Length != 24)
            Debug.LogError($"{simpleUserInfo.nickname}::lastLogin is wrong::{simpleUserInfo.lastLogin}");
#endif
    }
}
