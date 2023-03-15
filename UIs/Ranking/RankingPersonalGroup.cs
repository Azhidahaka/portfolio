using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingPersonalGroup : MonoBehaviour {
    public Text lblDifficulty;

    public GameObject prefabItem;

    public LayoutGroup layoutGroup;

    public Text lblEmpty;

    private List<ResultPopupPersonalSlot> listItem = new List<ResultPopupPersonalSlot>();
    private List<UserData.SingleRankDTO> rankInfos;

    private void Awake() {
        Common.ToggleActive(prefabItem, false);
    }

    public void SetData(long stageLevel, List<UserData.SingleRankDTO> rankInfos) {
        lblDifficulty.text = TermModel.instance.GetTerm($"difficulty_{stageLevel}");

        this.rankInfos = rankInfos;

        Common.ToggleActive(lblEmpty.gameObject, rankInfos.Count == 0);

        for (int i = 0; i < listItem.Count; i++) {
            Destroy(listItem[i].gameObject);
        }
        listItem.Clear();

        SetScrollView();
    }

    private void SetScrollView() {
        for (int i = 0; i < Constant.SHOW_RANKING_COUNT; i++) {
            UserData.SingleRankDTO rankInfo = null;
            if (i <= rankInfos.Count - 1) {
                rankInfo = rankInfos[i];
            }

            SetItem(rankInfo);
        }
    }

    private void SetItem(UserData.SingleRankDTO rankInfo) {
        GameObject go = Instantiate(prefabItem, layoutGroup.transform);

        ResultPopupPersonalSlot slot = go.GetComponent<ResultPopupPersonalSlot>();
        slot.SetData(rankInfo);
        Common.ToggleActive(go, true);
        listItem.Add(slot);
    }
}
