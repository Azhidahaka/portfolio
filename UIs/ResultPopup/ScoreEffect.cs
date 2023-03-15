using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreEffect : MonoBehaviour {
    public TextMeshProUGUI txtScore;
    public void SetData(long score) {
        txtScore.text = score.ToString();
    }
}
