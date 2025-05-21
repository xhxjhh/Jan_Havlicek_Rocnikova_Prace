using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public Text resultText;

    void Start()
    {
        string result = PlayerPrefs.GetString("GameResult", "Game Over");
        resultText.text = result;

        Invoke("ReturnToMenu", 4f); // wait 4 seconds then go to menu
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}