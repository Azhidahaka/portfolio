using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningEffect : MatchBlocksEffect {
    public void SetData(Vector2 targetSizeDelta, float boardScale, float duration = 0) {
        base.SetData(targetSizeDelta, duration);
        EventManager.Register(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);
        transform.localScale = Vector3.one * boardScale;
    }

    protected override void OnEnable() {

    }

    private void OnMatchBlocksChangeBoardWave(object[] args) {
        Hide();
        EventManager.Remove(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);
    }
}
