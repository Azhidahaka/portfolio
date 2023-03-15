using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : UIBase {
    public Text lblTitle;
    public GameObject prefabGold;
    public GameObject prefabDiamond;
    public ScrollRect scrollView;
    public LayoutGroup layoutGroup;

    private PRODUCT_COST_TYPE type;
    private List<ShopGoods> productSlots = new List<ShopGoods>();

    private bool prevUIExist = false;

    private void Awake() {
        Common.ToggleActive(prefabGold, false);
        Common.ToggleActive(prefabDiamond, false);
    }

    private void OnEnable() {
        PauseTime();
        EventManager.Register(EventEnum.BuyProductComplete, OnBuyProductComplete);
    }

    private void OnBuyProductComplete(object[] args) {
        Hide();
    }

    public override void Hide() {
        if (prevUIExist)
            SetData(PRODUCT_COST_TYPE.GOLD);
        else
            base.Hide();
    }

    private void OnDisable() {
        ResumeTime();
        EventManager.Remove(EventEnum.BuyProductComplete, OnBuyProductComplete);
    }

    public void SetData(PRODUCT_COST_TYPE type, bool prevUIExist = false) {
        this.type = type;
        this.prevUIExist = prevUIExist;

        if (type == PRODUCT_COST_TYPE.GOLD) 
            lblTitle.text = TermModel.instance.GetTerm("gold_shop");
        else
            lblTitle.text = TermModel.instance.GetTerm("diamond_shop");

        SetSlots();
        scrollView.Rebuild(CanvasUpdate.Layout);
    }

    private void SetSlots() {
        foreach (ShopGoods productSlot in productSlots) {
            Common.ToggleActive(productSlot.gameObject, false);
        }

        PACKAGE_TYPE packageType = PACKAGE_TYPE.GOLD;
        if (type == PRODUCT_COST_TYPE.DIAMOND)
            packageType = PACKAGE_TYPE.DIAMOND;

        List<GameData.ProductDTO> productDatas = GameDataModel.instance.GetProductDatas(PRODUCT_CATEGORY.GOODS, false);
        for (int i = 0; i < productDatas.Count; i++) {
            GameData.ProductDTO productData = productDatas[i];
            GameData.PackageDTO packageData = GameDataModel.instance.GetPackageData(productData.packageID, packageType);

            if (packageData == null)
                continue;

            ShopGoods productSlot;
            //추가하려는 슬롯오브젝트가 없는경우, 새로운 슬롯 생성
            if (i > productSlots.Count - 1) {
                GameObject slotObject;
                if (packageData.type == (long)PACKAGE_TYPE.GOLD)
                    slotObject = Instantiate(prefabGold, layoutGroup.transform);
                else
                    slotObject = Instantiate(prefabDiamond, layoutGroup.transform);

                productSlot = slotObject.GetComponent<ShopGoods>();
                productSlots.Add(productSlot);
            }
            //슬롯 오브젝트가 이미 있다면 데이터만 추가
            else 
                productSlot = productSlots[i];

            productSlot.SetData(productData, true);
            Common.ToggleActive(productSlot.gameObject, true);
        }
    }
}
