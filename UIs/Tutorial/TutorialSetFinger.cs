using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSetFinger : MonoBehaviour {
    private Transform target;

    void Update() {
        if (target == null)
            return;

        SetPosition();
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void SetPosition() {
        transform.position = target.position;
    }
}
