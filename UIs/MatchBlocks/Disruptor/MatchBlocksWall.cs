using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksWall : MatchBlocksDisruptor {
    public Text lblHP;

    public enum TRIGGER {
        Grade1,
        Grade2,
        Grade3,
        Grade4,
        Hit,
        Show,
        Hide,
    }

    public override void SetData(SpaceDTO spaceInfo) {
        base.SetData(spaceInfo);
        lblHP.text = spaceInfo.disruptorInfo.hp.ToString();
    }

    public override void Remove(bool instant = false) {
        if (spaceInfo.disruptorInfo.hp > 0) {
            if (instant || gameObject.activeSelf == false)
                Destroy();
            else
                StartCoroutine(JobShowAnim(TRIGGER.Hit));
            return;
        }

        if (instant)
            Destroy();
        else {
            StartCoroutine(JobShowAnim(TRIGGER.Hide, true));
        }
    }

    private void DetermineTrigger() {
        if (spaceInfo.disruptorInfo.hp > 0) {
            long hpNum = spaceInfo.disruptorInfo.hp;
            if (hpNum > Constant.WALL_HP_MAX) {
                hpNum = Constant.WALL_HP_MAX;
            }
            lblHP.text = hpNum.ToString();
            TRIGGER trigger = Common.ToEnum<TRIGGER>($"Grade{(Constant.WALL_HP_MAX - hpNum).ToString()}");
            SetTrigger(trigger);
        }
    }

    private void SetTrigger(TRIGGER trigger) {
        ResetTriggers();
        string name = trigger.ToString();
        animator.SetTrigger(name);
    }

    private void ResetTriggers() {
        for (TRIGGER trigger = TRIGGER.Grade1;
            trigger <= TRIGGER.Show; 
            trigger++) {
            animator.ResetTrigger(trigger.ToString());
        }
    }

    private void OnEnable() {
        StartCoroutine(JobShowAnim(TRIGGER.Show));
    }

    private IEnumerator JobShowAnim(TRIGGER trigger, bool hide = false) {
        SetTrigger(trigger);
        float showLength = AnimationUtil.GetAnimationLength(animator, trigger.ToString());
        yield return new WaitForSeconds(showLength);
        if (hide) 
            Destroy();
        else 
            DetermineTrigger();
    }
}
