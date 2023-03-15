using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenNumber : MonoBehaviour {
    public float duration = 0.5f;
    public float delayStart = 0f;
    public long number = 0;

    private Text label;
    
    public bool set = false;
    public bool commaFormat = true;

    public void Awake() {
        label = GetComponent<Text>();
    }

    private void OnEnable() {
        StartCoroutine(JobCheckNumber());
    }

    public void SetData(long number, bool commaFormat = true) {
        this.number = number;
        this.commaFormat = commaFormat;

        set = true;
        ShowLog("Tween SetData");
    }

    IEnumerator JobCheckNumber() {
        while(true) {
            long prevNumber;
            string text = label.text;
            text = text.Replace(",", "");
            if(long.TryParse(text, out prevNumber) == false)
                prevNumber = 0;
            ShowLog(string.Format("prevNumber:{0}", prevNumber));

            yield return new WaitWhile(()=> set == false);
            yield return new WaitForSeconds(delayStart);

            ShowLog("Tween Start");
            float startTime = Time.time;
            float elapsedTime = 0;
            long currentNumber = prevNumber;

            while(currentNumber != number) {
                ShowLog(string.Format("current:{0}, number:{1}", currentNumber, number));
                elapsedTime = Time.time - startTime;
                currentNumber = (long)(Mathf.Lerp(prevNumber, number, elapsedTime / duration));

                ShowLog(string.Format("elapsedTime:{0}, duration:{1}", elapsedTime, duration));

                if (commaFormat)
                    label.text = Common.GetCommaFormat(currentNumber);
                else
                    label.text = currentNumber.ToString();
                yield return new WaitForEndOfFrame();
            }

            ShowLog(string.Format("Tween End. current:{0}, number:{1}", currentNumber, number));
            set = false;
        }
    }

    public bool IsSet() {
        return set;
    }

    public void ShowLog(string text) {
        if (Constant.SHOW_TWEEN_NUMBER_LOG == false)
            return;

        Debug.Log(text);
    }

    public long GetNumber() {
        return number;
    }
}
