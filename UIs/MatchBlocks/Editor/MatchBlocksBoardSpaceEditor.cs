using UnityEngine;
using UnityEditor;
using LuckyFlow.EnumDefine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MatchBlocksBoardSpace))]
public class MatchBlocksBoardSpaceEditor : Editor {

    private MatchBlocksBoardSpace boardSpace;

    private void OnEnable() {
        boardSpace = target as MatchBlocksBoardSpace;
        boardSpace.SetSpaceInfoInEditMode();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Ice")) {
            boardSpace.SetIce(true, true);
        }

        if (GUILayout.Button("Normal")) {
            boardSpace.SetNormal(true, true);
        }

        if (GUILayout.Button("Empty")) {
            boardSpace.SetEmpty(true, true);
        }

        if (GUILayout.Button("Hide")) {
            boardSpace.SetHide(true, true);
        }
    }

}