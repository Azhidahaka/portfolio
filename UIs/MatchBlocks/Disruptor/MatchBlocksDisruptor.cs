using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBlocksDisruptor : MonoBehaviour {
    public Animator animator;

    protected SpaceDTO spaceInfo;

    public virtual void SetData(SpaceDTO spaceInfo) {
        this.spaceInfo = spaceInfo;
    }

    public virtual void Remove(bool instant = false) {
        if (instant)
            Destroy();
        else {
            animator.SetTrigger("Destroy");
            float length = AnimationUtil.GetAnimationLength(animator, "Destroy");
            Invoke("Destroy", length);
        }
    }

    protected virtual void Destroy() {
        Destroy(gameObject);
    }

    public virtual void UpdateState() {

    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
    }

    public virtual void Show() {
        Common.ToggleActive(gameObject, true);
    }
}
