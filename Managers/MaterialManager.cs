using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {
    public static MaterialManager instance;

    public Material matGray;

    private void Awake() {
        instance = this;
    }

    public Material GetGray() {
        return matGray;
    }
}
