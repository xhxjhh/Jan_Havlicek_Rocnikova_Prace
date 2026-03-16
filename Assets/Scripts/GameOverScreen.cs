using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public TMP_Text resultText;
    public Text resultTextLegacy;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != SceneNames.GameOver)
        {
            enabled = false;
            return;
        }

        if (resultText == null)
        {
            resultText = GetComponentInChildren<TMP_Text>(true);
        }

        

    }

    void Start()
    {
        string result = PlayerPrefs.GetString("GameResult", "Game Over");
        int turns = PlayerPrefs.GetInt("TurnsElapsed", 0);
        int heroDamage = PlayerPrefs.GetInt("HeroDamageDealt", 0);

        string finalText = result + "\n" + "Tahy: " + turns + "\n" + "Poškození: " + heroDamage;
        if (resultText != null)
        {
            resultText.text = finalText;
        }
        else if (resultTextLegacy != null)
        {
            resultTextLegacy.text = finalText;
        }
        else
        {
            Debug.LogError("GameOverScreen: No TMP_Text or Text found/assigned for result display.");
        }

        //Invoke("ReturnToMenu", 4f);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
