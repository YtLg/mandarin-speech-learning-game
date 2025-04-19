using UnityEngine;
using UnityEngine.SceneManagement;

public class nextLevelScript : MonoBehaviour
{
    public  void PressToNextLevel()
    {
        SceneManager.LoadScene("Scenes/Supermarket");

    }

}
