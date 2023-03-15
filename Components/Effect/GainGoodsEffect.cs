using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GainGoodsEffect : MatchBlocksEffect {
    public ParticleSystem ps;
    public TextMeshProUGUI lblAmount;

    public void SetData(long amount, float duration) {
        SetData(rectTransform.sizeDelta, duration, true);

        ParticleSystem.MainModule mainModule = ps.main;
        if (amount < Constant.MAX_GAIN_GOODS_PARTICLES) 
            mainModule.maxParticles = (int)amount;
        else 
            mainModule.maxParticles = (int)Constant.MAX_GAIN_GOODS_PARTICLES;

        lblAmount.text = Common.GetAddCountFormat(amount);
    }
}
