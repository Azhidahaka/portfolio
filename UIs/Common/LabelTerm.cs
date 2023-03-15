using UnityEngine;
using UnityEngine.UI;

public class LabelTerm : MonoBehaviour {
    public string term;
    public bool resetOnEnable = false;

    private Text lblText;

    public void Awake() {
        lblText = GetComponent<Text>();
    }

    private void Start() {
        if (null == TermModel.instance || lblText == null)
            return;
        if (term == "title") {
            Debug.Log(term);
        }
        lblText.text = TermModel.instance.GetTerm(term);
    }

    private void OnEnable() {
        if (resetOnEnable == false || lblText == null)
            return;

        lblText.text = TermModel.instance.GetTerm(term);
    }
}
