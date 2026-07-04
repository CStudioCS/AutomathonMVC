using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Automathon.Game
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private Button endOfEndScreenButton;

        private void Awake()
        {
            endOfEndScreenButton.onClick.AddListener(Done);
        }

        public void Scroll(Tank.TeamType winner)
        {
            //Reset everything

            gameObject.SetActive(true);

            if (winner == Tank.TeamType.Red)
            {
                winnerText.color = Color.red;
                winnerText.text = $"Red Wins!";
            }
            else
            {
                winnerText.color = Color.green;
                winnerText.text = $"Green Wins!";
            }

            //Make the object scroll with animations
        }

        private void Done()
        {
            //Scroll back up
            gameObject.SetActive(false);
            WorldView.Instance.OnEndScreenDone();
        }

        private void OnDestroy()
        {
            endOfEndScreenButton.onClick.RemoveAllListeners();
        }
    }
}