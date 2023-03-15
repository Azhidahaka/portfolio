using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBlocksEffect : MonoBehaviour {
    public Animator animator;
    public RectTransform rectTransform;

    private float duration;
    private bool realtime;

    public void SetData(Vector2 targetSizeDelta, float duration = 0, bool realtime = false) {
        this.duration = duration;
        this.realtime = realtime;

        rectTransform.sizeDelta = targetSizeDelta;
    }

    protected virtual void OnEnable() {
        CancelInvoke("Hide");
        if (animator != null) {
            if (duration == 0) 
                duration = AnimationUtil.GetAnimationLength(animator, "Show");
            StartCoroutine(JobDelayHide());
        }
        else {
            if (duration > 0)
                StartCoroutine(JobDelayHide());
        }
    }

    private IEnumerator JobDelayHide() {
        if (realtime)
            yield return new WaitForSecondsRealtime(duration);
        else
            yield return new WaitForSeconds(duration);
        Hide();
    }

    public void Show() {
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);
        else
            OnEnable();
    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
        transform.SetParent(EffectManager.instance.archive);
    }

    public void SetDuration(float duration) {
        this.duration = duration;
    }

    public void SetRealTime(bool realtime) {
        this.realtime = realtime;
    }
}
 