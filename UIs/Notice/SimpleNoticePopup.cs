using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNoticePopup : UIBase {
    public GameObject prefabNotice;
    public LayoutGroup layoutGroup;

    private List<UserData.NoticeDTO> noticeInfos;

    private List<SimpleNoticePopupSlot> listSlot = new List<SimpleNoticePopupSlot>();
    private List<SimpleNoticePopupSlot> listReusableSlot = new List<SimpleNoticePopupSlot>();

    private void Awake() {
        Common.ToggleActive(prefabNotice, false);
    }
    public void SetData(List<UserData.NoticeDTO> noticeInfos) {
        this.noticeInfos = noticeInfos;
        SetScrollVIew();
    }

    private void SetScrollVIew() {
        if (listSlot == null) {
            listSlot = new List<SimpleNoticePopupSlot>();
            listReusableSlot = new List<SimpleNoticePopupSlot>();
        }
        else {
            foreach (SimpleNoticePopupSlot scrollViewItem in listSlot) {
                if (listReusableSlot.Contains(scrollViewItem) == false)
                    listReusableSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        GameObject scrollViewGO;

        for (int i = 0; i < noticeInfos.Count; i++) {
            UserData.NoticeDTO noticeInfo = noticeInfos[i];

            SimpleNoticePopupSlot scrollViewItem;

            if (i >= listReusableSlot.Count) {
                scrollViewGO =  Instantiate(prefabNotice, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<SimpleNoticePopupSlot>();
                listReusableSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<SimpleNoticePopupSlot>();
            scrollViewItem.SetData(noticeInfo);
            listSlot.Add(scrollViewItem);
        }
    }
}
