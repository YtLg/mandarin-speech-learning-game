using UnityEngine;

public class TrackPlayerScript : MonoBehaviour
{
    public GameObject menu;
    public Transform head;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        head = Camera.main?.transform;
    }

    // Update is called once per frame
    void Update()
    {
        menu.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
        menu.transform.forward *= -1;
    }
}
