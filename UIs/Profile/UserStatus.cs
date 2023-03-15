using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserStatus : MonoBehaviour {
    public RawImage icoLeague;
    public Text lblLeague;

    public Text userName;
    public GameObject goUserNo;
    public Text lblUserNo;

    public void SetData() {
        SetData(BackendLogin.instance.nickname, UserDataModel.instance.publicUserData.currentLeagueID, UserDataModel.instance.publicUserData.no);
    }

    public void SetData(string nickname, long leagueID, long userNo) {
        icoLeague.texture = ResourceManager.instance.GetLeagueEmblem(leagueID);
        lblLeague.text = LeagueUtil.GetLeagueName(leagueID);

        userName.text = nickname;
        if (goUserNo != null) {
            if (userNo > 0) {
                Common.ToggleActive(goUserNo, true);
                lblUserNo.text = LeagueUtil.GetUserNo(userNo);
            }
            else
                Common.ToggleActive(goUserNo, false);
        }
    }
}
