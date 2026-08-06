using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputTaking : MonoBehaviour
{
    [SerializeField] private Button leftSwitchButton;
    [SerializeField] private Button rightSwitchButton;
    [SerializeField] private TMP_InputField leftInputField;
    [SerializeField] private TMP_InputField rightInputField;

    public void DeactivateMenu()
    {
        leftSwitchButton.interactable = false;
        rightSwitchButton.interactable = false;
        leftInputField.interactable = false;
        rightInputField.interactable = false;
    }

    public void ActivateMenu()
    {
        leftSwitchButton.interactable = true;
        rightSwitchButton.interactable = true;
        leftInputField.interactable = true;
        rightInputField.interactable = true;
    }
}
