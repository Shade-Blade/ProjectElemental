using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractTrigger : InteractTrigger
{
    public WorldNPCEntity npcEntity;

    public override void Interact()
    {
        npcEntity.Interact();
    }
}
