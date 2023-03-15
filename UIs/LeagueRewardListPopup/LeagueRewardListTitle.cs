using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeagueRewardListTitle : MonoBehaviour {
    public Text lblLeagueName;
    public void SetData(long leagueID) {
        lblLeagueName.text = TermModel.instance.GetTerm($"league_name_{leagueID}");
    }
}
