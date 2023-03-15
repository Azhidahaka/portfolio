using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeRankingSlot : MonoBehaviour {
    public Text lblRanking;
    public Text lblNickname;
    public Text lblScore;
    public GameObject objHighlight;
    public bool SetData(UserData.ChallengeRankDTO rankInfo) {
        if (rankInfo != null) {
            if (lblRanking != null)
                lblRanking.text = rankInfo.rank.ToString();
            lblNickname.text = rankInfo.extension.nickname;
            lblScore.text = Common.GetCommaFormat(rankInfo.score);
        }
        else {
            if (lblRanking != null)
                lblRanking.text = "-";
            lblNickname.text = "-";
            lblScore.text = "-";
        }

        if (rankInfo != null && rankInfo.gamerInDate == BackendLogin.instance.UserInDate) {
            Common.ToggleActive(objHighlight, true);
            return true;
        }
        else {
            Common.ToggleActive(objHighlight, false);
            return false;
        }
    }
}
