using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Singletons/GameConfig")]
public class GameConfigs : ScriptableObject
{
    [BoxGroup("Touch")]
    public float touchSphereRadius = 0.5f;

    [BoxGroup("Touch")]
    public LayerMask wormSegmentLayer;

    [BoxGroup("Movement")]
    public float moveCooldown = 0;

    [BoxGroup("Movement")]
    public float gridMoveThreshold = 25;

    [BoxGroup("Movement")]
    public float moveDuration = 0.05f;

    #region WormGizmos
    [BoxGroup("Gizmos")]
    public Color wormGizmoHeadColor = Color.blue;

    [BoxGroup("Gizmos")]
    public Color wormGizmoBodyColor = Color.green;

    [BoxGroup("Gizmos")]
    public float wormGizmoSize = 0.5f;
    #endregion
}