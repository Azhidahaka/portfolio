using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementSlot : MonoBehaviour {
    public enum REWARD_GET_STATE {
        NONE = 0,
        SINGLE = 1,
        MULTIPLE = 2,
    }

    public GameObject objReceived;
    public Text lblReceivedRewardAmount;
    public RawImage icoReceived;

    public GameObject objComplete;
    public Text lblCompleteRewardAmount;
    public RawImage icoComplete;
    public Slider sldComplete;

    public GameObject objNormal;
    public Text lblNormalRewardAmount;
    public RawImage icoNormal;
    public Slider sldNormal;

    public Text lblDesc;

    public Animator animator;

    public RawImage icoRepeat;
    public GameObject objDailyRepeat;
    public Image imgRemainTime;

    private List<GameData.AchievementDTO> achievementDatas;
    private long current;
    private bool cleared;
    private bool playAnim = false;

    private REWARD_GET_STATE rewardGetState = REWARD_GET_STATE.NONE;
    private void OnEnable() {
        EventManager.Register(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);

        StartCoroutine(JobCheckTime());
    }

    private IEnumerator JobCheckTime() {
        while (true) {
            if (achievementDatas != null &&
                achievementDatas.Count > 0 &&
                achievementDatas[0].repeat == (long)ACHIEVEMENT_REPEAT.DAILY) {
                UpdateRemainTime();
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void UpdateRemainTime() {
        float remainTime = Common.GetUTCTodayZero() + 86400 - Common.GetUTCNow();
        imgRemainTime.fillAmount = remainTime / 86400;
    }

    private void OnUpdateAchievementCleared(object[] args) {
        long achievementID = (long)args[0];
        if (achievementID != achievementDatas[0].achievementID)
            return;

        SetData(achievementDatas);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);
    }

    public void SetData(List<GameData.AchievementDTO> achievementDatas) {
        this.achievementDatas = achievementDatas;
        
        cleared = UserDataModel.instance.statistics.clearedAchievementIDs.Contains(achievementDatas[0].achievementID) ||
                  AchievementUtil.IsCleared(achievementDatas);

        List<long> receivedIDs = UserDataModel.instance.statistics.receivedAchievementIDs;
        //보상을 이미 수령한경우
        if (receivedIDs.Contains(achievementDatas[0].achievementID))
            SetReceived();
        else if (cleared) 
            SetComplete();
        else
            SetNormal();

        lblDesc.text = TermModel.instance.GetAchievementDesc(achievementDatas);
        if (achievementDatas[0].repeat == (long)ACHIEVEMENT_REPEAT.NONE) {
            Common.ToggleActive(icoRepeat.gameObject, false);
            Common.ToggleActive(objDailyRepeat, false);
        }
        else if (achievementDatas[0].repeat == (long)ACHIEVEMENT_REPEAT.DAILY) {
            Common.ToggleActive(objDailyRepeat, true);
            Common.ToggleActive(icoRepeat.gameObject, false);
        }
        else {
            //Common.ToggleActive(icoRepeat.gameObject, true);  //요청으로 숨김
            Common.ToggleActive(objDailyRepeat, false);
        }
    }

    private void SetNormal() {
        Common.ToggleActive(objNormal, true);
        Common.ToggleActive(objComplete, false);
        Common.ToggleActive(objReceived, false);

        icoNormal.texture = ResourceManager.instance.GetAchievementRewardIco(achievementDatas[0].rewardType);
        lblNormalRewardAmount.text = $"x {Common.GetCommaFormat(achievementDatas[0].rewardValue)}";

        //'아이템 없이 점수 획득' 업적에서 아이템을 한번이라도 사용 한 경우 : 0
        if (achievementDatas[0].group == (long)ACHIEVEMENT_GROUP.EARNED_SCORE_IN_ONE_ROUND_WITHOUT_ITEM_USE &&
            UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ROUND_ITEM_USECOUNT) > 0) {
            sldNormal.value = 0;
        }
        else
            sldNormal.value = (float)AchievementUtil.GetCurrentValue(achievementDatas[0]) / AchievementUtil.GetComparingValue(achievementDatas[0]);
    }

    private void SetComplete() {
        Common.ToggleActive(objComplete, true);
        Common.ToggleActive(objNormal, false);
        Common.ToggleActive(objReceived, false);

        icoComplete.texture = ResourceManager.instance.GetAchievementRewardIco(achievementDatas[0].rewardType);
        lblCompleteRewardAmount.text = $"x {Common.GetCommaFormat(achievementDatas[0].rewardValue)}";
        sldComplete.value = sldComplete.maxValue;
    }

    private void SetReceived() {
        Common.ToggleActive(objReceived, true);
        Common.ToggleActive(objNormal, false);
        Common.ToggleActive(objComplete, false);

        icoReceived.texture = ResourceManager.instance.GetAchievementRewardIco(achievementDatas[0].rewardType);
        lblReceivedRewardAmount.text = $"x {Common.GetCommaFormat(achievementDatas[0].rewardValue)}";
    }

    public void OnBtnSlotClick() {
        PlayAnim(REWARD_GET_STATE.SINGLE);
    }

    public void PlayAnim(REWARD_GET_STATE rewardGetState) {
        this.rewardGetState = rewardGetState;

        if (IsAvailableGetReward() == false) 
            return;

        playAnim = true;
        AnimationUtil.SetTrigger(animator, "Gain");
    }

    public bool IsAvailableGetReward() {
        if (cleared == false ||
            UserDataModel.instance.statistics.receivedAchievementIDs.Contains(achievementDatas[0].achievementID) ||
            playAnim) 
            return false;
        return true;
    }

    public void AnimFinished() {
        if (playAnim == false)
            return;
        playAnim = false;

        Callback callback = () => {
            rewardGetState = REWARD_GET_STATE.NONE;
            achievementDatas = AchievementUtil.GetSelectedAchievementDatas((ACHIEVEMENT_GROUP)achievementDatas[0].group);
            SetData(achievementDatas);
        };

        if (rewardGetState == REWARD_GET_STATE.SINGLE) 
            WebUser.instance.ReqGetAchievementReward(achievementDatas, callback);
        else if (rewardGetState == REWARD_GET_STATE.MULTIPLE) 
            WebUser.instance.ReqGetAchievementRewardAll();
    }
}
