using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public GameObject truckGo;
    private void OnMouseUp()
    {
        if (!Game.Instance.paused)
        {
            Game.Instance.placeHeldPackageOnTruck(truckGo);
            Debug.Log("place package on truck");
        }
    }
}
