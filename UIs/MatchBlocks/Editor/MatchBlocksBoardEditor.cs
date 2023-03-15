using UnityEngine;
using UnityEditor;
using LuckyFlow.EnumDefine;

[CustomEditor(typeof(MatchBlocksBoard))]
public class MatchBlocksBoardEditor : Editor {

    private MatchBlocksBoard board;
    private long prevWaveID;

    private void OnEnable() {
        board = target as MatchBlocksBoard;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (board.targetWaveID <= 0) {
            board.targetWaveID = 1;
            board.SetWaveInEditMode();
            prevWaveID = board.targetWaveID;
        }
        else if (board.targetWaveID != prevWaveID) {
            prevWaveID = board.targetWaveID;
            board.SetTargetWaveID(board.targetWaveID, true);
        }

        if (GUILayout.Button("Wave " + board.targetWaveID + " 보기")) {
            board.SetWaveInEditMode();
        }
    }
}