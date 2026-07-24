using Automathon.AI;
using Automathon.Game.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Automathon.Game.View
{
    public class NewInputTaker : MonoBehaviour
    {
        public InputProvider InputProvider;
        private Image currentDeviceImg;

        [SerializeField] private int playerIndex;
        [SerializeField] private InputManager inputManager;

        [Header("Device Images")]
        [SerializeField] private Image noDeviceImg;
        [SerializeField] private Image xboxControllerDeviceImg;
        [SerializeField] private Image playStationControllerDeviceImg;
        [SerializeField] private Image switchControllerDeviceImg;
        [SerializeField] private Image leftKeyboardDeviceImg;
        [SerializeField] private Image rightKeyboardDeviceImg;
        [SerializeField] private Image aiDeviceImg;

        private Dictionary<PlayerInputProvider.PlayerControlsType, Image> controlsToImg;

        [Header("Indication Frame")]
        [SerializeField] private TMP_Text indicationText;

        [Header("AI Input")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text testResultsText;
        [SerializeField] private TMP_Text tcpText;

        [Header("Misc")]
        [SerializeField] private TMP_Text pressAnyInputText;

        [SerializeField] private GameObject tankImageGameObject;

        private bool waitingForInput;

        [SerializeField] private Button changePlayerAIButton;
        private bool isPlayer;

        private void Awake()
        {
            controlsToImg = new Dictionary<PlayerInputProvider.PlayerControlsType, Image>()
            {
                { PlayerInputProvider.PlayerControlsType.Xbox, xboxControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.PlayStation, playStationControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.Switch, switchControllerDeviceImg },
                { PlayerInputProvider.PlayerControlsType.LeftKeyboard, leftKeyboardDeviceImg },
                { PlayerInputProvider.PlayerControlsType.RightKeyboard, rightKeyboardDeviceImg },
            };

            inputField.onEndEdit.AddListener(FinishedTyping);
            inputField.onValueChanged.AddListener(OnTyping);

            currentDeviceImg = noDeviceImg;
            SetCurrentDeviceImg(noDeviceImg);

            waitingForInput = true;
            isPlayer = true;

            changePlayerAIButton.onClick.AddListener(OnPlayerAIChange);
        }

        private void OnPlayerAIChange()
        {
            if (isPlayer)
            {
                SwitchToAI();
            }
            else
            {
                SwitchToPlayer();
            }
            isPlayer = !isPlayer;
        }

        private void FinishedTyping(string finalText)
        {
            ResetButtonFields();
            ResetInput();

            try
            {
                AIInputProvider aIInputProvider = new AIInputProvider("tcp://localhost:" + finalText);

                if (aIInputProvider.TestPing())
                {
                    testResultsText.text = "Connection successful";
                    SetInputProvider(aIInputProvider);
                    SetCurrentDeviceImg(aiDeviceImg);
                }
                else
                    testResultsText.text = "Could not connect to AI Server. Make sure Python's play script is already running.";
            }
            catch
            {
                testResultsText.text = "Invalid tcp address";

            }
        }

        private void OnTyping(string text)
        {
            ResetInput();
            ResetButtonFields();
        }

        private void Update()
        {
            if (waitingForInput && inputManager.TryFindNewInput(out PlayerInputProvider playerInputProvider))
            {
                SetInputProvider(playerInputProvider);

                Image img = controlsToImg[playerInputProvider.ControlsType];
                SetCurrentDeviceImg(img);

                waitingForInput = false;

                pressAnyInputText.text = "";
                testResultsText.text = "";

                ShowTankImage();
            }
        }

        private void ResetButtonFields()
        {
            waitingForInput = false;
            pressAnyInputText.text = "";
            testResultsText.text = "";
        }

        private void ResetInput()
        {
            SetCurrentDeviceImg(noDeviceImg);

            if (InputProvider != null)
            {
                WorldView.Instance.InputProviders[playerIndex] = null;
                if (InputProvider is AIInputProvider aIInputProvider)
                    aIInputProvider.EndLife();
                InputProvider = null;
            }
        }

        private void SetInputProvider(InputProvider inputProvider)
        {
            InputProvider = inputProvider;
            WorldView.Instance.InputProviders[playerIndex] = inputProvider;
        }

        private void SetCurrentDeviceImg(Image img)
        {
            currentDeviceImg.gameObject.SetActive(false);
            currentDeviceImg = img;
            currentDeviceImg.gameObject.SetActive(true);
        }

        private void ShowTankImage()
        {
            tankImageGameObject.SetActive(true);
        }

        private void HideTankImage()
        {
            tankImageGameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            inputField.onEndEdit.RemoveAllListeners();
            changePlayerAIButton.onClick.RemoveAllListeners();
        }

        private void SwitchToAI()
        {
            ResetInput();
            waitingForInput = false;
            indicationText.text = "AI";
            HideTankImage();
            pressAnyInputText.text = "Enter TCP port number...";
            SetCurrentDeviceImg(aiDeviceImg);
            inputField.text = "";
            inputField.gameObject.SetActive(true);
            testResultsText.text = "";
            testResultsText.gameObject.SetActive(true);
            tcpText.gameObject.SetActive(true);
        }

        private void SwitchToPlayer()
        {
            ResetInput();
            waitingForInput = true;
            indicationText.text = "Player";
            HideTankImage();
            pressAnyInputText.text = "Press any input...";
            SetCurrentDeviceImg(noDeviceImg);
            inputField.gameObject.SetActive(false);
            testResultsText.gameObject.SetActive(false);
            tcpText.gameObject.SetActive(false);
        }
    }

}