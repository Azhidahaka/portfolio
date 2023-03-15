using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelPopup : UIBase {

    private void Awake() {
        InitTutorialTargets();
    }

    private void OnEnable() {
        SetInFrontInCanvas();
        EventManager.Notify(EventEnum.GameLevelPopupShow);
    }
    public void SetData(string receiverInDate = "") {
        ChallengeUtil.CreateChallengeMsgInfo(receiverInDate);
    }

    private void StartGame(STAGE_LEVEL level) {
        Callback startGame = () => {
            UserDataModel.instance.LastFriendMessage = new UserData.FriendMessageDTO();

            UserDataModel.instance.LastSelectedStageLevel = (long)level;
            UserDataModel.instance.RemoveRefereeNote();
            UserDataModel.instance.ContinueGame = false;
            App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);

            //도전장인경우 난이도 셋팅
            //if (string.IsNullOrEmpty(UserDataModel.instance.challengeMsgInfo.receiverInDate) == false)
            UserDataModel.instance.challengeMsgInfo.difficulty = (long)level;

            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_PLAYED_COUNT, 1, false);

            AnalyticsUtil.LogPlayStage(UserDataModel.instance.LastSelectedStageLevel);
        };

        MatchBlocksUtil.ConfirmGold(startGame);
    }

    public void OnBtnEasyClick() {
        StartGame(STAGE_LEVEL.Easy);
    }

    public void OnBtnNormalClick() {
        StartGame(STAGE_LEVEL.Normal);
    }

    public void OnBtnHardClick() {
        StartGame(STAGE_LEVEL.Hard);
    }

    public void OnBtnExtremeClick() {
        StartGame(STAGE_LEVEL.ExtremeHard);
    }
}
