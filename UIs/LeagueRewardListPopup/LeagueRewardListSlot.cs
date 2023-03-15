using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameData;
using UnityEngine.UI;

public class LeagueRewardListSlot : MonoBehaviour {
    public RawImage icoEmblem;
    public Text lblRank;

    public List<LeagueRewardListSlotReward> rewards = new List<LeagueRewardListSlotReward>();

    public void SetData(LeagueRewardDTO rewardData, List<PackageDTO> packageDatas) {
        icoEmblem.texture = ResourceManager.instance.GetLeagueEmblem(rewardData.leagueID);
        //상위 %
        if (rewardData.rank == 0) {
            string format = TermModel.instance.GetTerm("format_league_rank_percent");
            lblRank.text = string.Format(format, (long)rewardData.percent);
        }
        else if (rewardData.rank >= 1 && rewardData.rank <= 3) 
            lblRank.text = TermModel.instance.GetTerm($"format_rank_{rewardData.rank}");
        else {
            string format = TermModel.instance.GetTerm("format_rank_other");
            lblRank.text = string.Format(format, rewardData.rank);
        }

        for (int i = 0; i < rewards.Count; i++) {
            rewards[i].SetData(packageDatas[i]);
        }
    }    
}
