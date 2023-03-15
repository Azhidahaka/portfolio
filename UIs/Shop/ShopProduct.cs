using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopProduct : MonoBehaviour {
    public Color notEnoughColor = Color.red;
    public Text lblCost;
    public MaskableGraphic icoCostTypeGold;
    public MaskableGraphic icoCostTypeDiamond;

    protected GameData.ProductDTO productData;

    protected void SetPrice() {
        if (productData.costType == (long)PRODUCT_COST_TYPE.CASH) 
            lblCost.text = IAPManager.instance.GetLocalizedPrice(productData);
        else
            lblCost.text = Common.GetCostFormat((PRODUCT_COST_TYPE)productData.costType,
                                                productData.cost,
                                                notEnoughColor);
    }

    protected void SetCostIco() {
        switch ((PRODUCT_COST_TYPE)productData.costType) {
            case PRODUCT_COST_TYPE.CASH:
                Common.ToggleActive(icoCostTypeDiamond.gameObject, false);
                Common.ToggleActive(icoCostTypeGold.gameObject, false);
                break;
            case PRODUCT_COST_TYPE.DIAMOND:
                Common.ToggleActive(icoCostTypeDiamond.gameObject, true);
                Common.ToggleActive(icoCostTypeGold.gameObject, false);
                break;
            case PRODUCT_COST_TYPE.GOLD:
                Common.ToggleActive(icoCostTypeDiamond.gameObject, false);
                Common.ToggleActive(icoCostTypeGold.gameObject, true);
                break;
        }
    }

    public virtual void UpdateData() {
    }
}
