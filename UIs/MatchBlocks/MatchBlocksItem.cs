using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksItem : MonoBehaviour {
    public Color notEnoughColor = Color.red;
    public RawImage icoItem;
    
    public Text lblCost;

    private long itemID;
    private GameData.ItemDTO itemData;

    public void SetData(long itemID) {
        this.itemID = itemID;
        icoItem.texture = ResourceManager.instance.GetIcoItemTexture(itemID);
        itemData = GameDataModel.instance.GetItemData(itemID);
        UpdateCost();
    }

    public void UpdateCost() {
        lblCost.text = Common.GetCostFormat(MatchBlocksReferee.instance.GetRefereeNote().availableGold, itemData.price, notEnoughColor);
    }

    public void OnBtnClick() {
        if (UserDataModel.instance.userProfile.gold < itemData.price) {
            MessageUtil.ShowShopPopupConfirm(PRODUCT_COST_TYPE.GOLD);
            return;
        }

        if (MatchBlocksReferee.instance.AvailableGold < itemData.price) {
            string msg = TermModel.instance.GetTerm("msg_available_gold_not_enough");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        EventManager.Notify(EventEnum.MatchBlocksUseItem, itemID);
    }

    public GameData.ItemDTO GetItemData() {
        return itemData;
    }
}
