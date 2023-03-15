using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksIce : MatchBlocksDisruptor {
    public enum TRIGGER {
        Grade1,
        Hit,
        Show,
        Hide,
    }

    //public RawImage imgOutline;

    public override void SetData(SpaceDTO spaceInfo) {
        /*
        float[] angles = new float[]{90, 180, 270, 0};
        int index = Random.Range(0, angles.Length);
        float angle = angles[index];

        imgOutline.transform.localEulerAngles = new Vector3(0, 0, angle);
        */
        base.SetData(spaceInfo);
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
        StopAllCoroutines();
        StartCoroutine(JobShowAnim(TRIGGER.Show));
    }

    private IEnumerator JobShowAnim(TRIGGER trigger, bool hide = false) {
        SetTrigger(trigger);
        float showLength = AnimationUtil.GetAnimationLength(animator, trigger.ToString());
        yield return new WaitForSeconds(showLength);
        if (hide) 
            Hide();
        else 
            SetTrigger(TRIGGER.Grade1);
    }

    public override void Remove(bool instant = false) {
        if (instant || gameObject.activeSelf == false)
            Hide();
        else 
            StartCoroutine(JobShowAnim(TRIGGER.Hide, true));
    }

    public override void Show() {
        //StopAllCoroutines();
        if (gameObject.activeSelf)
            OnEnable();
        else
            gameObject.SetActive(true);
    }
}

