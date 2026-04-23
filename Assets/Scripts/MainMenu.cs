using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        PlayerPrefs.SetInt("StageIndex", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.Battle); 
    }

    public void ContinueGame()
    {
        if (GameController.HasSave())
        {
            GameController.ContinueFromSave();
            return;
        }

        StartGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
