using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpText_HealthSightScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.HealthSight))
        {
            Destroy(gameObject);
        }
    }
}
