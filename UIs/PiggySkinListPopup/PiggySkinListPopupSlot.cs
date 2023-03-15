using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggySkinListPopupSlot : MonoBehaviour {
    public Color notEnoughColor = Color.red;
    public Transform posPiggy;

    public Text lblName;
    public GameObject objApplied;
    public GameObject objNotForSale;
    public GameObject objBtnGoShop;
    public GameObject objCost;
    public Text lblCost;

    private GameData.PiggyBankSkinDTO piggySkinData;
    private GameData.ProductDTO productData;

    private Bank bank;

    private enum TRIGGER {
        None,
        Unable,
        Idle,
    }

    private TRIGGER trigger = TRIGGER.None;

    public void SetData(GameData.PiggyBankSkinDTO piggySkinData) {
        this.piggySkinData = piggySkinData;
        productData = GameDataModel.instance.GetProductDataByPackageID(piggySkinData.requirePackageID);

        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(UserDataModel.instance.userProfile.piggyBankType);

        if (bank == null) {
            GameObject piggyBankObject = ResourceManager.instance.GetPiggyBank(piggySkinData.bankSkinID, posPiggy);
            bank = piggyBankObject.GetComponent<Bank>();
            //bank.SetData(piggyBankData, CANVAS_ORDER.POPUP_40);
            Destroy(bank.canvasIco);
        }

        UpdateState(false);

        lblName.text = TermModel.instance.GetPiggyBankSkinName(piggySkinData.bankSkinID);
    }

    private void OnEnable() {
        if (bank == null)
            return;
        CheckTrigger();
    }

    public void UpdateState(bool checkTrigger) {
        if (IsLock()) {
            if (IsExpired() || piggySkinData.buy == (long)PIGGYBANK_SKIN_BUY_TYPE.NOT_FOR_SALE) {
                Common.ToggleActive(objNotForSale, true);
                Common.ToggleActive(objCost, false);
                Common.ToggleActive(objBtnGoShop, false);
            }
            else if (piggySkinData.buy == (long)PIGGYBANK_SKIN_BUY_TYPE.PACKAGE) {
                Common.ToggleActive(objNotForSale, false);
                Common.ToggleActive(objCost, false);
                Common.ToggleActive(objBtnGoShop, true);
            }
            else {
                SetPrice();
                Common.ToggleActive(objCost, true);
                Common.ToggleActive(objNotForSale, false);
                Common.ToggleActive(objBtnGoShop, false);
            }

            Common.ToggleActive(objApplied, false);
        }
        else {
            Common.ToggleActive(objNotForSale, false);
            Common.ToggleActive(objApplied, IsApplied());
            Common.ToggleActive(objCost, false);
            Common.ToggleActive(objBtnGoShop, false);
        }

        if (checkTrigger)
            CheckTrigger();
    }

    private void CheckTrigger() {
        trigger = TRIGGER.Idle;
        if (IsLock())
            trigger = TRIGGER.Unable;

        AnimationUtil.SetTrigger(bank.animator, trigger.ToString());
    }

    private void SetPrice() {
        lblCost.text = Common.GetCostFormat((PRODUCT_COST_TYPE)productData.costType,
                                            productData.cost,
                                            notEnoughColor);
    }

    private bool IsLock() {
        if (piggySkinData.defaultUnlock == 0 && piggySkinData.buy == 0) 
            return UserDataModel.instance.userProfile.bankSkinIDs.Contains(piggySkinData.bankSkinID) == false;
        
        if (productData != null &&
            UserDataModel.instance.IsNonconsumableExist(productData.packageID) == false &&
            UserDataModel.instance.userProfile.bankSkinIDs.Contains(piggySkinData.bankSkinID) == false)
            return true;

        return false;
    }

    private bool IsApplied() {
        if (UserDataModel.instance.BankSkinID == piggySkinData.bankSkinID)
            return true;
        return false;
    }

    private bool IsExpired() {
        //이벤트상품인데 구매날짜가 지난경우
        if (productData != null && 
            productData.endDate > 0 &&
            Common.GetUTCNow() > productData.endDate) 
            return true;
        return false;
    }

    public void OnBtnSlotClick() {
            //구매하지 않은 스킨, 구매하기
        if (IsLock()) {
            if (IsExpired() || piggySkinData.buy == (long)PIGGYBANK_SKIN_BUY_TYPE.NOT_FOR_SALE) {
                string msg = TermModel.instance.GetTerm("msg_not_for_sale");
                MessageUtil.ShowSimpleWarning(msg);
                return;
            }
            else if (piggySkinData.buy == (long)PIGGYBANK_SKIN_BUY_TYPE.BUY)
                ProductUtil.BuyProduct(productData);
            //상점으로 연결되는 상품은 별도 버튼에서 처리
        }
        else 
            WebUser.instance.ReqChangePiggyBankSkin(piggySkinData.bankSkinID);
    }

    public void OnBtnGoShopClick() {
        EventManager.Notify(EventEnum.PiggySkinListPopupGoShop);
    }
}
