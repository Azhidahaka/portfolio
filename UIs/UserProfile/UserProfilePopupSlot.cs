using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class UserProfilePopupSlot : MonoBehaviour {
    public RawImage icoEmblem;
    public Text lblSeason;
    public Text lblScore;
    public Text lblRank;

    public void SetData(PublicUserDataDTO.LeagueRecordDTO record) {
        icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(record.leagueID);
        string seasonFormat = TermModel.instance.GetTerm("format_season_no");
        lblSeason.text = string.Format(seasonFormat, record.seasonNo);
        lblScore.text = Common.GetCommaFormat(record.score);
        if (record.rank == 0)
            lblRank.text = TermModel.instance.GetTerm("not_played_rank");
        else if (record.rank <= 3) {
            lblRank.text = TermModel.instance.GetTerm($"format_rank_{record.rank}");
        }
        else {
            string format = TermModel.instance.GetTerm("format_rank_other");
            lblRank.text = string.Format(format, Common.GetCommaFormat(record.rank));
        }
    }
}
