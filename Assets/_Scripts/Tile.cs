using Sirenix.OdinInspector;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileAppearance
    {
        None = 0, 
        ColorChange = 1, 
        MaterialChange = 2
    };
    
    [SerializeField] TileAppearance _appearance;
    
    [SerializeField][ShowIf("_appearance", TileAppearance.ColorChange)] private Color _baseColor, _offsetColor;
    [SerializeField][ShowIf("_appearance", TileAppearance.MaterialChange)] private Material _baseMat, _offsetMat;
    [SerializeField] private MeshRenderer _renderer;
    
    [ReadOnly] public bool IsOccupied  = false;

    public void Init(bool isOffset)
    {
        if(_appearance == TileAppearance.MaterialChange)
        {
            _renderer.material = isOffset ? _offsetMat : _baseMat;
        }
        else if (_appearance == TileAppearance.ColorChange)
        {
            _renderer.material.color = isOffset ? _offsetColor : _baseColor;
        }
    }

    public void SetOccupied(bool value)
    {
        IsOccupied = value;
    }
}
