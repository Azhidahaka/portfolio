using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour {
    public static UICamera instance;
    public Camera cam;
    private void Awake() {
        instance = this;
        cam = GetComponent<Camera>();
    }
}
