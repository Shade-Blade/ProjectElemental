using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpText_BronzeCakeScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (MainManager.Instance.playerData.itemsUsed > 1)
        {
            Destroy(gameObject);
        }
    }
}
