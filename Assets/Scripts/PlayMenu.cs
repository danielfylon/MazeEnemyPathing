using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void OnPlayButton()
    {
        Debug.Log("PLAY BUTTON PRESSED!");
        SceneManager.LoadScene("Map");
    }

    public void OnQuitButton()
    {
        Debug.Log("QUIT BUTTON PRESSED!");
        Application.Quit();
    }
}