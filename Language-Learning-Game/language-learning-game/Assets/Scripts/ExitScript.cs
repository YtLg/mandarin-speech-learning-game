using UnityEngine;

public class ExitScript : MonoBehaviour
{

    // Used to set the door active and inacvtive, could have probably been combined with next level script
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
