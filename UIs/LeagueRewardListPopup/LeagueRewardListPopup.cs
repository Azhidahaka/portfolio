using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeagueRewardListPopup : UIBase {
    public GameObject prefabTitle;
    public GameObject prefabRewardSlotOne;
    public GameObject prefabRewardSlotTwo;
    public GameObject prefabRewardSlotThree;
    public GameObject prefabRewardSlotFour;

    public LayoutGroup layoutGroup;

    private bool scrollViewSet = false;

    private void Awake() {
        Common.ToggleActive(prefabTitle, false);
        Common.ToggleActive(prefabRewardSlotOne, false);
        Common.ToggleActive(prefabRewardSlotTwo, false);
        Common.ToggleActive(prefabRewardSlotThree, false);
        Common.ToggleActive(prefabRewardSlotFour, false);
    }

    public void SetData() {
        SetScrollView();
    }

    private void SetScrollView() {
        if (scrollViewSet)
            return;
        scrollViewSet = true;


        List<GameData.LeagueRewardDTO> leagueRewards = GameDataModel.instance.leagueRewards;
        long prevLeagueID = 0;
        for (int i = 0; i < leagueRewards.Count; i++) {
            GameData.LeagueRewardDTO rewardData = leagueRewards[i];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (rewardData.leagueID >= (long)LEAGUE_LEVEL.BRONZE &&
                rewardData.leagueID <= (long)LEAGUE_LEVEL.SILVER)
                continue;
#else
            if (rewardData.leagueID >= (long)LEAGUE_LEVEL.TEST_BRONZE &&
                rewardData.leagueID <= (long)LEAGUE_LEVEL.TEST_SILVER)
                continue;
#endif
            GameObject go;
            if (prevLeagueID != rewardData.leagueID) {
                prevLeagueID = rewardData.leagueID;
                go = Instantiate(prefabTitle, layoutGroup.transform);
                LeagueRewardListTitle title = go.GetComponent<LeagueRewardListTitle>();
                title.SetData(rewardData.leagueID);
                Common.ToggleActive(go, true);
            }

            List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetLeagueRewardPackageDatas(rewardData.leagueID, 
                                                                                                        rewardData.rank, 
                                                                                                        rewardData.percent);
            GameObject prefab = GetPrefab(packageDatas);
            go = Instantiate(prefab, layoutGroup.transform);
            LeagueRewardListSlot slot = go.GetComponent<LeagueRewardListSlot>();
            slot.SetData(rewardData, packageDatas);
            Common.ToggleActive(go, true);
        }
    }

    private GameObject GetPrefab(List<GameData.PackageDTO> packageDatas) {
        switch(packageDatas.Count) {
            case 1:
                return prefabRewardSlotOne;
            case 2:
                return prefabRewardSlotTwo;
            case 3:
                return prefabRewardSlotThree;
        }
        return prefabRewardSlotFour;
    }
}
