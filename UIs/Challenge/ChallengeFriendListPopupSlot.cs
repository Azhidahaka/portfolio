using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeFriendListPopupSlot : MonoBehaviour {
    public Profile profile;
    public UserStatus userStatus;

    public GameObject goBtnSelect;
    public GameObject goSelected;

    private UserData.SimpleUserDTO simpleUserInfo;
    private UserData.PublicUserDataDTO publicUserData;

    private void OnEnable() {
        EventManager.Register(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    private void OnGetPublicUserData(object[] args) {
        string inDate = (string)args[0];
        if (inDate != simpleUserInfo.inDate)
            return;

        publicUserData = (UserData.PublicUserDataDTO)args[1];

        SetData(simpleUserInfo, publicUserData);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    public void SetData(UserData.SimpleUserDTO simpleUserInfo, UserData.PublicUserDataDTO publicUserData) {
        this.simpleUserInfo = simpleUserInfo;
        this.publicUserData = publicUserData;

        profile.SetData(publicUserData);

        if (publicUserData == null) 
            userStatus.SetData(simpleUserInfo.nickname, 0, 0);
        else
            userStatus.SetData(simpleUserInfo.nickname, publicUserData.currentLeagueID, publicUserData.no);
    }

    public void OnBtnSelectClick() {
        EventManager.Notify(EventEnum.ChallengeFriendListPopupSelectFriend, simpleUserInfo.inDate);
    }

    public void SetState(string friendInDate) {
        if (simpleUserInfo.inDate == friendInDate)
            Common.ToggleActive(goSelected, true);
        else
            Common.ToggleActive(goSelected, false);
    }
}
