using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductUtil {
    public static void BuyProduct(GameData.ProductDTO productData) {
        //비소모성 상품을 이미 구매한 경우
        if (productData.consumable == 0 &&
            UserDataModel.instance.IsNonconsumableExist(productData.packageID)) {
            string msg = TermModel.instance.GetTerm("msg_nonconsumable_exist");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        //현금결제 아이템
        if (productData.costType == (long)PRODUCT_COST_TYPE.CASH) {
            BuyProductWithCash(productData);
            return;
        }

        //재화 부족시
        if (IsNotEnoughGoods(productData)) {
            MessageUtil.ShowShopPopupConfirm((PRODUCT_COST_TYPE)productData.costType);
            return;
        }

        //구매 확인팝업에서 '예' 선택시
        Callback callbackOK = () => {
            WebProduct.instance.ReqBuyProduct(productData.packageID, OnBuyProductSuccess);
        };

        //구매 확인팝업 표시
        string formatConfirm = TermModel.instance.GetTerm("format_purchase_confirm");
        string costTypeTerm;
        if (productData.costType == (long)PRODUCT_COST_TYPE.DIAMOND)
            costTypeTerm = TermModel.instance.GetTerm("format_cost_type_diamond");
        else
            costTypeTerm = TermModel.instance.GetTerm("format_cost_type_gold");
        string msgConfirm = string.Format(formatConfirm, productData.cost, costTypeTerm);

        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msgConfirm, callbackOK, null, null, "", "", "", true);
    }

    private static bool IsNotEnoughGoods(GameData.ProductDTO productData) {
        if ((productData.costType == (long)PRODUCT_COST_TYPE.DIAMOND &&
            UserDataModel.instance.userProfile.diamond < productData.cost) ||
            (productData.costType == (long)PRODUCT_COST_TYPE.GOLD &&     
            UserDataModel.instance.userProfile.gold < productData.cost))
            return true;
        return false;
    }

    private static void BuyProductWithCash(GameData.ProductDTO productData) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        WebProduct.instance.ReqBuyProduct(productData.packageID, OnBuyProductSuccess);
#elif UNITY_ANDROID
        IAPManager.instance.BuyProductID(productData.strID, OnBuyProductSuccess);
#elif UNITY_IOS
        IAPManager.instance.BuyProductID(productData.appleStrID, OnBuyProductSuccess);
#endif
    }

    private static void OnBuyProductSuccess() {
        EventManager.Notify(EventEnum.BuyProductComplete);
        //어떤형태의 구매인지에 상관없이 서버에 데이터를 저장한다.
        BackendRequest.instance.ReqSyncUserData(false, true, OnReqSyncUserDataSuccess);
    }

    protected static void OnReqSyncUserDataSuccess() {
        string msg = TermModel.instance.GetTerm("msg_purchase_complete");
        MessageUtil.ShowSimpleWarning(msg, false);
    }

    public static bool IsNonConsumablePurchasedAll() {
        bool purchasedAll = true;
        List<GameData.ProductDTO> productDatas = GameDataModel.instance.products;
        foreach (GameData.ProductDTO productData in productDatas) {
            //소모성 상품은 체크하지 않음
            if (productData.consumable != 0)
                continue;

            if (UserDataModel.instance.IsNonconsumableExist(productData.packageID))
                continue;

            purchasedAll = false;
        }

        return purchasedAll;
    }
}
