using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopupChallenge : UIBase {
    public GameObject prefabItem;
    public GameObject prefabHighlightItem;

    public LayoutGroup layoutGroup;
    public ScoreEffect scoreEffect;

    public Text lblRewardGold;

    private List<ResultPopupChallengeSlot> listItem = new List<ResultPopupChallengeSlot>();

    private List<UserData.ChallengeRankDTO> leagueRankInfos;
    private UserData.ChallengeRankDTO currentRankInfo;

    private void Awake() {
        Common.ToggleActive(prefabItem, false);
        Common.ToggleActive(prefabHighlightItem, false);
    }

    public void SetData(string nickname) {
        this.leagueRankInfos = UserDataModel.instance.leagueRankInfos;

        UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();

        lblRewardGold.text = Common.GetCommaFormat(refereeNote.rewardGold);

        UserData.ChallengeRankDTO currentRankInfo;
        if (UserDataModel.instance.myChallengeRankInfo != null)
            currentRankInfo = UserDataModel.instance.myChallengeRankInfo;
        else { 
            currentRankInfo = new UserData.ChallengeRankDTO();
            currentRankInfo.extension.nickname = nickname;
            currentRankInfo.score = refereeNote.totalScore;
            currentRankInfo.extension.flagNo = UserDataModel.instance.userProfile.flagNo;
        }
        this.currentRankInfo = currentRankInfo;

        for (int i = 0; i < listItem.Count; i++) {
            Destroy(listItem[i].gameObject);
        }
        listItem.Clear();

        SetScrollView();

        scoreEffect.SetData(refereeNote.totalScore);
    }

    private void SetScrollView() {
        int myRankIndex = 0;
        for (int i = 0; i < leagueRankInfos.Count; i++) {
            if (leagueRankInfos[i].gamerInDate == BackendLogin.instance.UserInDate) {
                myRankIndex = i;
                break;
            }
        }

        int startIndex = System.Math.Max(myRankIndex - 5, 0);
        int endIndex = System.Math.Min(myRankIndex + 4, leagueRankInfos.Count - 1);
        if (endIndex + 1 - startIndex < 10)
            startIndex = System.Math.Max(0, endIndex + 1 - 10);

        if (endIndex + 1 - startIndex < 10)
            endIndex = System.Math.Min(startIndex + 10 - 1, leagueRankInfos.Count - 1);

        STATISTICS_TYPE key = STATISTICS_TYPE.ETC_CHALLENGE_MODE_BEST_RANK;
        long bestRank = UserDataModel.instance.GetStatistics(key);
        if (currentRankInfo.rank > 0 && bestRank == 0 || currentRankInfo.rank < bestRank)
            WebUser.instance.ReqSetStatistics(currentRankInfo.rank, key);

        for (int i = startIndex; i <= endIndex; i++) {
            UserData.ChallengeRankDTO rankInfo = leagueRankInfos[i];

            bool highlight = leagueRankInfos[i].gamerInDate == BackendLogin.instance.UserInDate;
            SetItem(highlight, rankInfo);
        }

        if (UserDataModel.instance.myChallengeRankInfo == null)
            SetItem(true, currentRankInfo);
    }

    private void SetItem(bool highlight, UserData.ChallengeRankDTO rankInfo) {
        GameObject go;

        if (highlight) 
            go = Instantiate(prefabHighlightItem, layoutGroup.transform);
        else
            go = Instantiate(prefabItem, layoutGroup.transform);

        ResultPopupChallengeSlot slot = go.GetComponent<ResultPopupChallengeSlot>();
        slot.SetData(rankInfo);
        Common.ToggleActive(go, true);
        listItem.Add(slot);
    }

    public void OnBtnChallengeClick() {
        WebStage.instance.ReqLeagueStart(() => {
            App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
        });
    }

    public void OnBtnHomeClick() {
        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.INTERSTITIAL);
        Callback callback = () => {
            App.instance.ChangeScene(App.SCENE_NAME.Home);
        };

        if (UserDataModel.instance.IsAdsRemoved() == false && 
            adsInstance != null && adsInstance.IsInterstitialLoaded())
            adsInstance.ShowInterstitial(callback);
        else
            callback();
    }

    public void OnBtnRankingDetailClick() {
        App.instance.ChangeScene(App.SCENE_NAME.Home, false, UI_NAME.Ranking);
    }

    public void OnBtnShareClick() {
        ShareManager.instance.Share();
        AnalyticsUtil.LogShareChallengeResult();
    }

    public override void Hide() {
        base.Hide();
    }
}
