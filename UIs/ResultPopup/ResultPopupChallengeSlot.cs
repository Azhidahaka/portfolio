using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopupChallengeSlot : MonoBehaviour {
    public Text lblRank;
    public Text lblNickname;
    public Text lblScore;
    public RawImage icoFlag;
    public GameObject bgFlag;
    public GameObject objMyRank;

    public RawImage icoEmblem;
    public RawImage icoFrame;

    private UserData.ChallengeRankDTO rankInfo;

    public void SetData(UserData.ChallengeRankDTO rankInfo, bool top3 = false) {
        this.rankInfo = rankInfo;

        if (top3 && 
            rankInfo != null &&
            rankInfo.gamerInDate == BackendLogin.instance.UserInDate)
            Common.ToggleActive(objMyRank, true);
        else
            Common.ToggleActive(objMyRank, false);

        if (rankInfo == null) {
            lblRank.text = "";
            lblNickname.text = "";
            lblScore.text = "";
            icoFlag.texture = null;
            Common.ToggleActive(icoFlag.gameObject, false);
            Common.ToggleActive(bgFlag, false);
            if (icoFrame != null)
                Common.ToggleActive(icoFrame.gameObject, false);
        }
        else {
            lblScore.text = Common.GetCommaFormat(rankInfo.score);
            if (rankInfo.rank > 0)
                lblRank.text = Common.GetCommaFormat(rankInfo.rank);
            else
                lblRank.text = "-";

            if (rankInfo.extension != null) {
                lblNickname.text = rankInfo.extension.nickname;
                icoFlag.texture = ResourceManager.instance.GetProfileTexture(rankInfo.extension.profileNo);
                if (icoFrame != null)
                    icoFrame.texture = ResourceManager.instance.GetProfileFrame(rankInfo.extension.frameNo);
            }

            Common.ToggleActive(icoFlag.gameObject, true);
            Common.ToggleActive(bgFlag, true);
            if (icoFrame != null)
                Common.ToggleActive(icoFrame.gameObject, true);
        }

        icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(UserDataModel.instance.leagueScoreInfo.leagueID);
    }
}
