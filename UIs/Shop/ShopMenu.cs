using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ShopMenuTab;

public class ShopMenu : UIBase {
    public List<ShopMenuTab> tabs;

    public GameObject prefabDiamond;
    public GameObject prefabGold;
    public GameObject prefabSkins;

    public LayoutGroup goodsLayoutGroup;
    public LayoutGroup skinsLayoutGroup;

    public ScrollRect scrollRect;

    public CommonGoods commonGoods;

    public ShopMenuDailyAds dailyAds;

    private PRODUCT_CATEGORY selectedCategory;

    private List<ShopGoods> listGoods;
    private List<ShopMenuSkin> listSkin;

    private void Awake() {
        Common.ToggleActive(prefabGold, false);
        Common.ToggleActive(prefabDiamond, false);
        Common.ToggleActive(prefabSkins, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.ShopMenuTabSelected, OnShopMenuTabSelected);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
        EventManager.Register(EventEnum.ShopMenuProductSelected, OnShopMenuProductSelected);
        EventManager.Register(EventEnum.ShopMenuSkinSelected, OnShopMenuSkinSelected);
    }

    private void ResetGrid() {
        if  (scrollRect.content == null)
            return;
        Vector3 gridLocalPos = scrollRect.content.localPosition;
        gridLocalPos.y = 0;
        scrollRect.content.localPosition = gridLocalPos;
    }

    private void OnShopMenuSkinSelected(object[] args) {
        GameData.SkinDTO skinData = (GameData.SkinDTO)args[0];
        WebUser.instance.ReqChangeSkin(skinData.skinID);
    }

    private void OnShopMenuProductSelected(object[] args) {
        GameData.ProductDTO productData = (GameData.ProductDTO)args[0];
        ProductUtil.BuyProduct(productData);
    }

    private void OnUserDataUserInfoUpdate(object[] args) {
        SetData(selectedCategory);
    }

    private void OnShopMenuTabSelected(object[] args) {
        selectedCategory = (PRODUCT_CATEGORY)args[0];
        SetData(selectedCategory);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ShopMenuTabSelected, OnShopMenuTabSelected);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
        EventManager.Remove(EventEnum.ShopMenuProductSelected, OnShopMenuProductSelected);
        EventManager.Remove(EventEnum.ShopMenuSkinSelected, OnShopMenuSkinSelected);
    }

    private void SetTabs() {
        foreach (ShopMenuTab tab in tabs) {
            tab.SetState(selectedCategory);
        }
    }

    public void SetData(PRODUCT_CATEGORY selectedCategory = PRODUCT_CATEGORY.SKINS) {
        this.selectedCategory = selectedCategory;
        SetTabs();
        SetProducts();
        commonGoods.SetData();
    }

    private void SetProducts() {
        //재화
        if (selectedCategory == PRODUCT_CATEGORY.GOODS) {
            scrollRect.content = goodsLayoutGroup.GetComponent<RectTransform>();

            DetermineShowDailyAds();            

            //초기화
            if (listGoods != null) {
                foreach (ShopGoods goods in listGoods) {
                    Destroy(goods.gameObject);
                }
            }

            listGoods = new List<ShopGoods>();
            List<GameData.ProductDTO> productDatas = GameDataModel.instance.GetProductDatas(PRODUCT_CATEGORY.GOODS);

            List<GameData.ProductDTO> purchasedDatas = new List<GameData.ProductDTO>(); 
            foreach (GameData.ProductDTO productData in productDatas) {
                if (IsPurchased(productData.packageID))
                    purchasedDatas.Add(productData);
                else
                    SetGoodsSlot(productData);
            }

            foreach (GameData.ProductDTO purchasedData in purchasedDatas) {
                SetGoodsSlot(purchasedData);
            }

            Common.ToggleActive(goodsLayoutGroup.gameObject, true);
            Common.ToggleActive(skinsLayoutGroup.gameObject, false);
        }
        //스킨
        else {
            scrollRect.content = skinsLayoutGroup.GetComponent<RectTransform>();

            if (listSkin == null) {
                listSkin = new List<ShopMenuSkin>();
                List<GameData.SkinDTO> skinDatas = GameDataModel.instance.skins;

                foreach (GameData.SkinDTO skinData in skinDatas) {
                    GameObject go = Instantiate(prefabSkins, skinsLayoutGroup.transform);
                    ShopMenuSkin element = go.GetComponent<ShopMenuSkin>();
                    element.SetData(skinData);
                    Common.ToggleActive(go, true);
                    listSkin.Add(element);
                }
            }
            else {
                foreach(ShopMenuSkin element in listSkin) {
                    element.UpdateData();
                }
            }

            Common.ToggleActive(skinsLayoutGroup.gameObject, true);
            Common.ToggleActive(goodsLayoutGroup.gameObject, false);
        }

        ResetGrid();
    }

    private void DetermineShowDailyAds() {
        dailyAds.SetData();
        Common.ToggleActive(dailyAds.gameObject, true);
    }

    private void SetGoodsSlot(GameData.ProductDTO productData) {
        GameObject go;

        List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetPackageDatas(productData.packageID);
        if (productData.consumable == 0) {
            if (productData.isEvent == 1 && productData.endDate < Common.GetUTCNow())
                return;

            go = ResourceManager.instance.LoadProductPrefab(productData.packageID, goodsLayoutGroup.transform);
            ShopMenuNonConsumable element = go.GetComponent<ShopMenuNonConsumable>();
            element.SetData(productData, false);
            Common.ToggleActive(go, true);
            listGoods.Add(element);
        }
        else {
            if (packageDatas[0].type == (long)PACKAGE_TYPE.GOLD) 
                go = Instantiate(prefabGold, goodsLayoutGroup.transform);
            else 
                go = Instantiate(prefabDiamond, goodsLayoutGroup.transform);
            ShopGoods element = go.GetComponent<ShopGoods>();
            element.SetData(productData, false);
            Common.ToggleActive(go, true);
            listGoods.Add(element);
        }
    }

    private bool IsPurchased(long packageID) {
        if (UserDataModel.instance.IsNonconsumableExist(packageID)) 
            return true;
        return false;
    }

    public void OnBtnBackClick() {
        Hide();
    }

    public override void OnCopy(List<object> datas) {
        SetData((PRODUCT_CATEGORY)datas[0]);
    }

    public override List<object> GetCopyDatas() {
        List<object> datas = new List<object>();
        datas.Add(selectedCategory);
        return datas;
    }

    public PRODUCT_CATEGORY GetSelectedCategory() {
        return selectedCategory;
    }
}
