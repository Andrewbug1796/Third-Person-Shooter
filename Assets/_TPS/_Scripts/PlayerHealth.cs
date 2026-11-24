using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public string mainMenuScene = "MainMenu";

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            // Player died - back to the title screen
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
