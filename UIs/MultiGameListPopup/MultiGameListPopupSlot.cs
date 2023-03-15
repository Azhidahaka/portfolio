using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class MultiGameListPopupSlot : MonoBehaviour {
    public GameObject objEvent;
    public Text lblPeriod;

    public Text lblSeasonNo;
    public RawImage icoEmblem;
    public Text lblLeagueName;

    public Text lblRank;
    public Text lblNickname;
    public Text lblScore;

    public Text lblNoRecord;

    public Text lblGold;

    public GameObject prefabReward;
    public LayoutGroup layoutGroup;
    public GameObject objRewardNone;

    public Text lblLeagueDescContent;

    private LeagueDataDTO leagueData;
    private LeagueScoreDTO leagueScoreInfo;

    private List<MultiGameListPopupSlotReward> listReward = new List<MultiGameListPopupSlotReward>();
    private List<MultiGameListPopupSlotReward> listReusableReward = new List<MultiGameListPopupSlotReward>();

    public GameObject goPosBtnReward;
    public GameObject goPosBtnStart;

    private void Awake() {
        Common.ToggleActive(prefabReward, false);
    }

    public void SetData(LeagueDataDTO leagueData) {
        this.leagueData = leagueData;
        leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;

        Common.ToggleActive(objEvent, false);
        string start = Common.ConvertTimestampToLeagueDate(leagueData.rankStartDateAndTime);
        string end = Common.ConvertTimestampToLeagueDate(leagueData.rankEndDateAndTime);
        lblPeriod.text = $"{start} ~ {end}";

        string seasonNoFormat = TermModel.instance.GetTerm("format_season_no");
        lblSeasonNo.text = string.Format(seasonNoFormat, Common.GetCommaFormat(leagueData.seasonNo));
        icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(leagueScoreInfo.leagueID);
        lblLeagueName.text = TermModel.instance.GetTerm($"league_name_{leagueScoreInfo.leagueID}");

        if (leagueScoreInfo.lastRank == 0)
            lblRank.text = TermModel.instance.GetTerm("not_played_rank");
        else if (leagueScoreInfo.lastRank >= 1 && leagueScoreInfo.lastRank <= 3)
            lblRank.text = TermModel.instance.GetTerm($"format_rank_{leagueScoreInfo.lastRank}");
        else {
            string rankFormat = TermModel.instance.GetTerm("format_rank_other");
            lblRank.text = string.Format(rankFormat, Common.GetCommaFormat(leagueScoreInfo.lastRank));
        }

        lblNickname.text = BackendLogin.instance.nickname;

        if (leagueScoreInfo.leagueScore == 0) {
            Common.ToggleActive(lblScore.gameObject, false);
            Common.ToggleActive(lblNoRecord.gameObject, true);
        }
        else {
            Common.ToggleActive(lblScore.gameObject, true);
            Common.ToggleActive(lblNoRecord.gameObject, false);
            lblScore.text = Common.GetCommaFormat(leagueScoreInfo.leagueScore);
        }

        long gold = UserDataModel.instance.userProfile.gold;
        if (gold > Constant.LEAGUE_USE_GOLD_LIMIT)
            gold = Constant.LEAGUE_USE_GOLD_LIMIT;
        lblGold.text = Common.GetCommaFormat(gold);

        string formatLeagueDescContent = TermModel.instance.GetTerm("format_league_desc_content");
        lblLeagueDescContent.text = string.Format(formatLeagueDescContent, Constant.LEAGUE_USE_GOLD_LIMIT);

        SetCurrentRewards();
    }

    private void SetCurrentRewards() {
        if (listReward == null) {
            listReward = new List<MultiGameListPopupSlotReward>();
            listReusableReward = new List<MultiGameListPopupSlotReward>();
        }
        else {
            foreach (MultiGameListPopupSlotReward scrollViewItem in listReward) {
                if (listReusableReward.Contains(scrollViewItem) == false)
                    listReusableReward.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listReward.Clear();
        }

        List<GameData.PackageDTO> packageDatas = LeagueUtil.GetRewardPackageDatas();

        GameObject scrollViewGO;

        for (int i = 0; i < packageDatas.Count; i++) {
            GameData.PackageDTO packageData = packageDatas[i];
            MultiGameListPopupSlotReward scrollViewItem;

            if (i >= listReusableReward.Count) {
                scrollViewGO = Instantiate(prefabReward, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<MultiGameListPopupSlotReward>();
                listReusableReward.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableReward[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<MultiGameListPopupSlotReward>();
            scrollViewItem.SetData(packageData);
            listReward.Add(scrollViewItem);
        }

        Common.ToggleActive(objRewardNone, packageDatas.Count == 0);
    }

    public void OnBtnStartClick() {
        long now = Common.GetUTCNow();
        long startTime = Common.GetUTCTodayZero() + 18 * 3600 + 1800;
        long endTime = Common.GetUTCTodayZero() + 20 * 3600;

        if (now >= startTime && now <= endTime) {
            string startTimeStr = Common.ConvertTimestampToHM(startTime);
            string endTimeStr = Common.ConvertTimestampToHM(endTime);
            string format = TermModel.instance.GetTerm("format_league_time_warning");
            string msg = string.Format(format, startTimeStr, endTimeStr);

            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg);
            return;
        }

        Callback startGame = () => {
            WebStage.instance.ReqLeagueStart(() => {
                App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
            });
        };

        MatchBlocksUtil.ConfirmGold(startGame);
    }


    public void OnBtnShowRewardClick() {
        LeagueRewardListPopup rewardListPopup = UIManager.instance.GetUI<LeagueRewardListPopup>(UI_NAME.LeagueRewardListPopup);
        rewardListPopup.SetData();
        rewardListPopup.Show();
    }

    public void OnBtnRankingClick() {
        EventManager.Notify(EventEnum.RankingButtonClick);
    }
}
