using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonGoods : MonoBehaviour {
    public Text lblDiamond;
    public Text lblGold;

    public LayoutGroup layoutGroup;

    private bool inMatchBlocks;

    private void OnEnable() {
        EventManager.Register(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);
    }

    private void OnGoodsUpdate(object[] args) {
        SetData(inMatchBlocks);
        StartCoroutine(Reposition());
    }

    private IEnumerator Reposition() {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);

        Common.ToggleActive(layoutGroup.gameObject, false);
        Common.ToggleActive(layoutGroup.gameObject, true);
    }

    public void SetData(bool inMatchBlocks = false) {
        this.inMatchBlocks = inMatchBlocks;
        if (inMatchBlocks) {
            long availableGold = MatchBlocksReferee.instance.AvailableGold;
            long availableGoldMax = MatchBlocksReferee.instance.AvailableGoldMax;
            if (UserDataModel.instance.userProfile.gold < availableGold)
                availableGold = UserDataModel.instance.userProfile.gold;
            lblGold.text = $"{Common.GetCommaFormat(availableGold)}/{Common.GetCommaFormat(availableGoldMax)}";
        }
        else {
            lblDiamond.text = Common.GetCommaFormat(UserDataModel.instance.userProfile.diamond);
            lblGold.text = Common.GetCommaFormat(UserDataModel.instance.userProfile.gold);
        }
    }

    public void OnBtnGoldClick() {
        ShopPopup shopPopup = UIManager.instance.GetUI<ShopPopup>(UI_NAME.ShopPopup);
        shopPopup.SetData(LuckyFlow.EnumDefine.PRODUCT_COST_TYPE.GOLD);
        shopPopup.Show();
    }

    public void OnBtnDiamondClick() {
        ShopPopup shopPopup = UIManager.instance.GetUI<ShopPopup>(UI_NAME.ShopPopup);
        shopPopup.SetData(LuckyFlow.EnumDefine.PRODUCT_COST_TYPE.DIAMOND);
        shopPopup.Show();
    }
}
