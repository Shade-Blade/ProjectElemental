using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractTrigger : InteractTrigger
{
    //Thing to reduce problems: interactable will be set up automatically
    //public WorldNPCEntity npcEntity;

    public void Start()
    {
        interactable = GetComponentInParent<IInteractable>();
    }

    public override void Interact()
    {
        if (interactable != null)
        {
            interactable.Interact();
        }
        else
        {
            Debug.LogError("NPC Interact Trigger has no interactable npc to trigger the method of");
        }
    }
}
