using UnityEngine;

public class ObjectToggleCanvas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ObjectPanelManager objectPanelManager;
    public ObjectDataScript objectDataScript;
    public Vector3 offset = new(0f, 0f, 0f);


    // Update is called once per frame
    public void ToggleCanvas()
    {
        print(offset);
       objectPanelManager.ShowCanvas(this.transform, offset, objectDataScript);
    }
}
