using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenu : MonoBehaviour
{
    public void OnPlayAgainButton()
    {
        Debug.Log("PLAY BUTTON PRESSED!");
        SceneManager.LoadScene("Map");
    }

    public void OnQuitButton1()
    {
        Debug.Log("QUIT BUTTON PRESSED!");
        Application.Quit();
    }
}