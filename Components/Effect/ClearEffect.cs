using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearEffect : MatchBlocksEffect {
    public RawImage ico;

    private new void OnEnable() {
        base.OnEnable();
    }

    public void SetData(int blockTextureIndex, 
                        Vector2 sizeDelta, 
                        float boardScale, 
                        float duration = 0,
                        bool realtime = false) {
        SetData(sizeDelta, duration, realtime);

        if (ico != null)
            ico.texture = ResourceManager.instance.GetBlockTexture(blockTextureIndex);

        rectTransform.sizeDelta = rectTransform.sizeDelta * boardScale;
    }

    public new void Hide() {
        Common.ToggleActive(gameObject, false);
        transform.SetParent(EffectManager.instance.archive);
    }
}
