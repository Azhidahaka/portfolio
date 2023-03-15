using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GainMultipleGoodsEffect : MatchBlocksEffect {
    public ParticleSystem psGold;
    public ParticleSystem psDiamond;
    public TextMeshProUGUI lblGoldAmount;
    public TextMeshProUGUI lblDimondAmount;

    public void SetData(long goldAmount, long diamondAmount, float duration) {
        SetData(rectTransform.sizeDelta, duration, true);

        SetGoods(lblGoldAmount, psGold, goldAmount);
        SetGoods(lblDimondAmount, psDiamond, diamondAmount);
    }

    private void SetGoods(TextMeshProUGUI lblAmount, ParticleSystem ps, long amount) {
        ParticleSystem.MainModule mainModule = ps.main;
        if (amount < Constant.MAX_GAIN_GOODS_PARTICLES) 
            mainModule.maxParticles = (int)amount;
        else 
            mainModule.maxParticles = (int)Constant.MAX_GAIN_GOODS_PARTICLES;

        lblAmount.text = Common.GetAddCountFormat(amount);
    }
}
