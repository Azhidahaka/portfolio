using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {
    public static EffectManager instance;

    public Transform archive;

    private Dictionary<EFFECT_NAME, List<MatchBlocksEffect>> dicEffectList = new Dictionary<EFFECT_NAME, List<MatchBlocksEffect>>();

    private List<EFFECT_NAME> popupDecoEffects = new List<EFFECT_NAME>() {
        EFFECT_NAME.vfxGetDiamond,
        EFFECT_NAME.vfxGetGold,
        EFFECT_NAME.vfxGetSkin,
        EFFECT_NAME.vfxGetItem
    };

    private void Awake() {
        instance = this;
    }

    public void SetData() {
        dicEffectList.Clear();
    }

    public MatchBlocksEffect LoadEffect(EFFECT_NAME effectName, Transform parent = null) {
        if (parent == null) {
            if (popupDecoEffects.Contains(effectName)) 
                parent = UIManager.instance.GetCanvasTransform(CANVAS_ORDER.POPUP_DECO_45);
            else
                parent = UIManager.instance.GetCanvasTransform(CANVAS_ORDER.DECO_30);
        }

        if (dicEffectList.ContainsKey(effectName)) {
            List<MatchBlocksEffect> list = dicEffectList[effectName];
            foreach(MatchBlocksEffect element in list) {
                if (element == null || element.gameObject.activeSelf)
                    continue;

                element.transform.SetParent(parent);
                element.transform.localPosition = Vector3.zero;
                return element;
            }

            GameObject effectObj = ResourceManager.instance.LoadEffect(effectName, parent);
            MatchBlocksEffect effect = effectObj.GetComponent<MatchBlocksEffect>();
            list.Add(effect);
            return effect;
        }
        else {
            List<MatchBlocksEffect> list = new List<MatchBlocksEffect>();
            GameObject effectObj = ResourceManager.instance.LoadEffect(effectName, parent);
            MatchBlocksEffect effect = effectObj.GetComponent<MatchBlocksEffect>();
            list.Add(effect);
            dicEffectList.Add(effectName, list);

            return effect;
        }
    }

    public void ShowGetGoldEffect(long amount) {
        GainGoodsEffect effect = (GainGoodsEffect)LoadEffect(EFFECT_NAME.vfxGetGold);
        effect.SetData(amount, 1.0f);
        effect.Show();
    }

    public void ShowGetDiamondEffect(long amount) {
        GainGoodsEffect effect = (GainGoodsEffect)LoadEffect(EFFECT_NAME.vfxGetDiamond);
        effect.SetData(amount, 1.0f);
        effect.Show();
    }

    public void ShowGetMultipleGoodsEffect(long goldAmount, long diamondAmount) {
        GainMultipleGoodsEffect effect = (GainMultipleGoodsEffect)LoadEffect(EFFECT_NAME.vfxGetGoods);
        effect.SetData(goldAmount, diamondAmount, 1.0f);
        effect.Show();
    }

    public void ShowGetSkinEffect(long skinID) {
        GainSkinEffect effect = (GainSkinEffect)LoadEffect(EFFECT_NAME.vfxGetSkin);
        effect.SetData(skinID);
        effect.Show();
    }

    public void ShowGetItemEffect(long itemID, long itemCount, bool isTicket = false) {
        GainItemEffect effect = (GainItemEffect)LoadEffect(EFFECT_NAME.vfxGetItem);
        effect.SetData(itemID, itemCount, isTicket);
        effect.Show();
    }
}
