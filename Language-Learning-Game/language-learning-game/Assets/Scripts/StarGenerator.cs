using UnityEngine;

public class StarGenerator : MonoBehaviour
{
    public StarTracker starTracker;
    public Vector3 starOffset;
    public GameObject starObjectPrefab;

    // Creates a star prefab above the calling object's head.
    public void GenerateStar()
    {
        GameObject star = Instantiate(starObjectPrefab);
        star.transform.position = transform.position + starOffset;
        starTracker.Increment();
    }
}
