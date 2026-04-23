using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    private const string StageIndexKey = "StageIndex";

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
        PlayerPrefs.SetInt(StageIndexKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.MainMenu);
    }

    public void NextStage()
    {
        string result = PlayerPrefs.GetString("GameResult", "Game Over");
        if (result != "You Won!")
        {
            PlayerPrefs.SetInt(StageIndexKey, 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneNames.MainMenu);
            return;
        }

        int stageIndex = Mathf.Max(1, PlayerPrefs.GetInt(StageIndexKey, 1));
        stageIndex++;
        PlayerPrefs.SetInt(StageIndexKey, stageIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.Battle);
    }

    public void SaveBetweenStages()
    {
        string result = PlayerPrefs.GetString("GameResult", "Game Over");
        if (result != "You Won!")
        {
            return;
        }

        GameController.SaveBetweenStages();

        if (resultText != null)
        {
            if (!resultText.text.Contains("Uloženo"))
            {
                resultText.text += "\nUloženo";
            }
        }
        else if (resultTextLegacy != null)
        {
            if (!resultTextLegacy.text.Contains("Uloženo"))
            {
                resultTextLegacy.text += "\nUloženo";
            }
        }
    }
}
