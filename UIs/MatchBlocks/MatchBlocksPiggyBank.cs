using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksPiggyBank : MonoBehaviour {
    private GameData.PiggyBankDTO piggyBankData;
    private Bank bank;

    private void OnEnable() {
        EventManager.Register(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.BankSkinChanged, OnBankSkinChanged);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnBankSkinChanged);

        if (bank == null)
            return;

        bank.Show();
    }

    private void OnBankSkinChanged(object[] args) {
        SetData();
        bank.Show();
    }

    private void OnGoodsUpdate(object[] args) {
        SetData();
        bank.Show();
    }

    public PIG_STATE GetNewState() {
        PIG_STATE newState;
        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        //최소 골드량을 만족하지 못함. Unable
        if (collectedGold < 
            piggyBankData.capacity * (Constant.MIN_DEBIT_GOLD_PERCENT / 100)) {
            newState = PIG_STATE.Unable;
            //icoPiggyBank.material = MaterialManager.instance.GetGray();
        }
        else if (collectedGold >= piggyBankData.capacity) {
            newState = PIG_STATE.Full;
            //icoPiggyBank.material = null;
        }
        else {
            newState = PIG_STATE.Idle;
            //icoPiggyBank.material = null;
        }

        return newState;
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Remove(EventEnum.BankSkinChanged, OnBankSkinChanged);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnBankSkinChanged);
        StopAllCoroutines();
    }

    public void SetData() {
        long piggyBankType = UserDataModel.instance.userProfile.piggyBankType;
        piggyBankData = GameDataModel.instance.GetPiggyBankData(piggyBankType);

        if (bank != null)
            Destroy(bank.gameObject);
        GameObject piggyBankObject = ResourceManager.instance.GetPiggyBank(UserDataModel.instance.BankSkinID, transform);
        bank = piggyBankObject.GetComponent<Bank>();
        bank.SetData(piggyBankData, CANVAS_ORDER.BASE_10);
    }

    public void OnBtnPigClick() {
        PiggyBankPopup piggyBankPopup = UIManager.instance.GetUI<PiggyBankPopup>(UI_NAME.PiggyBankPopup);
        piggyBankPopup.SetData(piggyBankData);
        piggyBankPopup.Show();
    }
}
