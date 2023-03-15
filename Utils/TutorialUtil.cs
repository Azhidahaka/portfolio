using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUtil {
    public static List<UserData.SimpleUserDTO> GetTutorialFriendInfos() {
        List<UserData.SimpleUserDTO> list = new List<UserData.SimpleUserDTO>();
        UserData.SimpleUserDTO friendInfo = new UserData.SimpleUserDTO();
        friendInfo.nickname = "LuckyFlow";
        list.Add(friendInfo);
        return list;
    }

    public static UserData.PublicUserDataDTO GetTutorialPublicUserData() {
        UserData.PublicUserDataDTO publicUserData = new UserData.PublicUserDataDTO();
        publicUserData.currentLeagueID = (long)LEAGUE_LEVEL.BRONZE;
        publicUserData.no = 0;
        publicUserData.nickname = "LuckyFlow";
        publicUserData.profileFrameNo = 7;
        publicUserData.profileNo = 101;
        return publicUserData;
    }

    public static List<UserData.FriendMessageDTO> GetTutorialFriendMessages() {
        List<UserData.FriendMessageDTO> list = new List<UserData.FriendMessageDTO>();
        UserData.FriendMessageDTO message = new UserData.FriendMessageDTO();
        message.challengeMsgInfo = new UserData.ChallengeMsgDTO();
        message.challengeMsgInfo.state = (long)CHALLENGE_STATE.CHECK_RESULT;
        list.Add(message);
        return list;
    }

    public static UserData.ChallengeMsgDTO GetTutorialChallengeMsgDTO() {
        UserData.ChallengeMsgDTO challengeMsgDTO = new UserData.ChallengeMsgDTO();
        challengeMsgDTO.senderInDate = BackendLogin.instance.UserInDate;
        challengeMsgDTO.bettingGold = 1000;
        challengeMsgDTO.createDate = Common.GetUTCNow();
        challengeMsgDTO.difficulty = UserDataModel.instance.statistics.tutorialChallengeDifficulty;
        challengeMsgDTO.senderScore = UserDataModel.instance.statistics.tutorialChallengeScore;
        challengeMsgDTO.receiverScore = challengeMsgDTO.senderScore - 10;
        challengeMsgDTO.state = (long)CHALLENGE_STATE.CHECK_RESULT;
        return challengeMsgDTO;
    }
}
