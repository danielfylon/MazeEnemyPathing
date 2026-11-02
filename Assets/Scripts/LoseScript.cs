
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string menuSceneName = "Menu";

    // Called when Play Again is clicked
    public void PlayAgain()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    // Called when Quit is clicked
    public void QuitGame()
    {
        Debug.Log("Player quit the game.");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
