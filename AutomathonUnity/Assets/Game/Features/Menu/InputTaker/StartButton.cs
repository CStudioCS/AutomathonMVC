using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Automathon.Game.View
{
    public class StartButton : MonoBehaviour
    {
        [SerializeField] private InputTaking inputTaking;
        [SerializeField] private NewInputTaker inputTaker1;
        [SerializeField] private NewInputTaker inputTaker2;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text startText;
        [SerializeField] private float startTextAlphaChangePeriod;

        private InputAction cancelAction;


        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            button.gameObject.SetActive(false);

            cancelAction = InputSystem.actions.FindAction("UI/Cancel");
            cancelAction.Enable();
            cancelAction.performed += OnCancel;
        }

        private void OnCancel(InputAction.CallbackContext ctx)
        {
            inputTaker1.Reset();
            inputTaker2.Reset();
            inputTaking.ActivateMenu();
            button.gameObject.SetActive(false);
        }

        private void OnButtonClick()
        {
            if (inputTaker1.InputProvider != null && inputTaker2.InputProvider != null)
            {
                if (WorldView.Instance.InputProviders[0] == null || WorldView.Instance.InputProviders[1] == null)
                {
                    Debug.LogError("World view didn't receive input providers sent by input takers");
                    return;
                }

                WorldView.Instance.StartGame();
                SoundManager.instance.PlaySound("StartGame");
            }
        }

        private void Update()
        {
            startText.color = new Color(startText.color.r, startText.color.g, startText.color.b, ViewMath.TriangleFunction(startTextAlphaChangePeriod));
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
            cancelAction.Disable();
            cancelAction.performed -= OnCancel;
        }
    }

}