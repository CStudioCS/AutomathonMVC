using Automathon.Game.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputTaking : MonoBehaviour
{
    [SerializeField] private Button leftSwitchButton;
    [SerializeField] private Button rightSwitchButton;
    [SerializeField] private TMP_InputField leftInputField;
    [SerializeField] private TMP_InputField rightInputField;

    [SerializeField] private NewInputTaker inputTaker1;
    [SerializeField] private NewInputTaker inputTaker2;
    [SerializeField] private StartButton startButton;

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

    public void ResetInputTaking()
    {
        inputTaker1.Reset();
        inputTaker2.Reset();
        ActivateMenu();
        startButton.gameObject.SetActive(false);
    }
}
