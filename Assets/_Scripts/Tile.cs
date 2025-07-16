using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Color _baseColor, _offsetColor, _highlightColor;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    private Color _originalColor;

    public void Init(bool isOffset) {
        _originalColor = isOffset ? _offsetColor : _baseColor;
        _renderer.material.color = _originalColor;
        
        // Ensure the tile has a collider for raycasting
        if (GetComponent<BoxCollider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _renderer.material.color = _highlightColor;
        if (_highlight != null)
            _highlight.SetActive(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _renderer.material.color = _originalColor;
        if (_highlight != null)
            _highlight.SetActive(false);
    }
}