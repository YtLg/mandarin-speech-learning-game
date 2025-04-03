using UnityEngine;

public class ExitScript : MonoBehaviour
{
    public GameObject exitCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(true);
        exitCollider.SetActive(false);
    }
    
    public void OpenExit()
    {
        gameObject.SetActive (false);
        exitCollider.SetActive(true);
    }
}
