using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonShadesFix : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);// Deselect the button when it is clicked
        }
    }
}
