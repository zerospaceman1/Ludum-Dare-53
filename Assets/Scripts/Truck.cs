using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : Indexed
{
    public GameObject box;
    public GameObject frontWheel;
    public GameObject rearWheel;

    public GameObject packageHolder;
    public float maxWeight;
    public float maxSize;

    public bool full = false;
    
    public void skip()
    {
        Game.Instance.skipTruck(this);
    }
}
