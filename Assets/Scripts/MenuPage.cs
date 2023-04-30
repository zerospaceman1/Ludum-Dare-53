using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPage : MonoBehaviour
{
    public enum Page
    {
        main,
        game,       // UI while playing
        gameMenu,   // Menu when player presses ESC or 'Pause Menu'
        gameover    // displayed when game is over
    }

    public Page page;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
