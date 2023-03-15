using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleWarningPopup : UIBase {
    public Text lbl;
    public GameObject objBg;

    public void SetData(string msg, bool showBg = true) {
        lbl.text = msg;
        if (showBg)
            Common.ToggleActive(objBg, true);
        else
            Common.ToggleActive(objBg, false);
    }

    public void OnEnable() {
        StartCoroutine(JobDelayedHide());
    }

    private IEnumerator JobDelayedHide() {
        yield return new WaitForSecondsRealtime(1.0f);
        DelayedHide();
    }

    private void DelayedHide() {
        Hide();
    }

    public override void OnCopy(List<object> datas) {
        SetData(datas[0] as string);
    }

    public override List<object> GetCopyDatas() {
        List<object> datas = new List<object>();
        datas.Add(lbl.text);
        return datas;
    }
}
