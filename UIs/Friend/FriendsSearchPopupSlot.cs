using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsSearchPopupSlot : MonoBehaviour {
    public RawImage icoProfile;
    public RawImage icoFrame;

    public RawImage icoEmblem;
    public Text lblLeagueName;

    public Text lblNickname;
    public GameObject goUserNo;
    public Text lblUserNo;

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

        if (publicUserData != null) {
            lblNickname.text = LeagueUtil.GetUserName(simpleUserInfo.nickname);
            Common.ToggleActive(goUserNo, publicUserData.no > 0);
            lblUserNo.text = LeagueUtil.GetUserNo(publicUserData.no);
            icoProfile.texture = ResourceManager.instance.GetProfileTexture(publicUserData.profileNo);
            icoFrame.texture = ResourceManager.instance.GetProfileFrame(publicUserData.profileFrameNo);
            icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(publicUserData.currentLeagueID);
            lblLeagueName.text = LeagueUtil.GetLeagueName(publicUserData.currentLeagueID);
        }
        else {
            Common.ToggleActive(goUserNo, false);
            lblNickname.text = simpleUserInfo.nickname;
            icoProfile.texture = ResourceManager.instance.GetProfileTexture(0);
            icoFrame.texture = ResourceManager.instance.GetProfileFrame(0);
            icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem((long)LEAGUE_LEVEL.BRONZE);
            lblLeagueName.text = LeagueUtil.GetLeagueName((long)LEAGUE_LEVEL.BRONZE);
        }
    }

    public void OnBtnInviteClick() {
        BackendRequest.instance.ReqFriendInvite(simpleUserInfo.inDate, NotifyUpdate);
    }

    public void OnBtnAgreeClick() {
        BackendRequest.instance.ReqFriendAccept(simpleUserInfo, NotifyUpdate);
    }

    public void OnBtnDisagreeClick() {
        BackendRequest.instance.ReqFriendReject(simpleUserInfo.inDate, NotifyUpdate);
    }

    public void NotifyUpdate() {
        EventManager.Notify(EventEnum.FriendsStateUpdate);
    }
}
