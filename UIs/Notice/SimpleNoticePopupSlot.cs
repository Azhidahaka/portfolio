using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNoticePopupSlot : MonoBehaviour {
    public Text lblTitleCenter;
    public Text lblTitleRight;
    public Text lblPostingDate;
    public Text lblContents;

    public Animator animator;

    public RawImage imgNotice;
    public GameObject goNew;

    public enum State {
        Fold,
        Open,
    }

    public State state;

    private UserData.NoticeDTO noticeInfo;

    private void OnEnable() {
        EventManager.Register(EventEnum.GetNoticeImageComplete, OnGetNoticeImageComplete);
    }

    private void OnGetNoticeImageComplete(object[] args) {
        string uuid = (string)args[0];
        if (noticeInfo == null || uuid != noticeInfo.uuid)
            return;

        TryGetImage();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.GetNoticeImageComplete, OnGetNoticeImageComplete);
    }

    public void SetData(UserData.NoticeDTO noticeInfo) {
        this.noticeInfo = noticeInfo;
        state = State.Fold;
        string title = noticeInfo.title;
        bool centerTitle = true;

        if (centerTitle) {
            lblTitleCenter.text = title;
            Common.ToggleActive(lblTitleCenter.gameObject, true);
            Common.ToggleActive(lblTitleRight.gameObject, false);
        }
        else {
            lblTitleRight.text = title;
            Common.ToggleActive(lblTitleRight.gameObject, true);
            Common.ToggleActive(lblTitleCenter.gameObject, false);
        }

        DateTime parsedDate = DateTime.Parse(noticeInfo.postingDate);
        TimeSpan timeSpan = ( parsedDate - new DateTime(1970, 1, 1, 0, 0, 0) );
        double postingDateDouble = timeSpan.TotalSeconds;
        string postingDateStr = Common.GetDateStringFormat(postingDateDouble);
        lblPostingDate.text = postingDateStr;

        lblContents.text = noticeInfo.content;

        TryGetImage();   

        Common.ToggleActive(goNew, noticeInfo.read == false);
    }

    private void TryGetImage() {
        if (string.IsNullOrEmpty(noticeInfo.imageKey) == false) {
            imgNotice.texture = NoticeManager.instance.GetImage(noticeInfo);
            if (imgNotice.texture == null) {
                NoticeManager.instance.RequestImage(noticeInfo);
            }
        }
        else
            imgNotice.texture = ResourceManager.instance.GetEventBannerImage(0);
    }

    public void OnBtnSlotClick() {
        WebNotice.instance.ReqSetNoticeRead(noticeInfo);

        if (state == State.Fold) {
            state = State.Open;
            Common.ToggleActive(goNew, false);
        }
        else
            state = State.Fold;

        AnimationUtil.SetTrigger(animator, state.ToString());
    }
}
