using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSetFocus : MonoBehaviour {
    public Transform posFinger;

    private Transform target;
    void Update() {
        if (target == null)
            return;

        transform.position = target.position;
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }
}
