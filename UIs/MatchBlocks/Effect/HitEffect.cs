using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LuckyFlow.Event;

public class HitEffect : UIBase {
    public Animator animator;
    public TextMeshProUGUI lblHit;
    public TextMeshProUGUI lblScore;

    public void SetData(long hitCount, long score) {
        SetInFrontInCanvas();
        lblScore.text = Common.GetCommaFormat(score);
        lblHit.text = $"{hitCount.ToString()} Hit";
    }

    private void OnEnable() {
        float length = AnimationUtil.GetAnimationLength(animator, "Show");
        Invoke("NotifyEnd", length);
    }

    private void NotifyEnd() {
        EventManager.Notify(EventEnum.MatchBlocksEffectEnd);
    }
}
