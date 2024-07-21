using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CoverObject))]
public class CoverSystem : Editor {

    private CoverObject coverTarget;

    private void OnEnable() {
        if(coverTarget == null) {
            coverTarget = (CoverObject)target;
            coverTarget.Setup();
            coverTarget.CreatePointsSketch();
        }
    }

    private void OnSceneGUI () {
        coverTarget.UpdateValues();
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Create points")) {
            coverTarget.CreatePointsObjects();
        }
        if (GUILayout.Button("Update Parent")) {
            coverTarget.UpdateParent();
        }
        if (GUILayout.Button("Recreate points drawing")) {
            coverTarget.ResetCoverPoints();
        }
    }
}