using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUtil {
    public static void ShowSimpleWarning(string msg, bool showBg = true) {
        SimpleWarningPopup popup = UIManager.instance.GetUI<SimpleWarningPopup>(UI_NAME.SimpleWarningPopup);
        popup.SetData(msg, showBg);
        popup.Show();
    }

    public static void ShowNotImplementedMsg() {
        SimpleWarningPopup popup = UIManager.instance.GetUI<SimpleWarningPopup>(UI_NAME.SimpleWarningPopup);
        string msg = TermModel.instance.GetTerm("msg_implementing");
        popup.SetData(msg);
        popup.Show();
    }

    public static void ShowWarning(CommonPopup.BUTTON_TYPE type,
                                   string message,
                                   Callback callbackOk = null, 
                                   Callback callbackCancel = null,
                                   Callback callbackModify = null,
                                   string textOk = "",
                                   string textCancel = "",
                                   string textModify = "",
                                   bool showGuestWarning = false,
                                   Texture texture = null) {
        CommonPopup commonPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.CommonPopup);
        commonPopup.SetData(type, 
                            message, 
                            callbackOk, 
                            callbackCancel, 
                            callbackModify,
                            textOk,
                            textCancel,
                            textModify,
                            showGuestWarning,
                            texture);
        commonPopup.Show();
    }


    public static void ShowNetworkWarning(Callback callbackOk = null) {
        string message = TermModel.instance.GetTerm("msg_network_error");
        string textOk = TermModel.instance.GetTerm("btn_retry");
        string textCancel = TermModel.instance.GetTerm("btn_quit");
        Callback callbackCancel = () => {
            Application.Quit();
        };

        CommonPopup commonPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.CommonPopup);
        commonPopup.SetData(CommonPopup.BUTTON_TYPE.YES_NO, 
                            message, 
                            callbackOk, 
                            callbackCancel, 
                            null,
                            textOk,
                            textCancel);
        commonPopup.Show();
    }

    public static void ShowSessionExpired(Callback callbackOk = null) {
        string message = TermModel.instance.GetTerm("msg_network_error");
        string textOk = TermModel.instance.GetTerm("btn_retry");
        string textCancel = TermModel.instance.GetTerm("btn_quit");
        Callback callbackCancel = () => {
            Application.Quit();
        };

        CommonPopup commonPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.CommonPopup);
        commonPopup.SetData(CommonPopup.BUTTON_TYPE.YES_NO, 
                            message, 
                            callbackOk, 
                            callbackCancel, 
                            null,
                            textOk,
                            textCancel);
        commonPopup.Show();
    }

    public static void ShowQuitPopup() {
        string msg = TermModel.instance.GetTerm("msg_confirm_quit");

        CommonPopup quitPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.QuitPopup);
        quitPopup.SetData(CommonPopup.BUTTON_TYPE.YES_NO, msg, () => {
            Application.Quit();
        });
        quitPopup.Show();
    }

    public static bool IsQuitPopupActive() {
        CommonPopup quitPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.QuitPopup, false);
        if (quitPopup == null || quitPopup.gameObject.activeSelf == false)
            return false;
        return true;
    }

    public static void HideQuitPopup() {
        CommonPopup quitPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.QuitPopup, false);
        if (quitPopup != null && quitPopup.gameObject.activeSelf)
            quitPopup.Hide();
    }

    public static void ShowShopPopupConfirm(PRODUCT_COST_TYPE costType) {
        string msg;
        ShopMenu shopMenu = UIManager.instance.GetUI<ShopMenu>(UI_NAME.ShopMenu, false);
        //이미 재화탭이 선택된 상태일때는 부족 메시지만 띄워줌
        if (shopMenu != null && 
            shopMenu.gameObject.activeSelf && 
            shopMenu.GetSelectedCategory() == PRODUCT_CATEGORY.GOODS) {
            msg = TermModel.instance.GetTerm("msg_not_enough_diamond");
            ShowSimpleWarning(msg);
            return;
        }

        if (costType == PRODUCT_COST_TYPE.GOLD)
            msg = TermModel.instance.GetTerm("msg_confirm_buy_gold");
        else 
            msg = TermModel.instance.GetTerm("msg_confirm_buy_diamond");

        CommonPopup commonPopup = UIManager.instance.GetUI<CommonPopup>(UI_NAME.CommonPopup);

        Callback callbackOk = () => {
            //스킨 구매중이었다면 재화 탭 표시
            if (shopMenu != null && 
                shopMenu.gameObject.activeSelf && 
                shopMenu.GetSelectedCategory() == PRODUCT_CATEGORY.SKINS) {
                shopMenu.SetData(PRODUCT_CATEGORY.GOODS);
                return;
            }

            ShopPopup shopPopup = UIManager.instance.GetUI<ShopPopup>(UI_NAME.ShopPopup, false);

            bool prevUIExist = false;
            //이미 ShopPopup이 떠있는상태라면 
            if (shopPopup != null && shopPopup.gameObject.activeSelf)
                prevUIExist = true;
            else 
                shopPopup = UIManager.instance.GetUI<ShopPopup>(UI_NAME.ShopPopup);

            shopPopup.SetData(costType, prevUIExist);
            shopPopup.Show();
        };

        commonPopup.SetData(CommonPopup.BUTTON_TYPE.YES_NO, msg, callbackOk);
        commonPopup.Show();
    }
}
