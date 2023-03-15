using BackEnd;
using LuckyFlow.EnumDefine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener {
    public static IAPManager instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    private Callback successCallback;

    private long restorationResult = 0;

    private bool restore = true;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Init();
    }

    public bool IsInitialized() {
        return (storeController != null && extensionProvider != null);
    }

    public void Init() {
        if (IsInitialized())
            return;

        StandardPurchasingModule module = StandardPurchasingModule.Instance();
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        List<GameData.ProductDTO> productDatas = GameDataModel.instance.products;

        for (int i = 0; i < productDatas.Count; i++) {
            GameData.ProductDTO productData = productDatas[i];
            //현금으로 결제하는 상품들만 등록
            if (productData.costType != (long)PRODUCT_COST_TYPE.CASH)
                continue;

            ProductType productType;
            if (productData.consumable == 0)
                productType = ProductType.NonConsumable;
            else
                productType = ProductType.Consumable;
#if UNITY_ANDROID
            builder.AddProduct(productDatas[i].strID, productType, new IDs
#elif UNITY_IOS
            builder.AddProduct(productDatas[i].appleStrID, productType, new IDs
#endif
            {
                {productDatas[i].strID, GooglePlay.Name},
                {productDatas[i].appleStrID, AppleAppStore.Name},
            });
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyProductID(string productId, Callback successCallback) {
        this.successCallback = successCallback;
        restore = false;

        try {
            if (IsInitialized()) {
                Product product = storeController.products.WithID(productId);

                if (product.definition.type == ProductType.NonConsumable &&
                    HadPurchased(productId)) {
                    ShowHadPurchasedMessage();
#if UNITY_EDITOR || UNITY_ANDROID
                    CheckNonConsumableHadPurchased();
#endif
                    return;
                }
                
                if (product != null && product.availableToPurchase) {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    storeController.InitiatePurchase(product);
                }
                else {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e) {
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }

    public void RestorePurchase(Callback callback = null) {
        if (!IsInitialized()) {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        restore = true;
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer) {
            Debug.Log("RestorePurchases started ...");
            IAppleExtensions apple = extensionProvider.GetExtension<IAppleExtensions>();

            apple.RestoreTransactions((result) => {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                if (result == true)
                    restorationResult = (long)RESTORATION_RESULT.SUCCESS;
                else
                    restorationResult = (long)RESTORATION_RESULT.FAIL;
            });
        }
        else {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController sc, IExtensionProvider ep) {
        Debug.Log("OnInitialized : PASS");

        storeController = sc;
        extensionProvider = ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason) {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + reason);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
#if UNITY_ANDROID
        BackendReturnObject validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescription", false);
        if (validation.IsSuccess()) {
#endif
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            GameData.ProductDTO productData = GameDataModel.instance.GetProductData(args.purchasedProduct.definition.id);
            WebProduct.instance.ReqBuyProduct(productData.packageID, successCallback, restore);

#if UNITY_ANDROID
        }
        else {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }
#endif
        restore = true;
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        restore = true;
    }

    public string GetLocalizedPrice(GameData.ProductDTO productData) {
#if DEVELOPMENT_BUILD
        return "T" + Common.GetCommaFormat(productData.cost);
#endif

        Product[] productIAPDatas = storeController.products.all;

        for (int i = 0; i < productIAPDatas.Length; i++) {
#if UNITY_ANDROID
            if (productIAPDatas[i].definition.id != productData.strID)
                continue;
#elif UNITY_IOS
            if (productIAPDatas[i].definition.id != productData.appleStrID)
                continue;
#endif
            return productIAPDatas[i].metadata.localizedPriceString;
        }

        return "T" + Common.GetCommaFormat(productData.cost);
    }

    private void Update() {
        if (restorationResult == (long)RESTORATION_RESULT.FAIL) {
            string msg = TermModel.instance.GetTerm("msg_purchase_restoration_fail");
            MessageUtil.ShowSimpleWarning(msg);
            restorationResult = (long)RESTORATION_RESULT.NONE;
        }
        else if (restorationResult == (long)RESTORATION_RESULT.SUCCESS) {
            string msg = TermModel.instance.GetTerm("msg_purchase_restoration_success");
            MessageUtil.ShowSimpleWarning(msg);
            restorationResult = (long)RESTORATION_RESULT.NONE;
        }
    }

    private void ShowHadPurchasedMessage() {
        string msg;
#if UNITY_EDITOR || UNITY_ANDROID
        msg = TermModel.instance.GetTerm("msg_nonconsumable_exist");
        MessageUtil.ShowSimpleWarning(msg);
//#elif UNITY_IOS
            msg = TermModel.instance.GetTerm("msg_purchase_restoration_confirm");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO,
                                msg,
                                () => RestorePurchase(null));
#endif
    }

    public bool HadPurchased(string productID) {
        if (!IsInitialized())
            return false;

        Product product = storeController.products.WithID(productID);
        if (product != null)
            return product.hasReceipt;

        return false;
    }

    public void CheckNonConsumableHadPurchased(Callback callback = null) {
        if (!IsInitialized())
            return;
        restore = true;
        try {
            Product[] productIAPDatas = storeController.products.all;
            for (int i = 0; i < productIAPDatas.Length; i++) {
                Product productIAPData = productIAPDatas[i];

                //비소모성 상품이 아니거나, 구매기록이 없으면 패스
                if (productIAPData.definition.type != ProductType.NonConsumable ||
                    HadPurchased(productIAPData.definition.id) == false)
                    continue;

                GameData.ProductDTO productData = GameDataModel.instance.GetProductData(productIAPData.definition.id);
                if (UserDataModel.instance.IsNonconsumableExist(productData.packageID) == false)
                    WebProduct.instance.ReqBuyProduct(productData.packageID, null, true);
            }

            Debug.Log($"CheckNonConsumableHadPurchased::Complete");
        }
        catch (Exception e) {
            Debug.Log($"CheckNonConsumableHadPurchased::{e.Message}");
        }

        if (callback != null)
            callback();
    }
}