using Sirenix.OdinInspector;
using UnityEngine;

public class WormSegment : MonoBehaviour
{
    public Worm worm;
    
    [SerializeField] [ReadOnly] private int segmentIndex;
    [SerializeField] [ReadOnly] private bool isCloseToHead;
    
    private float moveTimer = 0f;
    private bool isDragging = false;
    private Vector2 mouseStartPos;
    
    public Vector3 Pos
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector3 LPos
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

    public void Setup(Worm _worm, int _segmentIndex, bool _isCloseToHead)
    {
        worm = _worm;
        segmentIndex = _segmentIndex;
        isCloseToHead = _isCloseToHead;
    }

    public bool IsCloseToHead => isCloseToHead;
}