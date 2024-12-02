using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitNPCHolderScript : MonoBehaviour
{
    //right facing by default
    public List<GameObject> props;
    public WorldNPCEntity npc;
    public PitNPC pitNPC;

    public enum PitNPC
    {
        Lim,
        Palla,
        Rosette,
        Bead,
        InnkeeperGryphon,
        Pyri,
        InnkeeperRabbit,
        Glaze,
        InnkeeperJellyfish,
        InnkeeperAshcrest,
        InnkeeperMosquito,
        InnkeeperHawk,
        InnkeeperChaintail,
        Aurelia,
        InnkeeperPlaguebud,
        Blanca,
        Torstrum,
        Stella,
        Sizzle,
        Gourmand,
        Vali,
        Wolfram,
        Spruce,
        Roielle,
        ShopkeeperMosquito,
        ShopkeeperSpeartongue,
        Alumi
    }


    public void Flip()
    {
        for (int i = 0; i < props.Count; i++)
        {
            props[i].transform.localPosition = Vector3.Scale(props[i].transform.localPosition, new Vector3(-1,1,1));
        }
        if (npc == null)
        {
            return;
        }
        npc.transform.localPosition = Vector3.Scale(npc.transform.localPosition, new Vector3(-1, 1, 1));
        npc.wed.initialFacingAngle += 180;
        npc.initialFacingAngle += 180;
        npc.trueFacingRotation += 180;
    }

    //various NPCs may have a thing that determines their selling inventory here
    public virtual void Initialize()
    {
        if (transform.position.x > 0)
        {
            Flip();
        }
    }
}
