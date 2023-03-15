using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeagueResultPopup : UIBase {
    public Animator animator;

    public TextMeshProUGUI lblScore;
    public Text lblRank;
    public RawImage icoEmblem;

    public AnimCallbackLinker animCallbackLinker;

    private Callback hideCallback;
    private bool enableHide = false;

    private void OnEnable() {
        SetInFrontInCanvas();
    }

    public void SetData(UserData.LeagueScoreDTO leagueScoreInfo, Callback hideCallback) {
        enableHide = false;
        this.hideCallback = hideCallback;

        lblScore.text = Common.GetCommaFormat(leagueScoreInfo.leagueScore);
        if (leagueScoreInfo.lastRank == 0) 
            lblRank.text = TermModel.instance.GetTerm("not_played_rank");
        else if (leagueScoreInfo.lastRank >= 1 && leagueScoreInfo.lastRank <= 3) 
            lblRank.text = TermModel.instance.GetTerm($"format_rank_{leagueScoreInfo.lastRank}");
        else {
            string rankFormat = TermModel.instance.GetTerm("format_rank_other");
            lblRank.text = string.Format(rankFormat, Common.GetCommaFormat(leagueScoreInfo.lastRank));
        } 
        icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(leagueScoreInfo.leagueID);

        animCallbackLinker.SetCallback(() => enableHide = true);
    }
    public void OnHideClick() {
        if (enableHide == false)
            return;

        Hide();
        hideCallback();
    }
}
