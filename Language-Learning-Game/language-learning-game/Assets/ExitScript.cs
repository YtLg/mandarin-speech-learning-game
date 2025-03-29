using UnityEngine;

public class ExitScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(true);
    }
    
    public void OpenExit()
    {
        gameObject.SetActive (false);
    }
}
