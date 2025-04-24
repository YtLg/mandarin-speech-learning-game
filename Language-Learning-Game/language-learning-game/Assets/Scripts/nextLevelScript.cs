using UnityEngine;
using UnityEngine.SceneManagement;

public class nextLevelScript : MonoBehaviour
{
    // Probably could have been in another script, it just loads up the supermarket scene.
    public  void PressToNextLevel()
    {
        SceneManager.LoadScene("Scenes/Supermarket");

    }

}
