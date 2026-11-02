using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class PlayerLives : MonoBehaviour
{
    [Header("Lives")]
    public int startingLives = 3;
    public float respawnProtection = 3f;

    [Header("References")]
    public Transform spawnPoint;
    public TMP_Text livesText;

    [Header("Game Over")]
    public string loseSceneName = "LoseScene";

    private int currentLives;
    private Rigidbody rb;

    private bool gameOver = false;
    private float invulnerableUntil = 0f;

    // Checks whether the player can take damage
    public bool CanBeHit
    {
        get
        {
            return !gameOver &&
                   currentLives > 0 &&
                   Time.time >= invulnerableUntil;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Start each game with 3 lives
        currentLives = startingLives;

        UpdateLivesUI();
    }

    // Called after a successful Skeleton attack
    public void LoseLife()
    {
        if (!CanBeHit)
            return;

        // Remove one life
        currentLives--;

        Debug.Log("Player lost a life! Lives left: "
                  + currentLives);

        UpdateLivesUI();


        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            RespawnPlayer();
        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    void RespawnPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogError(
                "PlayerSpawnPoint has not been assigned!"
            );
            return;
        }

        // Prevent another hit immediately after respawn
        invulnerableUntil =
            Time.time + respawnProtection;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;
        }
        else
        {
            transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }

        Debug.Log("Player has respawned!");
    }

    void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER! No lives remaining.");

        // Open your Lose screen
        SceneManager.LoadScene(loseSceneName);
    }

    //test using component context menu
    [ContextMenu("Test - Lose One Life")]
    void TestLoseLife()
    {
        LoseLife();
    }
}