using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class GetItemListPopup : UIBase {
    public LayoutGroup layoutGroup;
    public GameObject preafabItemSlot;

    private List<GetItemListPopupSlot> listItem = new List<GetItemListPopupSlot>();
    private List<GetItemListPopupSlot> listReusableItem = new List<GetItemListPopupSlot>();

    private List<MailRewardDTO> rewards;
    private List<GameData.PackageDTO> packages;

    private Callback hideCallback;

    private void Awake() {
        Common.ToggleActive(preafabItemSlot.gameObject, false);
    }

    private void OnEnable() {
        SetInFrontInCanvas();
    }

    public void SetData(List<MailRewardDTO> rewards) {
        this.rewards = rewards;
        this.hideCallback = null;

        SetScrollVIew(true);
    }

    private void SetScrollVIew(bool mail) {
        if (listItem == null) {
            listItem = new List<GetItemListPopupSlot>();
            listReusableItem = new List<GetItemListPopupSlot>();
        }
        else {
            foreach (GetItemListPopupSlot scrollViewItem in listItem) {
                if (listReusableItem.Contains(scrollViewItem) == false)
                    listReusableItem.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listItem.Clear();
        }

        GameObject scrollViewGO;

        IList listData;
        if (mail)
            listData = rewards;
        else
            listData = packages;

        for (int i = 0; i < listData.Count; i++) {
            GetItemListPopupSlot scrollViewItem;

            if (i >= listReusableItem.Count) {
                scrollViewGO =  Instantiate(preafabItemSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<GetItemListPopupSlot>();
                listReusableItem.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableItem[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<GetItemListPopupSlot>();
            if (mail)
                scrollViewItem.SetData((MailRewardDTO)listData[i]);
            else
                scrollViewItem.SetData((GameData.PackageDTO)listData[i]);
            listItem.Add(scrollViewItem);
        }
    }

    public void SetData(List<GameData.PackageDTO> packages, Callback hideCallback = null) {
        this.packages = packages;
        this.hideCallback = hideCallback;

        SetScrollVIew(false);
    }

    public void OnBtnHideClick() {
        Hide();
        if (hideCallback != null) {
            hideCallback();
            hideCallback = null;
        }
    }
}
