using LuckyFlow.EnumDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ranking : UIBase {
    public enum POPUP_STATE {
        PERSONAL,
        CHALLENGE,
    }

    public GameObject objPersonal;
    public RankingPersonalGroup easy;
    public RankingPersonalGroup normal;
    public RankingPersonalGroup hard;
    public RankingPersonalGroup extremeHard;

    public GameObject objChallenge;
    public RankingChallenge rankingChallenge;

    public GameObject bgBtnPersonal;
    public GameObject bgBtnChallenge;

    public ScrollRect scrollRect;

    public GameObject goMyInfo;
    public Text lblSeasonNo;
    public RawImage icoEmblem;
    public Text lblLeagueName;
    public Text lblRank;
    public Text lblNickname;
    public Text lblScore;
    public Text lblNoRecord;

    private POPUP_STATE popupState;

    public void SetData(POPUP_STATE popupState) {
        this.popupState = popupState;
        if (popupState == POPUP_STATE.PERSONAL)
            SetPersonal();
        else
            SetChallenge();
        ResetGrid();
    }

    private void ResetGrid() {
        if (scrollRect.content == null)
            return;
        Vector3 gridLocalPos = scrollRect.content.localPosition;
        gridLocalPos.y = 0;
        scrollRect.content.localPosition = gridLocalPos;
    }

    private void SetPersonal() {
        scrollRect.content = objPersonal.GetComponent<RectTransform>();

        Common.ToggleActive(objPersonal, true);
        List<UserData.SingleRankDTO> easyRankInfos = UserDataModel.instance.GetSingleRankInfos((long)STAGE_LEVEL.Easy);
        easy.SetData((long)STAGE_LEVEL.Easy, easyRankInfos);
        List<UserData.SingleRankDTO> normalRankInfos = UserDataModel.instance.GetSingleRankInfos((long)STAGE_LEVEL.Normal);
        normal.SetData((long)STAGE_LEVEL.Normal, normalRankInfos);
        List<UserData.SingleRankDTO> hardRankInfos = UserDataModel.instance.GetSingleRankInfos((long)STAGE_LEVEL.Hard);
        hard.SetData((long)STAGE_LEVEL.Hard, hardRankInfos);
        List<UserData.SingleRankDTO> extremeHardRankInfos = UserDataModel.instance.GetSingleRankInfos((long)STAGE_LEVEL.ExtremeHard);
        extremeHard.SetData((long)STAGE_LEVEL.ExtremeHard, extremeHardRankInfos);

        Common.ToggleActive(objChallenge, false);

        Common.ToggleActive(bgBtnPersonal, true);
        Common.ToggleActive(bgBtnChallenge, false);
    }

    private void SetChallenge() {
        scrollRect.content = objChallenge.GetComponent<RectTransform>();

        Common.ToggleActive(objChallenge, true);
        rankingChallenge.SetData();

        Common.ToggleActive(objPersonal, false);

        Common.ToggleActive(bgBtnPersonal, false);
        Common.ToggleActive(bgBtnChallenge, true);

        SetMyInfo();
    }

    public void OnBtnPersonalClick() {
        SetData(POPUP_STATE.PERSONAL);
    }

    public void OnBtnChallengeClick() {
        if (popupState == POPUP_STATE.CHALLENGE)
            return;

        LeagueUtil.ShowLeagueRanking();
    }

    public void OnBtnBackClick() {
        Hide();
    }

    private void SetMyInfo() {
        try {
            UserData.LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
            string seasonNoFormat = TermModel.instance.GetTerm("format_season_no");
            lblSeasonNo.text = string.Format(seasonNoFormat, Common.GetCommaFormat(leagueScoreInfo.seasonNo));

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

            Common.ToggleActive(goMyInfo, true);
        }
        catch (Exception e) {
            Common.ToggleActive(goMyInfo, false);
        }
    }
}
