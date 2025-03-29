using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class StarTracker : MonoBehaviour
{
    private int starCount;
    public int starGoal;
    public TextMeshProUGUI scoreText;
    public ExitScript exitScript;
    public void Start()
    {
        UpdateText();
    }
    public void Increment()
    {
        starCount++;
        UpdateText();

        if (starCount >= starGoal)
        {
            exitScript.OpenExit();
        }
    }

    public void UpdateText()
    {
        string currScoreText = starCount.ToString();
        string goalScoreText = starGoal.ToString();
        scoreText.text = (currScoreText + "/" + goalScoreText);
    }

}
