using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CodeController : MonoBehaviour
{
    [Header("Code Settings")]
    [SerializeField] private string[] codeDigits = { "7", "2", "4", "9" };

    [Header("Code UI")]
    [SerializeField] private TMP_Text[] codeSlots;

    private bool[] cluesFound = new bool[4];

    private int clues = 0;

    private void Start()
    {
        UpdateCodeDisplay();
    }

    private void UpdateCodeDisplay()
    {
        for (int i = 0; i < codeSlots.Length; i++)
        {
            codeSlots[i].text = cluesFound[i] ? codeDigits[i] : "_";
        }
    }

    public void RevealClue(int clueNumber)
    {
        int index = clueNumber - 1;

        if (index < 0 || index >= codeDigits.Length)
            return;

        if (cluesFound[index])
            return;

        cluesFound[index] = true;

        UpdateCodeDisplay();

        if (AllCluesFound())
        {
            Win();
        }
    }

    private bool AllCluesFound()
    {
        foreach (bool found in cluesFound)
        {
            if (!found)
                return false;
        }

        return true;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            RevealClue(1);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            RevealClue(2);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            RevealClue(3);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            RevealClue(4);
    }

    private void Win()
    {
        SceneManager.LoadScene("WinScreen");
    }
}
