using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // Ensure there's an EventSystem in the scene
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), 
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }
    }
}
