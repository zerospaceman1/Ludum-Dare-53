using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Package : Indexed
{
    public bool delivered;
    public bool waiting;  // waiting to be delivered, stopped on conveyor
    public int scoreValue;
    public int scoreDecreaseMult;

    public float size;
    public float weight;

    [SerializeField]
    private TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start()
    {
        Game.Instance.OnPackageScoreTickDown += updateScore;
        showScore();
        waiting = false;
        delivered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (!Game.Instance.paused)
        {
            Debug.Log("Mouse Click Detected");
            Game.Instance.setHeldPackage(index);
        }
    }

    private void OnMouseUp()
    {
        //Game.Instance.setHeldPackage(null);
    }

    public void setDelivered()
    {
        delivered = true;
    }

    private void updateScore()
    {
        //if (scoreValue > 0 && !delivered && waiting)
        if (!delivered && waiting)  // let package score keep going into negatives??
        {
            scoreValue -= 1 * scoreDecreaseMult;
        }
        showScore();
    }

    private void showScore()
    {
        scoreText.SetText(scoreValue.ToString());
    }
}
