using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardTooltip : MonoBehaviour {
    public Text lblGold;

    public void SetData(long gold) {
        lblGold.text = Common.GetCommaFormat(gold);
    }

    public void Show() {
        Common.ToggleActive(gameObject, true);
        EventManager.Notify(EventEnum.RewardTooltipShow);
    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
    }
}
