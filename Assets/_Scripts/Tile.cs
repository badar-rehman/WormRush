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
    [SerializeField] public MeshRenderer _renderer;
    
    [ReadOnly] public bool IsOccupied  = false;
    [ReadOnly] public int x, y;

    public void Init(int _x, int _y)
    {
        IsOccupied = false;
        x = _x;
        y = _y;
        
        bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
        
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