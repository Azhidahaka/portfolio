using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopupPersonal : UIBase {
    public GameObject prefabItem;
    public GameObject prefabHighlightItem;

    public LayoutGroup layoutGroup;
    public ScoreEffect scoreEffect;

    public GameObject goBtnChallenge;

    public Text lblRewardGold;

    private List<ResultPopupPersonalSlot> listItem = new List<ResultPopupPersonalSlot>();
    private List<UserData.SingleRankDTO> rankInfos;
    private UserData.SingleRankDTO currentRankInfo;

    private void Awake() {
        InitTutorialTargets();

        Common.ToggleActive(prefabItem, false);
        Common.ToggleActive(prefabHighlightItem, false);
    }

    public void SetData(List<UserData.SingleRankDTO> rankInfos, UserData.SingleRankDTO currentRankInfo) {
        UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
        lblRewardGold.text = Common.GetCommaFormat(refereeNote.rewardGold);

        this.rankInfos = rankInfos;
        this.currentRankInfo = currentRankInfo;

        for (int i = 0; i < listItem.Count; i++) {
            Destroy(listItem[i].gameObject);
        }
        listItem.Clear();

        SetScrollView();

        scoreEffect.SetData(currentRankInfo.score);
        Common.ToggleActive(goBtnChallenge, currentRankInfo.score > 0);
    }

    private void SetScrollView() {
        List<int> currentEqualIndexes = new List<int>();

        for (int i = 0; i < rankInfos.Count; i++) {
            UserData.SingleRankDTO rankInfo = rankInfos[i];    
            if (rankInfo.score == currentRankInfo.score)
                currentEqualIndexes.Add(i);
        }

        //동점인 케이스를 조회
        int equalIndex = Constant.INCORRECT;
        if (currentEqualIndexes.Count > 0) 
            equalIndex = currentEqualIndexes[currentEqualIndexes.Count - 1];

        for (int i = 0; i < Constant.SHOW_RANKING_COUNT; i++) {
            UserData.SingleRankDTO rankInfo = null;
            if (i <= rankInfos.Count - 1) {
                rankInfo = rankInfos[i];
            }

            bool highlight = i == equalIndex;
            if (highlight)
                currentRankInfo.rank = rankInfo.rank;

            SetItem(highlight, rankInfo);
        }

        if (equalIndex == Constant.INCORRECT)
            SetItem(true, currentRankInfo);
    }

    private void SetItem(bool highlight, UserData.SingleRankDTO rankInfo) {
        GameObject go;

        if (highlight) 
            go = Instantiate(prefabHighlightItem, layoutGroup.transform);
        else
            go = Instantiate(prefabItem, layoutGroup.transform);

        ResultPopupPersonalSlot slot = go.GetComponent<ResultPopupPersonalSlot>();
        slot.SetData(rankInfo);
        Common.ToggleActive(go, true);
        listItem.Add(slot);
    }

    public void OnBtnLeagueClick() {
        ChallengeGameEnterPopup popup = UIManager.instance.GetUI<ChallengeGameEnterPopup>(UI_NAME.ChallengeGameEnterPopup);
        popup.SetData();
        popup.Show();
    }

    public void OnBtnHomeClick() {
        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.INTERSTITIAL);
        Callback callback = () => {
            App.instance.ChangeScene(App.SCENE_NAME.Home);
        };

        if (UserDataModel.instance.statistics.tutorialCompleteIDs.Contains((long)TUTORIAL_ID.SHOW_CHALLENGE_RESULT) &&
            UserDataModel.instance.IsAdsRemoved() == false &&
            adsInstance != null && adsInstance.IsInterstitialLoaded())
            adsInstance.ShowInterstitial(callback);
        else
            callback();
    }

    public void OnBtnRankingDetailClick() {
        App.instance.ChangeScene(App.SCENE_NAME.Home, false, UI_NAME.Ranking);
    }

    public void OnBtnRetryClick() {
        UserDataModel.instance.ContinueGame = false;
        ChallengeUtil.CreateChallengeMsgInfo("");
        App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
    }

    public void OnBtnShareClick() {
        ShareManager.instance.Share();
        AnalyticsUtil.LogShareSingleResult();
    }

    public override void Hide() {
        base.Hide();
    }

    public void OnBtnChallengeClick() {
        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialChallengeBefore();
            return;
        }

        if (UserDataModel.instance.userProfile.challengeSendRemainCount == 0) {
            string msg = TermModel.instance.GetTerm("msg_challenge_send_limit");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        //상대정보가 없다면 상대 선택
        if (string.IsNullOrEmpty(UserDataModel.instance.challengeMsgInfo.receiverInDate)) {
            Callback callback = () => {
                ChallengeFriendListPopup challengeFriendListPopup = UIManager.instance.GetUI<ChallengeFriendListPopup>(UI_NAME.ChallengeFriendListPopup);
                challengeFriendListPopup.SetData();
                challengeFriendListPopup.Show();
            };

            if (UserDataModel.instance.getFriendList)
                callback();
            else
                BackendRequest.instance.ReqFriendList(callback);
        }
        //상대가 이미 결정된 상태라면 베팅금액 선택
        else {
            ChallengeBeforePopup challengeBeforePopup = UIManager.instance.GetUI<ChallengeBeforePopup>(UI_NAME.ChallengeBeforePopup);
            challengeBeforePopup.SetData();
            challengeBeforePopup.Show();
        }
    }

    private void TutorialChallengeBefore() {
        Common.ToggleActive(goBtnChallenge, false);

        ChallengeFriendListPopup challengeFriendListPopup = UIManager.instance.GetUI<ChallengeFriendListPopup>(UI_NAME.ChallengeFriendListPopup);
        challengeFriendListPopup.SetData();
        challengeFriendListPopup.Show();
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.ChallengeMessageSend, OnChallengeMessageSend);
    }

    private void OnChallengeMessageSend(object[] args) {
        Common.ToggleActive(goBtnChallenge, false);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ChallengeMessageSend, OnChallengeMessageSend);
    }
}
