using UnityEngine;
using UnityEngine.UI;

namespace Automathon.Game.View
{
    public class StartButton : MonoBehaviour
    {
        [SerializeField] private InputTaker inputTaker1;
        [SerializeField] private InputTaker inputTaker2;
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            button.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (inputTaker1.InputProvider != null && inputTaker2.InputProvider != null)
                button.gameObject.SetActive(true);
        }

        private void OnButtonClick()
        {
            if (inputTaker1.InputProvider != null && inputTaker2.InputProvider != null)
            {
                if (WorldView.Instance.InputProviders.Count != 2)
                {
                    Debug.LogError("World view didn't receive input providers sent by input takers");
                    return;
                }

                WorldView.Instance.StartGame();
                inputTaker1.gameObject.SetActive(false);
                inputTaker2.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }

}