using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggyBankPopup : UIBase {
    public CommonGoods commonGoods;
    public Text lblTitle;

    public Slider sldGold;

    public GameObject objBtnWithdrawIco;
    public Text lblBtnWithdraw;

    public Text lblCollected;
    public Text lblCapacity;
    public LayoutGroup layoutGroup;

    public GameObject objBtnUpgrade;
    public GameObject objBtnSkin;
    public Text lblGrade;

    public Transform posPiggy;

    private GameData.PiggyBankDTO piggyBankData;
    private PIG_STATE state = PIG_STATE.None;

    private Bank bank;

    private string formatGold;

    public void SetData(GameData.PiggyBankDTO piggyBankData) {
        formatGold = TermModel.instance.GetTerm("format_gold");

        this.piggyBankData = piggyBankData;
        commonGoods.SetData();

        if (piggyBankData.showAds == 0) {
            Common.ToggleActive(objBtnWithdrawIco, false);
            lblBtnWithdraw.text = TermModel.instance.GetTerm("btn_withdraw");
        }
        else {
            Common.ToggleActive(objBtnWithdrawIco, true);
            lblBtnWithdraw.text = TermModel.instance.GetTerm("btn_withdraw_ads");
        }

        UpdateGold();

        if (bank != null)
            Destroy(bank.gameObject);
        GameObject piggyBankObject = ResourceManager.instance.GetPiggyBank(UserDataModel.instance.BankSkinID, posPiggy);
        bank = piggyBankObject.GetComponent<Bank>();
        bank.SetData(piggyBankData, CANVAS_ORDER.POPUP_40);

        DetermineShowBtns();

        string format = TermModel.instance.GetTerm("format_piggy_grade");
        lblGrade.text = string.Format(format, piggyBankData.type);
    }

    private void DetermineShowBtns() {
        if (App.instance.GetSceneName() == App.SCENE_NAME.MatchBlocks) {
            Common.ToggleActive(objBtnUpgrade, false);
            Common.ToggleActive(objBtnSkin, false);
        }
        else if (piggyBankData.type == (long)PIGGY_BANK_TYPE.CASH) {
            Common.ToggleActive(objBtnUpgrade, false);
            Common.ToggleActive(objBtnSkin, true);
        }
        else {
            Common.ToggleActive(objBtnUpgrade, true);
            Common.ToggleActive(objBtnSkin, true);
        }
    }

    private void OnEnable() {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);

        Common.ToggleActive(layoutGroup.gameObject, false);
        Common.ToggleActive(layoutGroup.gameObject, true);

        EventManager.Register(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.BankSkinChanged, OnGoodsUpdate);
        EventManager.Register(EventEnum.WithdrawPiggyGoldComplete, OnWithdrawPiggyGoldComplete);

        if (piggyBankData == null)
            return;

        StopAllCoroutines();
        StartCoroutine(JobCheckGold());

        PauseTime();

        bank.Show();
    }

    private void OnWithdrawPiggyGoldComplete(object[] args) {
        bank.ShowGetGoldAnim();
    }

    private void OnDisable() {
        ResumeTime();

        EventManager.Remove(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Remove(EventEnum.BankSkinChanged, OnGoodsUpdate);
        EventManager.Remove(EventEnum.WithdrawPiggyGoldComplete, OnWithdrawPiggyGoldComplete);
    }

    private void OnGoodsUpdate(object[] args) {
        SetData(piggyBankData);
    }

    private IEnumerator JobCheckGold() {
        while(true) {
            /*
            PIG_STATE newState = GetNewState();
            if (state != newState) {
                state = newState;
                AnimationUtil.SetTrigger(animator, state.ToString());
            }*/
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);

            Common.ToggleActive(layoutGroup.gameObject, false);
            Common.ToggleActive(layoutGroup.gameObject, true);

            yield return new WaitForSecondsRealtime(1.0f);
        }       
    }

    private void UpdateGold() {
        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        long collectedLong = (long)collectedGold;

        lblCollected.text = string.Format(formatGold, Common.GetCommaFormat(collectedLong));
        lblCapacity.text = string.Format(formatGold, Common.GetCommaFormat(piggyBankData.capacity));
        sldGold.value = (float)collectedLong / piggyBankData.capacity;
    }

    public void OnBtnWithdrawClick() {
        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        if (collectedGold < 
            piggyBankData.capacity * (Constant.MIN_DEBIT_GOLD_PERCENT / 100)) {
            //@todo 메세지;
            string format = TermModel.instance.GetTerm("format_withdraw_warning");
            string msg = string.Format(format, Constant.MIN_DEBIT_GOLD_PERCENT);
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.REWARDED_VIDEO);
        Callback callback = () => {
            EventManager.Notify(EventEnum.MatchBlocksBtnPiggyBankClick);
        };

        if (UserDataModel.instance.userProfile.piggyBankType != (long)PIGGY_BANK_TYPE.CASH &&
            adsInstance != null && 
            adsInstance.IsRewardedVideoLoaded()) {
            adsInstance.ShowRewardedAd(false, 
                                       () => {
                                            AnalyticsUtil.LogAdImpressionPiggy();
                                            callback();
                                        });
        }
        else
            callback();
    }

    public void OnBtnHideClick() {
        Hide();
    }

    public void OnBtnUpgradeClick() {
        Hide();
        ShopMenu shopMenu = UIManager.instance.GetUI<ShopMenu>(UI_NAME.ShopMenu);
        shopMenu.SetData(PRODUCT_CATEGORY.GOODS);
        shopMenu.Show();
    }

    public void OnBtnSkinClick() {
        Hide();
        PiggySkinListPopup skinPopup = UIManager.instance.GetUI<PiggySkinListPopup>(UI_NAME.PiggySkinListPopup);
        skinPopup.SetData();
        skinPopup.Show();
    }
}
