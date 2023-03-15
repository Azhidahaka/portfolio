using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPopupPersonalSlot : MonoBehaviour {
    public Text lblRank;
    public Text lblScore;
    public Text lblTime;

    private UserData.SingleRankDTO rankInfo;

    public void SetData(UserData.SingleRankDTO rankInfo) {
        this.rankInfo = rankInfo;

        if (rankInfo == null) {
            lblRank.text = "";
            lblScore.text = "";
            lblTime.text = "";
        }
        else {
            lblScore.text = Common.GetCommaFormat(rankInfo.score);
            if (rankInfo.rank > 0)
                lblRank.text = Common.GetCommaFormat(rankInfo.rank);
            else
                lblRank.text = "-";
            lblTime.text = Common.GetDateStringFormat(rankInfo.time);
        }
    }
}
