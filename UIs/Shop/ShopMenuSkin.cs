using System.Collections;
using System.Collections.Generic;
using GameData;
using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuSkin : ShopProduct {
    public Transform parentThumbnail;
    public Text lblName;
    public GameObject objLock;
    
    public Image icoApplied;
    public Text lblApplied;
    public GameObject objNotForSale;

    private GameObject objThumbnail;
    private SkinDTO skinData;

    public void SetData(SkinDTO skinData, bool dataOnly = false) {
        this.skinData = skinData;

        if (skinData.defaultUnlock == 0) {
            productData = GameDataModel.instance.GetProductDataBySkinID(skinData.skinID);
            bool skinExist = UserDataModel.instance.IsSkinExist(skinData.skinID);
            Common.ToggleActive(objLock, skinExist == false);
        }
        else
            Common.ToggleActive(objLock, false);

        if (dataOnly == false)
            objThumbnail = ResourceManager.instance.GetSkinThumbnail(skinData.skinID, parentThumbnail);
        lblName.text = TermModel.instance.GetSkinName(skinData.skinID);

        //구매가능한 스킨 && 구매하지 않은경우
        if (IsLock()) {
            if (skinData.buy == 0) {
                Common.ToggleActive(objNotForSale, true);
                Common.ToggleActive(lblCost.gameObject, false);
            }
            else {
                SetPrice();
                SetCostIco();
                Common.ToggleActive(lblCost.gameObject, true);
                Common.ToggleActive(objNotForSale, false);
            }

            Common.ToggleActive(icoApplied.gameObject, false);
            Common.ToggleActive(lblApplied.gameObject, false);
        }
        else {
            Common.ToggleActive(objNotForSale, false);
            Common.ToggleActive(icoApplied.gameObject, IsApplied());
            Common.ToggleActive(lblApplied.gameObject, IsApplied());
            Common.ToggleActive(lblCost.gameObject, false);
        }
    }

    private bool IsLock() {
        if (productData != null &&
            UserDataModel.instance.IsNonconsumableExist(productData.packageID) == false &&
            UserDataModel.instance.userProfile.skinIDs.Contains(skinData.skinID) == false)
            return true;
        return false;
    }

    private bool IsApplied() {
        if (UserDataModel.instance.SkinID == skinData.skinID)
            return true;
        return false;
    }

    public void OnBtnProductClick() {
        //구매하지 않은 스킨, 구매하기
        if (IsLock()) {
            if (skinData.buy == 0) {
                string msg = TermModel.instance.GetTerm("msg_not_for_sale");
                MessageUtil.ShowSimpleWarning(msg);
                return;
            }
            else
                EventManager.Notify(EventEnum.ShopMenuProductSelected, productData);
        }
        else 
            EventManager.Notify(EventEnum.ShopMenuSkinSelected, skinData);
    }
    public override void UpdateData() {
        SetData(skinData, true);
    }
}
