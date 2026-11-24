using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour
{
    public string mainMenuScene = "MainMenu";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Goal Reached!");
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
