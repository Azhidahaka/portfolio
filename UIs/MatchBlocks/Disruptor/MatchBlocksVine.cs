using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksVine : MatchBlocksDisruptor {
    public enum TRIGGER {
        Grade1,
        Grade2,
        Show,
        Hide,
    }

    public override void SetData(SpaceDTO spaceInfo) {
        base.SetData(spaceInfo);
    }

    private void DetermineTrigger() {
        if (spaceInfo.disruptorInfo.stem) 
            SetTrigger(TRIGGER.Grade1);
        else
            SetTrigger(TRIGGER.Grade2);
    }

    private void ResetTriggers() {
        for (TRIGGER trigger = TRIGGER.Grade1; trigger <= TRIGGER.Hide; trigger++) {
            animator.ResetTrigger(trigger.ToString());
        }
    }

    public override void Remove(bool instant = false) {
        ResetTriggers();

        if (instant)
            Destroy();
        else {
            StartCoroutine(JobShowAnim(TRIGGER.Hide, true));
        }
    }

    private void SetTrigger(TRIGGER trigger) {
        ResetTriggers();
        string name = trigger.ToString();
        animator.SetTrigger(name);
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

    public override void UpdateState() {
        DetermineTrigger();
    }
}
