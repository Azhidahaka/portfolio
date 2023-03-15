using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이 스크립트가 붙은 녀석들은 씬이 전환되어도 사라지지않음
public class DontDestroy : MonoBehaviour {
    public bool eventDestroy = false;

    private void Awake() {
        DontDestroyOnLoad(this);
    }

    private void OnEnable() {
        EventManager.Register( EventEnum.LoadTitle, OnLoadTitle);
    }

    private void OnLoadTitle(object[] args) {
        if (eventDestroy)
            Destroy(gameObject);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.LoadTitle, OnLoadTitle);
    }
}
