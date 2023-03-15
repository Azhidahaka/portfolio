using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruct : MonoBehaviour {
    private float time;
    private void Awake() {
        Invoke("Destroy", time);
    }

    private void Destroy() {
        Destroy(gameObject);
    }

    public void SetData(float time) {
        this.time = time;
    }
}
