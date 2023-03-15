using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUserStatus : MonoBehaviour {
    public UserStatus status;
    public Profile profile;
    public Text lblDifficulty;
    public Text lblScore;
    public GameObject goMe;
    public GameObject goFriend;

    private UserData.ChallengeMsgDTO challengeInfo;
    private bool before;

    public void SetData(UserData.ChallengeMsgDTO challengeInfo, bool sender, bool before) {
        this.challengeInfo = challengeInfo;
        this.before = before;
        if (sender)
            SetSenderInfo();
        else
            SetReceiverInfo();
    }

    public void SetSenderInfo() {
        string nickname = "";
        long leagueID = 0;
        long userNumber = 0;
        long profileID = 0;
        long frameID = 0;
        long difficulty = challengeInfo.difficulty;
        string scoreStr = "-";

        UserData.PublicUserDataDTO publicUserData = null;

        if (challengeInfo.senderInDate == BackendLogin.instance.UserInDate) {
            publicUserData = UserDataModel.instance.publicUserData;

            if (challengeInfo.senderScore > 0) 
                scoreStr = Common.GetCommaFormat(challengeInfo.senderScore);

            Common.ToggleActive(goMe, true);
            Common.ToggleActive(goFriend, false);
        }
        else {
            UserData.SimpleUserDTO friendInfo = UserDataModel.instance.GetFriendInfo(challengeInfo.senderInDate);
            if (friendInfo == null) {
                friendInfo = new UserData.SimpleUserDTO();
                friendInfo.inDate = challengeInfo.senderInDate;
                friendInfo.lastLogin = challengeInfo.senderInDate;
            }
            publicUserData = PublicUserDataManager.instance.GetPublicUserData(friendInfo);

            //아직 플레이하지 않음
            if (before) 
                scoreStr = "?";
            else
                scoreStr = Common.GetCommaFormat(challengeInfo.senderScore);
            Common.ToggleActive(goMe, false);
            Common.ToggleActive(goFriend, true);
        }

        if (publicUserData != null) {
            nickname = publicUserData.nickname;
            leagueID = publicUserData.currentLeagueID;
            userNumber = publicUserData.no;
            profileID = publicUserData.profileNo; 
            frameID = publicUserData.profileFrameNo;
        }

        lblDifficulty.text = TermModel.instance.GetTerm($"difficulty_{difficulty}");
        lblScore.text = scoreStr;
        profile.SetData(profileID, frameID);
        status.SetData(nickname, leagueID, userNumber);
    }

    public void SetReceiverInfo() {
        string nickname = "";
        long leagueID = 0;
        long userNumber = 0;
        long profileID = 0;
        long frameID = 0;
        long difficulty = challengeInfo.difficulty;
        string scoreStr = "-";

        UserData.PublicUserDataDTO publicUserData = null;

        if (challengeInfo.receiverInDate == BackendLogin.instance.UserInDate) {
            publicUserData = UserDataModel.instance.publicUserData;
            Common.ToggleActive(goMe, true);
            Common.ToggleActive(goFriend, false);
        }
        else {
            UserData.SimpleUserDTO friendInfo = UserDataModel.instance.GetFriendInfo(challengeInfo.receiverInDate);
            if (friendInfo == null) {
                friendInfo = new UserData.SimpleUserDTO();
                friendInfo.inDate = challengeInfo.receiverInDate;
                friendInfo.lastLogin = Common.ConvertTimeStampToInDateFormat(Common.GetUTCNow());
            }
            publicUserData = PublicUserDataManager.instance.GetPublicUserData(friendInfo);
            Common.ToggleActive(goMe, false);
            Common.ToggleActive(goFriend, true);
        }

        if (publicUserData != null) {
            nickname = publicUserData.nickname;
            leagueID = publicUserData.currentLeagueID;
            userNumber = publicUserData.no;
            profileID = publicUserData.profileNo;
            frameID = publicUserData.profileFrameNo;
        }

        if (before == false)
            scoreStr = Common.GetCommaFormat(challengeInfo.receiverScore);

        lblDifficulty.text = TermModel.instance.GetTerm($"difficulty_{difficulty}");
        lblScore.text = scoreStr;
        profile.SetData(profileID, frameID);
        status.SetData(nickname, leagueID, userNumber);
    }
}
