using Automathon.AI;
using Automathon.Game.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Automathon.Game.View
{
    public class InputTaker : MonoBehaviour
    {
        public InputProvider InputProvider;
        private Image currentDeviceImg;

        [SerializeField] private InputManager inputManager;

        [Header("Device Images")]
        [SerializeField] private Image noDeviceImg;
        [SerializeField] private Image xBoxControllerDeviceImg;
        [SerializeField] private Image playStationControllerDeviceImg;
        [SerializeField] private Image switchControllerDeviceImg;
        [SerializeField] private Image leftKeyboardDeviceImg;
        [SerializeField] private Image rightKeyboardDeviceImg;
        [SerializeField] private Image aiDeviceImg;

        private Dictionary<PlayerInputProvider.PlayerControlsType, Image> controlsToImg;

        [Header("Misc")]
        [SerializeField] private TMP_Text pressAnyInputText;
        [SerializeField] private Button pairDeviceButton;
        [SerializeField] private TMP_Text pairDeviceButtonText;

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text testResultsText;

        private bool waitingForInput;

        private void Awake()
        {
            controlsToImg = new Dictionary<PlayerInputProvider.PlayerControlsType, Image>()
            {
                { PlayerInputProvider.PlayerControlsType.Xbox, xBoxControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.PlayStation, playStationControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.Switch, switchControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.LeftKeyboard, leftKeyboardDeviceImg },
                { PlayerInputProvider.PlayerControlsType.RightKeyboard, rightKeyboardDeviceImg },
            };

            inputField.onEndEdit.AddListener(FinishedTyping);
            inputField.onValueChanged.AddListener(OnTyping);
            pairDeviceButton.onClick.AddListener(OnButtonClick);

            currentDeviceImg = noDeviceImg;
            SetCurrentDeviceImg(noDeviceImg);
        }

        private void FinishedTyping(string finalText)
        {
            ResetButtonFields();
            ResetInput();

            AIInputProvider aIInputProvider = new AIInputProvider(finalText);

            if (aIInputProvider.TestPing())
            {
                testResultsText.text = "Connection successful";
                SetInputProvider(aIInputProvider);
                SetCurrentDeviceImg(aiDeviceImg);
            }
            else
                testResultsText.text = "Could not connect to AI Server. Make sure Python's play script is already running.";
        }

        private void OnTyping(string text)
        {
            ResetInput();
            ResetButtonFields();
        }

        private void OnButtonClick()
        {
            ResetInput();
            waitingForInput = !waitingForInput;

            if (waitingForInput)
            {
                pairDeviceButtonText.text = "Cancel";
                pressAnyInputText.text = "Press any Input...";
            }
            else
                ResetButtonFields();
        }

        private void Update()
        {
            if (waitingForInput && inputManager.TryFindNewInput(out PlayerInputProvider playerInputProvider))
            {
                SetInputProvider(playerInputProvider);
                Image img = controlsToImg[playerInputProvider.ControlsType];
                SetCurrentDeviceImg(img);
                ResetButtonFields();
            }
        }

        private void ResetButtonFields()
        {
            waitingForInput = false;
            pairDeviceButtonText.text = "Pair New Device";
            pressAnyInputText.text = "";
            testResultsText.text = "";
        }

        private void ResetInput()
        {
            SetCurrentDeviceImg(noDeviceImg);

            if (InputProvider != null)
            {
                WorldView.Instance.InputProviders.Remove(InputProvider);
                InputProvider.OnDestroyed();
            }
        }

        private void SetInputProvider(InputProvider inputProvider)
        {
            InputProvider = inputProvider;
            WorldView.Instance.InputProviders.Add(inputProvider);
        }

        private void SetCurrentDeviceImg(Image img)
        {
            currentDeviceImg.gameObject.SetActive(false);
            currentDeviceImg = img;
            currentDeviceImg.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            inputField.onEndEdit.RemoveAllListeners();
            pairDeviceButton.onClick.RemoveAllListeners();
        }
    }
}