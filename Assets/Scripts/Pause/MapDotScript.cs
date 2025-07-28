using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapDotScript : MonoBehaviour
{
    //public MainManager.WorldLocation worldLocation;
    public string worldLocation;

    public MainManager.WorldLocation GetWorldLocation()
    {
        Enum.TryParse(worldLocation, out MainManager.WorldLocation result);

        return result;
    }
}
