using System.Collections;
using System.Collections.Generic;
using GameData;
using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using UnityEngine;
using UnityEngine.UI;

public class ShopGoods : ShopProduct {
    public RawImage icoProduct;
    public Text lblProductName;
    public GameObject objPurchased;

    protected bool shopPopup = false;

    public virtual void SetData(ProductDTO productData, bool shopPopup) {
        this.productData = productData;
        this.shopPopup = shopPopup;
        
        lblProductName.text = TermModel.instance.GetProductName(productData.packageID);
        icoProduct.texture = ResourceManager.instance.GetIcoProductTexture(productData.packageID);

        SetPrice();
        DetermineShowPurchased();
    }

    private void DetermineShowPurchased() {
        Common.ToggleActive(objPurchased, IsPurchased());
    }

    private bool IsPurchased() {
        if (productData.consumable == 0 &&
            UserDataModel.instance.IsNonconsumableExist(productData.packageID)) 
            return true;
        return false;
    }

    public override void UpdateData() {
        SetData(productData, shopPopup);
    }

    public void OnBtnBuyProductClick() {
        Callback buyProductCallback = () => {
            ProductUtil.BuyProduct(productData);
        };

        if (productData.costType == (long)PRODUCT_COST_TYPE.CASH) {
            if (productData.consumable == 0 &&
                UserDataModel.instance.IsNonconsumableExist(productData.packageID)) {
                string errormsg = TermModel.instance.GetTerm("msg_nonconsumable_exist");
                MessageUtil.ShowSimpleWarning(errormsg);
                return;
            }

            string msg = TermModel.instance.GetTerm("msg_purchase_confirm");
            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, buyProductCallback, null, null, "", "", "", true);
        }
        else
            buyProductCallback();
    }
}
