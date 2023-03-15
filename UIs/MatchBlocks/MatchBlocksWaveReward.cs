using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksWaveReward : MonoBehaviour {
    public float ratio;

    public RewardTooltip tooltip;

    public enum TRIGGERS {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled,
        GainReward,
    }

    public Animator animator;
    public TRIGGERS trigger = TRIGGERS.Disabled;

    private UserData.RefereeNoteDTO refereeNote;

    public void OnEnable() {
        tooltip.Hide();

        EventManager.Register(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
        EventManager.Register(EventEnum.MatchBlocksRefereeDecision, OnMatchBlocksRefereeDecision);
        EventManager.Register(EventEnum.MatchBlocksGetWaveRewardComplete, OnMatchBlocksGetWaveRewardComplete);
    }

    private void OnMatchBlocksGetWaveRewardComplete(object[] args) {
        refereeNote = (UserData.RefereeNoteDTO)args[0];
        //애니메이션 재생 
        AnimationUtil.SetTrigger(animator, TRIGGERS.GainReward.ToString());
    }

    private void GetRewardAnimComplete() {
        SetData(refereeNote);
    }

    private void OnMatchBlocksRefereeDecision(object[] args) {
        UserData.RefereeNoteDTO refereeNote = (UserData.RefereeNoteDTO)args[1];
        SetData(refereeNote);
    }

    private void OnMatchBlocksStart(object[] args) {
        UserData.RefereeNoteDTO refereeNote = (UserData.RefereeNoteDTO)args[0];
        SetData(refereeNote);
    }


    public void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
        EventManager.Remove(EventEnum.MatchBlocksRefereeDecision, OnMatchBlocksRefereeDecision);
        EventManager.Remove(EventEnum.MatchBlocksGetWaveRewardComplete, OnMatchBlocksGetWaveRewardComplete);
    }

    public void SetData(UserData.RefereeNoteDTO refereeNote) {
        //보상을 이미 받은경우 : 사라짐
        if (refereeNote.waveRewardReceived) 
            trigger = TRIGGERS.Disabled;
        //아직 수령하지 않은경우
        else {
            GameData.BoardWaveDTO boardWaveData = MatchBlocksReferee.instance.GetBoardWaveData();
            float currentRatio = (float)refereeNote.waveScore / boardWaveData.goal;
            //웨이브점수가 보상 비율에 도달하지 않은경우 : Disable
            if (currentRatio < ratio) 
                trigger = TRIGGERS.Disabled;
            //보상을 수령할 수 있는경우 : Normal
            else 
                trigger = TRIGGERS.Normal;
        }

        AnimationUtil.SetTrigger(animator, trigger.ToString());
    }

    public void OnBtnGetRewardClick() {
        //툴팁 표시
        long gold = CalculateRewardGold();
        tooltip.SetData(gold);
        tooltip.Show();
    }

    private long CalculateRewardGold() {
        UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
        GameData.WaveRewardDTO waveRewardData = GameDataModel.instance.GetWaveRewardData(refereeNote.stageLevel);

        long waveAdditionalValue = refereeNote.waveCount / 3;
        long gold = waveRewardData.clear + waveRewardData.clearAdditional * waveAdditionalValue;
        gold = Math.Min(gold, waveRewardData.clearMax);
   
        return gold;
    }

    public void OnResGetWaveReward() {
        SetData(MatchBlocksReferee.instance.GetRefereeNote());
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            tooltip.Hide();
            SetData(MatchBlocksReferee.instance.GetRefereeNote());
        }
    }
}
