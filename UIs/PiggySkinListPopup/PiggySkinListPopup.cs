using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggySkinListPopup : UIBase {
    public LayoutGroup layoutGroup;
    public GameObject prefabSlot;

    private List<PiggySkinListPopupSlot> listItem = new List<PiggySkinListPopupSlot>();
    private List<PiggySkinListPopupSlot> listReusableItem = new List<PiggySkinListPopupSlot>();

    private void Awake() {
        Common.ToggleActive(prefabSlot, false);
    }
    private void OnEnable() {
        SetInFrontInCanvas();

        EventManager.Register(EventEnum.BankSkinChanged, OnBankSkinChanged);
        EventManager.Register(EventEnum.PiggySkinListPopupGoShop, OnPiggySkinListPopupGoShop);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);   
    }

    private void OnUserDataUserInfoUpdate(object[] args) {
        foreach(PiggySkinListPopupSlot item in listItem) {
            item.UpdateState(true);
        }
    }

    private void OnPiggySkinListPopupGoShop(object[] args) {
        Hide();
        ShopMenu shopMenu = UIManager.instance.GetUI<ShopMenu>(UI_NAME.ShopMenu);
        shopMenu.SetData(PRODUCT_CATEGORY.GOODS);
        shopMenu.Show();
    }

    private void OnBankSkinChanged(object[] args) {
        foreach(PiggySkinListPopupSlot item in listItem) {
            item.UpdateState(true);
        }
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.BankSkinChanged, OnBankSkinChanged);
        EventManager.Remove(EventEnum.PiggySkinListPopupGoShop, OnPiggySkinListPopupGoShop);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
    }

    public void SetData() {
        SetScrollVIew();
    }

    private void SetScrollVIew() {
        if (listItem == null) {
            listItem = new List<PiggySkinListPopupSlot>();
            listReusableItem = new List<PiggySkinListPopupSlot>();
        }
        else {
            foreach (PiggySkinListPopupSlot scrollViewItem in listItem) {
                if (listReusableItem.Contains(scrollViewItem) == false)
                    listReusableItem.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listItem.Clear();
        }

        GameObject scrollViewGO;
        List<GameData.PiggyBankSkinDTO> piggyBankSkinDatas = GameDataModel.instance.piggyBankSkins;

        for (int i = 0; i < piggyBankSkinDatas.Count; i++) {
            GameData.PiggyBankSkinDTO skinData = piggyBankSkinDatas[i];

            PiggySkinListPopupSlot scrollViewItem;

            if (i >= listReusableItem.Count) {
                scrollViewGO =  Instantiate(prefabSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<PiggySkinListPopupSlot>();
                listReusableItem.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableItem[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<PiggySkinListPopupSlot>();
            scrollViewItem.SetData(skinData);
            listItem.Add(scrollViewItem);
        }
    }

    public void OnBtnCloseClick() {
        Hide();
        PiggyBankPopup piggyBankPopup = UIManager.instance.GetUI<PiggyBankPopup>(UI_NAME.PiggyBankPopup);
        long piggyBankType = UserDataModel.instance.userProfile.piggyBankType;
        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(piggyBankType);
        piggyBankPopup.SetData(piggyBankData);
        piggyBankPopup.Show();
    }
}
