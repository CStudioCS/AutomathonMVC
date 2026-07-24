using UnityEngine;
using UnityEngine.UI;

namespace Automathon.Game.View
{
    public class StartButton : MonoBehaviour
    {
        [SerializeField] private GameObject inputMenu;
        [SerializeField] private NewInputTaker inputTaker1;
        [SerializeField] private NewInputTaker inputTaker2;
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            button.gameObject.SetActive(false);
        }

        private void Update()
        {
            button.gameObject.SetActive(inputTaker1.InputProvider != null && inputTaker2.InputProvider != null);
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
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }

}