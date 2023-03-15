using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class MultiGameListPopup : UIBase {
    public GameObject prefabLeagueInProgress;
    public GameObject prefabLeagueComingSoon;
    public LayoutGroup layoutGroup;

    private List<MultiGameListPopupSlot> listSlot = new List<MultiGameListPopupSlot>();
    private List<MultiGameListPopupSlot> listReusableSlot = new List<MultiGameListPopupSlot>();

    private void Awake() {
        InitTutorialTargets();

        Common.ToggleActive(prefabLeagueInProgress, false);
        Common.ToggleActive(prefabLeagueComingSoon, false);
    }

    private void OnEnable() {
        RectTransform rectTransform = layoutGroup.GetComponent<RectTransform>();
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);

        EventManager.Register(EventEnum.RankingButtonClick, OnRankingButtonClick);
    }

    private void OnRankingButtonClick(object[] data) {
        LeagueUtil.ShowLeagueRanking();
        Hide();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.RankingButtonClick, OnRankingButtonClick);
    }

    public void SetData() {
        SetScrollView();        
    }

    private void SetScrollView() {
        if (listSlot == null) {
            listSlot = new List<MultiGameListPopupSlot>();
            listReusableSlot = new List<MultiGameListPopupSlot>();
        }
        else {
            foreach (MultiGameListPopupSlot scrollViewItem in listSlot) {
                if (listReusableSlot.Contains(scrollViewItem) == false)
                    listReusableSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        for (int i = 0; i < listSlot.Count; i++) {
            Destroy(listSlot[i].gameObject);
        }
        listSlot.Clear();

        GameObject scrollViewGO;

        List<LeagueDataDTO> leagueInfos = UserDataModel.instance.leagueInfos;
        for (int i = 0; i < leagueInfos.Count; i++) {
            LeagueDataDTO leagueInfo = leagueInfos[i];
            MultiGameListPopupSlot scrollViewItem;

            string leagueLevelStr = ((LEAGUE_LEVEL)UserDataModel.instance.leagueScoreInfo.leagueID).ToString();
            if (leagueInfo.title.Contains(leagueLevelStr) == false)
                continue;

            if (i >= listReusableSlot.Count) {
                scrollViewGO =  Instantiate(prefabLeagueInProgress, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<MultiGameListPopupSlot>();
                listReusableSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<MultiGameListPopupSlot>();
            scrollViewItem.SetData(leagueInfo);
            listSlot.Add(scrollViewItem);
        }

        prefabLeagueComingSoon.SetActive(true);
        prefabLeagueComingSoon.transform.SetSiblingIndex(layoutGroup.transform.childCount - 1);
    }

    public MultiGameListPopupSlot GetFirstSlot() {
        if (listSlot == null || listSlot.Count == 0)
            return null;
        return listSlot[0];
    }

    public Transform GetFirstSlotTransform() {
        if (listSlot == null || listSlot.Count == 0)
            return null;
        return listSlot[0].transform;
    }
}
