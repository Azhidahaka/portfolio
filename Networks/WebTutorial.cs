using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebTutorial : MonoBehaviour {
    public static WebTutorial instance;

    public void Awake() {
        instance = this;
    }

    public void ReqStartTutorial(long tutorialID, Callback successCallback = null) {
        if (tutorialID < 1)
            return;

        //if (UserDataModel.instance.statistics.tutorialStartIDs.Contains(tutorialID))
            //return;

        //UserDataModel.instance.statistics.tutorialStartIDs.Add(tutorialID);
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.STATISTICS);

        AnalyticsUtil.LogTutorialBegin(tutorialID);

        if (successCallback != null)
            successCallback();
    }

    public void ReqEndTutorial(long tutorialID, Callback successCallback = null) {
        if (tutorialID < 1)
            return;

        if (UserDataModel.instance.statistics.tutorialCompleteIDs.Contains(tutorialID) == false)
            UserDataModel.instance.statistics.tutorialCompleteIDs.Add(tutorialID);

        GameData.TutorialDTO tutorialData = GameDataModel.instance.GetTutorialData(tutorialID);

        if (tutorialData.rewardGold > 0) {
            UserDataModel.instance.AddGold(tutorialData.rewardGold);
            UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
            EventManager.Notify(EventEnum.GoodsUpdate);
        }

        if (tutorialID == (long)TUTORIAL_ID.SEND_CHALLENGE_MESSAGE) {
            UserDataModel.instance.statistics.tutorialChallengeScore = MatchBlocksReferee.instance.GetRefereeNote().totalScore;
            UserDataModel.instance.statistics.tutorialChallengeDifficulty = UserDataModel.instance.LastSelectedStageLevel;
        }

        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.STATISTICS);

        AnalyticsUtil.LogTutorialComplete(tutorialID);

        if (successCallback != null)
            successCallback();
    }
}
