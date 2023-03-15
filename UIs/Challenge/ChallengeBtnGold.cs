using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeBtnGold : MonoBehaviour {
    public long gold;

    public GameObject goNormal;
    public GameObject goSelected;

    private bool availableChange;

    public void SetData(bool availableChange) {
        this.availableChange = availableChange;
    }

    public void OnBtnClick() {
        if (UserDataModel.instance.userProfile.gold < gold) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_gold");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        if (availableChange == false)
            return;

        EventManager.Notify(EventEnum.ChallengeSelectGold, gold);
    }

    public void SetState(long gold) {
        if (this.gold == gold)
            Common.ToggleActive(goSelected, true);
        else
            Common.ToggleActive(goSelected, false);
    }
}
