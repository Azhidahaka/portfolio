using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour {
    public RawImage imgProfile;
    public RawImage imgProfileOutline;

    private void OnEnable() {
        EventManager.Register(EventEnum.ProfileSkinChanged, OnProfileSkinChanged);
    }

    private void OnProfileSkinChanged(object[] args) {
        SetData(UserDataModel.instance.publicUserData);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ProfileSkinChanged, OnProfileSkinChanged);
    }

    public void SetData(UserData.PublicUserDataDTO userStatus) {
        if (userStatus == null) {
            imgProfile.texture = ResourceManager.instance.GetProfileTexture(0);
            imgProfileOutline.texture = ResourceManager.instance.GetProfileFrame(0);
        }
        else {
            imgProfile.texture = ResourceManager.instance.GetProfileTexture(userStatus.profileNo);
            imgProfileOutline.texture = ResourceManager.instance.GetProfileFrame(userStatus.profileFrameNo);
        }
    }

    public void SetData(long profileNo, long frameNo) {
        imgProfile.texture = ResourceManager.instance.GetProfileTexture(profileNo);
        imgProfileOutline.texture = ResourceManager.instance.GetProfileFrame(frameNo);
    }
}
