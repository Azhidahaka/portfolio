using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementNoticePopup : UIBase {
    public Animator animator;

    public Queue<long> queue = new Queue<long>();
    public Text lblAchievenemtDesc;
    public RawImage icoReward;
    public Text lblRewardAmount;

    private bool animFinished = true;
    private long achievementID;

    private void Awake() {
        AnimCallbackLinker linker = animator.GetComponent<AnimCallbackLinker>();
        linker.SetCallback(OnAnimFinished);
    }

    public void SetData(long achievementID) {
        this.achievementID = achievementID;
        if (queue.Contains(achievementID) == false)
            queue.Enqueue(achievementID);
    }

    private void OnEnable() {
        if (queue.Count == 0)
            return;

        StartCoroutine(JobCheckAnimation());
    }

    private IEnumerator JobCheckAnimation() {
        do {
            animFinished = false;
            long achievementID = queue.Dequeue();
            SetAchievementInfo(achievementID);
            AnimationUtil.SetTrigger(animator, "Show");
            yield return new WaitUntil(() => animFinished);
        } while(queue.Count > 0);

        Hide();
    }

    private void SetAchievementInfo(long achievementID) {
        List<GameData.AchievementDTO> achieventDatas = GameDataModel.instance.GetAchievementDatasByID(achievementID);
        string format = TermModel.instance.GetTerm("format_achievement_success");
        string desc = TermModel.instance.GetAchievementDesc(achieventDatas);
        lblAchievenemtDesc.text = string.Format(format, desc);
        lblRewardAmount.text = $"x{Common.GetCommaFormat(achieventDatas[0].rewardValue)}";
        icoReward.texture = ResourceManager.instance.GetAchievementRewardIco(achieventDatas[0].rewardType);
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    public void OnAnimFinished() {
        animFinished = true;
    }

    public override void OnCopy(List<object> datas) {
        achievementID = (long)datas[0];
        SetData(achievementID);
    }

    public override List<object> GetCopyDatas() {
        List<object> datas = new List<object>();
        datas.Add(achievementID);
        return datas;
    }
}
