using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuTab : MonoBehaviour {

    public Image imgTab;
    public PRODUCT_CATEGORY category;

    public void SetState(PRODUCT_CATEGORY selectedCategory) {
        if (selectedCategory == category) {
            Common.ToggleActive(imgTab.gameObject, true);
        }
        else {
            Common.ToggleActive(imgTab.gameObject, false);
        }
    }

    public void OnTabSelected() {
        EventManager.Notify(EventEnum.ShopMenuTabSelected, category);
    }
}
