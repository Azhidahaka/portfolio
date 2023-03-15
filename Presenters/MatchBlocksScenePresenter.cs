using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserData;

public class MatchBlocksScenePresenter : MonoBehaviour {
    public static MatchBlocksScenePresenter instance;

    private void Awake() {
        instance = this;
    }

    public void SetData() {
        bool continueGame = UserDataModel.instance.ContinueGame;
        long stageLevel;
        long totalScore = 0;
        RefereeNoteDTO refereeNote = null;

        if (continueGame) {
            refereeNote = UserDataModel.instance.LoadRefereeNote();
            stageLevel = refereeNote.stageLevel;
            totalScore = refereeNote.totalScore;
            UserDataModel.instance.LastSelectedStageLevel = stageLevel;
        }
        else 
            stageLevel = UserDataModel.instance.LastSelectedStageLevel;
         
        GameData.StageDTO stageData = GameDataModel.instance.GetStageData(stageLevel);
        MatchBlocksReferee.instance.SetData(stageData, refereeNote);

        MatchBlocks matchBlocksMenu = UIManager.instance.GetUI<MatchBlocks>(UI_NAME.MatchBlocks);

        int boardWaveIndex = 0;
        List<BundleInfoDTO> bundleInfos = null;
        if (refereeNote != null) {
            boardWaveIndex = refereeNote.boardWaveIndex;
            bundleInfos = refereeNote.bundleInfos;
        }

        matchBlocksMenu.SetData(stageData, bundleInfos, boardWaveIndex, totalScore);
        matchBlocksMenu.Show();

        UserDataModel.instance.ContinueGame = false;

        EventManager.Notify(EventEnum.LoadMatchBlocksComplete);
        MatchBlocksReferee.instance.StartMatchBlocks(matchBlocksMenu);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksEnd, OnMatchBlocksEnd);
        EventManager.Register(EventEnum.MatchBlocksBtnPiggyBankClick, OnMatchBlocksBtnPiggyBankClick); 
    }

    private void OnMatchBlocksBtnPiggyBankClick(object[] args) {
        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(UserDataModel.instance.userProfile.piggyBankType);

        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        //@todo : 골드가 최소량을 만족하지 못하면 메시지 출력
        if (collectedGold < 
            piggyBankData.capacity * (Constant.MIN_DEBIT_GOLD_PERCENT / 100)) {
            //메세지 표시
            return;
        }
        
        //골드가 충분히 모였다면 골드 획득후 상태 갱신
        WebUser.instance.ReqWithdrawPiggyBank(() => {
            EventManager.Notify(EventEnum.WithdrawPiggyGoldComplete);
        });

    }

    private void OnMatchBlocksEnd(object[] args) {
        RefereeNoteDTO refereeNote = (RefereeNoteDTO)args[0];

        ResultPopupPersonal resultPopup = UIManager.instance.GetUI<ResultPopupPersonal>(UI_NAME.ResultPopupPersonal);

        List<SingleRankDTO> singleRanks = UserDataModel.instance.GetSingleRankInfos(refereeNote.stageLevel);
        SingleRankDTO lastSingleRank = UserDataModel.instance.statistics.lastSingleRank;

        resultPopup.SetData(singleRanks, lastSingleRank);
        resultPopup.Show();

        //상대가 이미 결정되어있는 상황이라면 바로 도전장보내기 팝업을 띄워준다.
        if (lastSingleRank.score > 0 &&
            string.IsNullOrEmpty(UserDataModel.instance.challengeMsgInfo.receiverInDate) == false) {
            resultPopup.OnBtnChallengeClick();
        }

        EventManager.Notify(EventEnum.TutorialCheck);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksEnd, OnMatchBlocksEnd);
        EventManager.Remove(EventEnum.MatchBlocksBtnPiggyBankClick, OnMatchBlocksBtnPiggyBankClick); 
    }
}
