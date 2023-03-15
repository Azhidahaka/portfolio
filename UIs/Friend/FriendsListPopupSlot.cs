using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsListPopupSlot : MonoBehaviour {
    public Profile profile;
    public UserStatus userStatus;

    public GameObject objBtnRemove;
    public GameObject objBtnChallenge;

    private UserData.SimpleUserDTO simpleUserInfo;
    private UserData.PublicUserDataDTO publicUserData;

    private void OnEnable() {
        EventManager.Register(EventEnum.FriendListPopupShowBtnBreak, OnFriendListPopupShowBtnBreak);
        EventManager.Register(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    private void OnFriendListPopupShowBtnBreak(object[] args) {
        bool friendBreak = (bool)args[0];
        
        Common.ToggleActive(objBtnRemove, friendBreak);
        Common.ToggleActive(objBtnChallenge, friendBreak == false);
    }

    private void OnGetPublicUserData(object[] args) {
        string inDate = (string)args[0];
        if (inDate != simpleUserInfo.inDate)
            return;

        publicUserData = (UserData.PublicUserDataDTO)args[1];

        SetData(simpleUserInfo, publicUserData);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.FriendListPopupShowBtnBreak, OnFriendListPopupShowBtnBreak);
        EventManager.Remove(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    public void SetData(UserData.SimpleUserDTO simpleUserInfo, UserData.PublicUserDataDTO publicUserData) {
        this.simpleUserInfo = simpleUserInfo;
        this.publicUserData = publicUserData;

        Common.ToggleActive(objBtnRemove, false);
        Common.ToggleActive(objBtnChallenge, true);
        profile.SetData(publicUserData);

        if (publicUserData == null) 
            userStatus.SetData(simpleUserInfo.nickname, 0, 0);
        else
            userStatus.SetData(simpleUserInfo.nickname, publicUserData.currentLeagueID, publicUserData.no);
    }

    public void OnBtnRemoveClick() {
        //친구삭제
        EventManager.Notify(EventEnum.FriendListPopupBtnFriendBreakClick, simpleUserInfo.inDate);
    }

    public void OnBtnChallengeClick() {
        if (UserDataModel.instance.userProfile.challengeSendRemainCount == 0) {
            string msg = TermModel.instance.GetTerm("msg_challenge_send_limit");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        GameLevelPopup gameLevelPopup = UIManager.instance.GetUI<GameLevelPopup>(UI_NAME.GameLevelPopup);
        gameLevelPopup.SetData(simpleUserInfo.inDate);
        gameLevelPopup.Show();
    }
}
