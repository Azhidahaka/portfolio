using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeUtil {
    public static void CreateChallengeMsgInfo(string receiverInDate, long senderScore = 0, long difficulty = 0) {
        UserData.ChallengeMsgDTO challengeMsgInfo = new UserData.ChallengeMsgDTO();
        challengeMsgInfo.receiverInDate = receiverInDate;
        challengeMsgInfo.senderInDate = BackendLogin.instance.UserInDate;
        //스코어, 난이도정보 추가는 게임이 끝난 뒤에 진행
        challengeMsgInfo.senderScore = senderScore;
        if (difficulty > 0)
            challengeMsgInfo.difficulty = difficulty;
        else
            challengeMsgInfo.difficulty = UserDataModel.instance.LastSelectedStageLevel;

        UserDataModel.instance.challengeMsgInfo = challengeMsgInfo;
    }
}
