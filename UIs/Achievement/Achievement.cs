using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement : UIBase {
    public GameObject prefabSlot;

    public Text lblTitle;
    public LayoutGroup layoutGroup;
    public Text lblRemainTime;
    public GameObject objBtnGetAll;

    private List<AchievementSlot> listSlot = new List<AchievementSlot>();
    private string remainTimeFormat;

    private void OnEnable() {
        remainTimeFormat = TermModel.instance.GetTerm("format_daily_reset");
        EventManager.Register(EventEnum.RefreshDate, OnRefreshDate);
        EventManager.Register(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);
        EventManager.Register(EventEnum.AchievementGetRewardAllComplete, OnAchievementGetRewardAllComplete);
        EventManager.Register(EventEnum.AchievementGetRewardComplete, OnUpdateAchievementCleared);
        ResetGrid();

        StartCoroutine(JobCheckTime());
    }

    private void OnAchievementGetRewardAllComplete(object[] args) {
        SetData();
    }

    private void OnUpdateAchievementCleared(object[] args) {
        DetermineShowBtnGetAll();
    }

    private void ResetGrid() {
        RectTransform gridRectTransform = layoutGroup.GetComponent<RectTransform>();
        Vector3 gridLocalPos = gridRectTransform.localPosition;
        gridLocalPos.y = 0;
        gridRectTransform.localPosition = gridLocalPos;
    }

    private IEnumerator JobCheckTime() {
        while (true) {
            UpdateRemainTime();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void UpdateRemainTime() {
        string remainTimeStr = Common.GetTimerFormat(Common.GetUTCTodayZero() + 86400 - Common.GetUTCNow());
        lblRemainTime.text = string.Format(remainTimeFormat, remainTimeStr);
    }

    private void OnRefreshDate(object[] args) {
        SetData();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.RefreshDate, OnRefreshDate);
        EventManager.Remove(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);
        EventManager.Remove(EventEnum.AchievementGetRewardAllComplete, OnAchievementGetRewardAllComplete);
        EventManager.Remove(EventEnum.AchievementGetRewardComplete, OnUpdateAchievementCleared);
    }

    private void Awake() {
        lblTitle.text = TermModel.instance.GetTerm("achievement");
        Common.ToggleActive(prefabSlot, false);    
    }

    public void SetData() {
        for (int i = 0; i < listSlot.Count; i++) {
            Destroy(listSlot[i].gameObject);
        }
        listSlot.Clear();

        SetScrollView();
        UpdateRemainTime();
        DetermineShowBtnGetAll();
    }

    private void DetermineShowBtnGetAll() {
        long newCount = AchievementUtil.GetClearedAchivementCount();
        if (newCount == 0)
            Common.ToggleActive(objBtnGetAll, false);
        else
            Common.ToggleActive(objBtnGetAll, true);
    }

    private void SetScrollView() {
        //일일업적
        for (ACHIEVEMENT_GROUP group = ACHIEVEMENT_GROUP.START + 1; group < ACHIEVEMENT_GROUP.END; group++) {
            List<GameData.AchievementDTO> achievementDatas = AchievementUtil.GetSelectedAchievementDatas(group);
            if (achievementDatas[0].repeat != (long)ACHIEVEMENT_REPEAT.DAILY)
                continue;

            SetSlot(achievementDatas);
        }

        //일일업적 이외
        for (ACHIEVEMENT_GROUP group = ACHIEVEMENT_GROUP.START + 1; group < ACHIEVEMENT_GROUP.END; group++) {
            List<GameData.AchievementDTO> achievementDatas = AchievementUtil.GetSelectedAchievementDatas(group);
            if (achievementDatas[0].repeat == (long)ACHIEVEMENT_REPEAT.DAILY)
                continue;

            SetSlot(achievementDatas);
        }
    }

    private void SetSlot(List<GameData.AchievementDTO> achievementDatas) {
        GameObject go = Instantiate(prefabSlot, layoutGroup.transform);
        AchievementSlot slot = go.GetComponent<AchievementSlot>();
        slot.SetData(achievementDatas);
        Common.ToggleActive(go, true);
        listSlot.Add(slot);
    }

    public class SortByRepeat : IComparer<GameData.AchievementDTO> {
        public int Compare(GameData.AchievementDTO left, GameData.AchievementDTO right) {
            if (left.repeat == (long)ACHIEVEMENT_REPEAT.DAILY && 
                right.repeat != (long)ACHIEVEMENT_REPEAT.DAILY)
                return -1;
            else if (left.repeat != (long)ACHIEVEMENT_REPEAT.DAILY && 
                     right.repeat == (long)ACHIEVEMENT_REPEAT.DAILY)
                return 1;
            return 0;
        }
    }

    public void OnBtnHomeClick() {
        Hide();
    }

    public void OnBtnGetRewardsAllClick() {
        int count = 0;
        for (int i = 0; i < listSlot.Count; i++) {
            AchievementSlot slot = listSlot[i];
            if (slot.IsAvailableGetReward() == false)
                continue;
            
            AchievementSlot.REWARD_GET_STATE state = AchievementSlot.REWARD_GET_STATE.NONE;
            if (count == 0)
                state = AchievementSlot.REWARD_GET_STATE.MULTIPLE;
            slot.PlayAnim(state);
            count++;
        }
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }
}
