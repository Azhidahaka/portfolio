using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeGameEnterPopup : UIBase {
    public CommonGoods goods;
    public Text lblCurrentTicket;
    public Text lblMaxTicket;
    public Text lblTicketPrice;
    public Text lblResetRemainTime;

    public Button btnStart;
    public Animator btnStartAnimator;

    public GameObject objBtnBuyTicket;
    private long ticketPrice;
    private string remainTimeFormat;

    private void OnEnable() {
        remainTimeFormat = TermModel.instance.GetTerm("format_daily_reset");
        EventManager.Register(EventEnum.RefreshDate, OnRefreshDate);
        EventManager.Register(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);

        StartCoroutine(JobCheckTime());
    }

    private IEnumerator JobCheckTime() {
        while (true) {
            UpdateRemainTime();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void UpdateRemainTime() {
        string remainTimeStr = Common.GetTimerFormat(Common.GetUTCTodayZero() + 86400 - Common.GetUTCNow());
        lblResetRemainTime.text = string.Format(remainTimeFormat, remainTimeStr);
    }

    private void OnRefreshDate(object[] args) {
        SetData();
    }

    private void OnGoodsUpdate(object[] args) {
        SetData();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.RefreshDate, OnRefreshDate);
        EventManager.Remove(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);
    }

    public void SetData() {
        goods.SetData();

        long currentTicket = UserDataModel.instance.userProfile.challengeTicket;
        Color currentColor = Color.green;
        if (currentTicket == 0)
            currentColor = Color.red;

        lblCurrentTicket.text = Common.GetColoredText(currentColor, Common.GetCommaFormat(currentTicket));
        lblMaxTicket.text = $"/ {Common.GetCommaFormat(Constant.CHALLENGE_TICKET_MAX)}";

        long ticketBuyCount = UserDataModel.instance.userProfile.challengeTicktBuyCount;
        if (ticketBuyCount < Constant.CHALLENGE_TICKET_PRICE_LIST.Count)
            ticketPrice = Constant.CHALLENGE_TICKET_PRICE_LIST[(int)ticketBuyCount];
        else
            ticketPrice = Constant.CHALLENGE_TICKET_PRICE_LIST[Constant.CHALLENGE_TICKET_PRICE_LIST.Count - 1];

        Color ticketPriceColor = Color.white;
        if (UserDataModel.instance.userProfile.diamond < ticketPrice) 
            ticketPriceColor = Color.red;
        lblTicketPrice.text = Common.GetColoredText(ticketPriceColor, Common.GetCommaFormat(ticketPrice));

        DetermineBtnStartEnable();
        UpdateRemainTime();

        if (currentTicket > 0) 
            Common.ToggleActive(objBtnBuyTicket, false);
        else
            Common.ToggleActive(objBtnBuyTicket, true);
    }

    private void DetermineBtnStartEnable() {
        long currentTicket = UserDataModel.instance.userProfile.challengeTicket;
        //티켓이 충분하지 않으면 비활성화
        if (currentTicket == 0) {
            btnStart.interactable = false;
            AnimationUtil.SetTrigger(btnStartAnimator, "Disabled");
        }
        else {
            btnStart.interactable = true;
            AnimationUtil.SetTrigger(btnStartAnimator, "Normal");
        }
    }

    public void OnBtnStartClick() {
        /*
        WebStage.instance.ReqChallengeStart(() => {
            App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
        });*/
    }

    public void OnBtnBuyTicketClick() {
        long currentTicket = UserDataModel.instance.userProfile.challengeTicket;
        if (currentTicket >= Constant.CHALLENGE_TICKET_MAX) {
            string msg = TermModel.instance.GetTerm("msg_ticket_max");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        if (UserDataModel.instance.userProfile.diamond < ticketPrice) {
            MessageUtil.ShowShopPopupConfirm(LuckyFlow.EnumDefine.PRODUCT_COST_TYPE.DIAMOND);
            return;
        }

        WebUser.instance.ReqBuyChallengeTicket(ticketPrice, OnResBuyChalengeTicket);
    }

    private void OnResBuyChalengeTicket() {
        string msg = TermModel.instance.GetTerm("msg_buy_ticket_complete");
        MessageUtil.ShowSimpleWarning(msg);
        SetData();
    }

}
