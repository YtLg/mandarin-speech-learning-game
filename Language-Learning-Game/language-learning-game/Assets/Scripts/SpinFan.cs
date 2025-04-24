using UnityEngine;

public class SpinFan : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        // rotates it a lot to make it look spinning
        transform.Rotate(0, 0, 5);
    }
}
