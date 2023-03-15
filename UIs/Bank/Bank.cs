using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;

public class Bank : MonoBehaviour {
    public ParticleSystem psPiggyMoney;
    public ParticleSystem psOrbit;
    public Canvas canvasIco;
    public Animator animator;
    
    private GameData.PiggyBankDTO piggyBankData;
    private PIG_STATE state = PIG_STATE.None;

    public void SetData(GameData.PiggyBankDTO piggyBankData, CANVAS_ORDER order) {
        this.piggyBankData = piggyBankData;
        canvasIco.sortingOrder = (int)order + 2;

        SetParticleSortOrder(psPiggyMoney, (int)order + 3);
        SetParticleSortOrder(psOrbit, (int)order + 1);
    }

    private void SetParticleSortOrder(ParticleSystem ps, int order) {
        ParticleSystem[] particles = ps.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++) {
            particles[i].GetComponent<ParticleSystemRenderer>().sortingOrder = (int)order;
        }
    }

    private IEnumerator JobCheckGold() {
        while(true) {
            state = GetNewState();
            AnimationUtil.SetTrigger(animator, state.ToString());
            yield return new WaitForSecondsRealtime(1.0f);
        }       
    }

    public PIG_STATE GetNewState() {
        PIG_STATE newState;
        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        //최소 골드량을 만족하지 못함. Unable
        if (collectedGold < 
            piggyBankData.capacity * (Constant.MIN_DEBIT_GOLD_PERCENT / 100)) {
            newState = PIG_STATE.Unable;
        }
        else if (collectedGold >= piggyBankData.capacity) {
            newState = PIG_STATE.Full;
        }
        else {
            newState = PIG_STATE.Idle;
        }

        return newState;
    }

    public void Show() {
        gameObject.SetActive(true);
        StartCoroutine(JobCheckGold());
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private IEnumerator JobGetGold() {
        string trigger = PIG_STATE.Click.ToString();
        AnimationUtil.SetTrigger(animator, trigger);
        float length = AnimationUtil.GetAnimationLength(animator, trigger.ToLower());

        yield return new WaitForSecondsRealtime(length);

        string msg = TermModel.instance.GetTerm("msg_withdraw_complete");
        MessageUtil.ShowSimpleWarning(msg);

        StartCoroutine(JobCheckGold());
    }

    public void ShowGetGoldAnim() {
        StopCoroutine(JobCheckGold());
        StartCoroutine(JobGetGold());
    }
}
