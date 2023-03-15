using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GainScoreEffect : MatchBlocksEffect {
    public Text txtScore;

    private new void OnEnable() {
        base.OnEnable();
    }

    public void SetData(Vector2 sizeDelta, 
                        float boardScale,
                        long score,
                        float duration = 0) {
        SetData(sizeDelta, duration);

        txtScore.text = Common.GetAddCountFormat(score);

        rectTransform.sizeDelta = rectTransform.sizeDelta * boardScale;
    }

    public new void Hide() {
        Common.ToggleActive(gameObject, false);
        transform.SetParent(EffectManager.instance.archive);
    }
}
