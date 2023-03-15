using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingChallenge : MonoBehaviour {
    public ResultPopupChallengeSlot first;
    public ResultPopupChallengeSlot second;
    public ResultPopupChallengeSlot third;

    public GameObject prefabItem;
    public GameObject prefabHighlightItem;

    public LayoutGroup layoutGroup;

    public Text lblEmpty;

    private List<ResultPopupChallengeSlot> listItem = new List<ResultPopupChallengeSlot>();

    private List<UserData.ChallengeRankDTO> leagueRankInfos;

    private void Awake() {
        Common.ToggleActive(prefabItem, false);
        Common.ToggleActive(prefabHighlightItem, false);
    }

    public void SetData() {
        this.leagueRankInfos = UserDataModel.instance.leagueRankInfos;

        UserData.ChallengeRankDTO firstInfo = null;
        UserData.ChallengeRankDTO secondInfo = null;
        UserData.ChallengeRankDTO thirdInfo = null;
            
        if (UserDataModel.instance.top3RankInfos.Count >= 1)
            firstInfo = UserDataModel.instance.top3RankInfos[0];
        if (UserDataModel.instance.top3RankInfos.Count >= 2)
            secondInfo = UserDataModel.instance.top3RankInfos[1];
        if (UserDataModel.instance.top3RankInfos.Count >= 3)
            thirdInfo = UserDataModel.instance.top3RankInfos[2];

        first.SetData(firstInfo, true);
        second.SetData(secondInfo, true);
        third.SetData(thirdInfo, true);

        for (int i = 0; i < listItem.Count; i++) {
            Destroy(listItem[i].gameObject);
        }
        listItem.Clear();
        
        if (leagueRankInfos.Count > 0) {
            SetScrollView();
            Common.ToggleActive(lblEmpty.gameObject, false);
        }
        else {
            Common.ToggleActive(lblEmpty.gameObject, true);
        }
    }

    private void SetScrollView() {
        int myRankIndex = 0;
        for (int i = 0; i < leagueRankInfos.Count; i++) {
            if (leagueRankInfos[i].gamerInDate == BackendLogin.instance.UserInDate) {
                myRankIndex = i;
                break;
            }
        }

        int startIndex = System.Math.Max(myRankIndex - 5, 0);
        int endIndex = System.Math.Min(myRankIndex + 4, leagueRankInfos.Count - 1);
        if (endIndex + 1 - startIndex < 10)
            startIndex = System.Math.Max(0, endIndex + 1 - 10);

        if (endIndex + 1 - startIndex < 10)
            endIndex = System.Math.Min(startIndex + 10 - 1, leagueRankInfos.Count - 1);

        for (int i = startIndex; i <= endIndex; i++) {
            UserData.ChallengeRankDTO rankInfo = leagueRankInfos[i];

            bool highlight = leagueRankInfos[i].gamerInDate == BackendLogin.instance.UserInDate;
            SetItem(highlight, rankInfo);
        }
    }

    private void SetItem(bool highlight, UserData.ChallengeRankDTO rankInfo) {
        GameObject go;

        if (highlight) 
            go = Instantiate(prefabHighlightItem, layoutGroup.transform);
        else
            go = Instantiate(prefabItem, layoutGroup.transform);

        ResultPopupChallengeSlot slot = go.GetComponent<ResultPopupChallengeSlot>();
        slot.SetData(rankInfo);
        Common.ToggleActive(go, true);
        listItem.Add(slot);
    }
}
