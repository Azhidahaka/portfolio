using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksTimer : MonoBehaviour {
    public Slider sldTimer;

    private float timeMax;

    public void SetData(GameData.StageDTO stageData) {
        timeMax = stageData.time;
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
         
    }

    private void OnMatchBlocksStart(object[] args) {
        StartCoroutine(JobCheckTimer());
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
         
    }

    private IEnumerator JobCheckTimer() {
        while(MatchBlocksReferee.instance.GetState() != MatchBlocksReferee.STATE.GAME_OVER) {
            UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
            sldTimer.value = refereeNote.remainTime / timeMax;
            yield return new WaitForEndOfFrame();
        }
    }
}
