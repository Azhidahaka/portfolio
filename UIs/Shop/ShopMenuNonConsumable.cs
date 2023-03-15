using GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuNonConsumable : ShopGoods {
    public override void SetData(ProductDTO productData, bool shopPopup) {
        this.productData = productData;
        this.shopPopup = shopPopup;

        SetPrice();
        DetermineShowPurchased();
    }

    private void DetermineShowPurchased() {
        Common.ToggleActive(objPurchased, IsPurchased());
    }

    private bool IsPurchased() {
        if (UserDataModel.instance.IsNonconsumableExist(productData.packageID)) 
            return true;
        return false;
    }
}
