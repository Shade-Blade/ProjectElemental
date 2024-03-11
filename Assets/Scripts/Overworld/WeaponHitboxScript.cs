using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitboxScript : MonoBehaviour
{
    public WorldPlayer player;

    public new CapsuleCollider collider;
    public GameObject visualCollider;

    public bool hitboxActive = false;

    public Vector3 lastPos;

    // Start is called before the first frame update
    void Start()
    {
        player = WorldPlayer.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = WorldPlayer.Instance;
        }
        lastPos = transform.position;
        switch (player.GetActionState())
        {
            case WorldPlayer.ActionState.Slash:
                hitboxActive = true;
                if (visualCollider != null)
                {
                    visualCollider.SetActive(true);
                }
                collider.enabled = true;
                collider.radius = WorldPlayer.SWORD_HITBOX_RADIUS;
                if (visualCollider != null)
                {
                    visualCollider.transform.localScale = Vector3.one * WorldPlayer.SWORD_HITBOX_RADIUS * 2;
                }

                float slashProgress = player.GetVisualAttackTime() / WorldPlayer.SWORD_SWING_TIME;
                if (slashProgress >= 1)
                {
                    slashProgress = 1;
                }

                float slashAngle = (-MainManager.EasingQuadratic(slashProgress, 1) + 0.5f) * WorldPlayer.SWORD_ANGLE_SPREAD;

                float offset = WorldPlayer.SWORD_HITBOX_OFFSET;
                transform.localPosition = Vector3.forward * offset * Mathf.Sin(slashAngle * (Mathf.PI / 180f)) + Vector3.right * offset * Mathf.Cos(slashAngle * (Mathf.PI / 180f));

                break;
            case WorldPlayer.ActionState.Smash:
                hitboxActive = true;
                if (visualCollider != null)
                {
                    visualCollider.SetActive(true);
                }
                collider.enabled = true;

                float smashProgress = player.GetVisualAttackTime() / WorldPlayer.HAMMER_SWING_TIME;
                if (smashProgress >= 1)
                {
                    smashProgress = 1;
                }

                if (smashProgress >= 1 || player.GetAttackTime() != player.GetVisualAttackTime())
                {
                    collider.radius = WorldPlayer.HAMMER_BIG_RADIUS;
                    if (visualCollider != null)
                    {
                        visualCollider.transform.localScale = Vector3.one * WorldPlayer.HAMMER_BIG_RADIUS * 2;
                    }
                } else
                {
                    collider.radius = WorldPlayer.HAMMER_HITBOX_RADIUS;
                    if (visualCollider != null)
                    {
                        visualCollider.transform.localScale = Vector3.one * WorldPlayer.HAMMER_HITBOX_RADIUS * 2;
                    }
                }

                float smashAngle = (1 - MainManager.EasingQuadratic(smashProgress, 1)) * WorldPlayer.HAMMER_ANGLE_SPREAD + WorldPlayer.HAMMER_DOWN_ANGLE;

                float offsetB = WorldPlayer.HAMMER_HITBOX_OFFSET;

                transform.localPosition = Vector3.up * offsetB * Mathf.Sin(smashAngle * (Mathf.PI / 180f)) + Vector3.right * offsetB * Mathf.Cos(smashAngle * (Mathf.PI / 180f));

                break;
            default:
                hitboxActive = false;
                if (visualCollider != null)
                {
                    visualCollider.SetActive(false);
                }
                collider.enabled = false;

                transform.localPosition = Vector3.right * 0.5f;
                break;
        }
    }

    bool HitboxActive()
    {
        /*
        switch (player.GetActionState())
        {
            case WorldPlayer.ActionState.Slash:
            case WorldPlayer.ActionState.Smash:
                return true;
            default:
                return false;
        }
        */
        return hitboxActive;
    }

    Vector3 GetFacingVector()
    {
        return player.FacingVector();
    }

    Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessCollision(other);
    }

    public void OnTriggerStay(Collider other)
    {
        ProcessCollision(other);
    }

    public void ProcessCollision(Collider other)
    {
        //Note: Colliding with a real collider that does not have a slash/smash trigger will interrupt the weapon hit
        //Colliders with triggers can decide whether to interrupt or not (i.e. slashable vines won't interrupt slashing)
        bool interrupt = false;
        if (HitboxActive())
        {
            interrupt = true;
            if ((other.isTrigger && !other.gameObject.CompareTag("SolidTrigger")) || other.gameObject.CompareTag("InvisibleWall"))
            {
                //triggers won't interrupt by default (but some wacky triggers can do that I guess if you want to smash thin air for some reason)
                interrupt = false;
            }

            switch (player.GetActionState())
            {
                case WorldPlayer.ActionState.Slash:
                    if (other.transform.GetComponent<ISlashTrigger>() != null)
                    {
                        interrupt = other.transform.GetComponent<ISlashTrigger>().Slash(GetFacingVector(), GetPlayerPosition());
                    }
                    break;
                case WorldPlayer.ActionState.Smash:
                    if (other.transform.GetComponent<ISmashTrigger>() != null)
                    {
                        interrupt = other.transform.GetComponent<ISmashTrigger>().Smash(GetFacingVector(), GetPlayerPosition());
                    }
                    break;
            }
        }
        //Debug.Log(player.GetVisualAttackTime() + " " + WorldPlayer.INTERRUPT_LENIENCY);
        bool secondaryCheck = false;
        if (player.GetActionState() == WorldPlayer.ActionState.Slash && player.GetVisualAttackTime() > WorldPlayer.SWORD_INTERRUPT_LENIENCY)
        {
            secondaryCheck = true;
        }
        if (player.GetActionState() == WorldPlayer.ActionState.Smash && player.GetVisualAttackTime() > WorldPlayer.HAMMER_INTERRUPT_LENIENCY)
        {
            secondaryCheck = true;
        }
        if (interrupt && secondaryCheck)
        {
            //Final check to make oblique hits not fail
            //Note: this seems buggy (Mesh colliders act a bit sus)
            Vector3 checkPos = other.ClosestPointOnBounds(transform.position);
            checkPos -= transform.position;

            //float checker = Vector3.Dot(player.FacingVector(), checkPos);
            //Debug.Log(checker + " " + checkPos + " " + player.FacingVector());

            //
            //if (checker > 0.5f || checkPos == Vector3.zero)

            //My dot product setup didn't work right :(
            if (checkPos.magnitude < 0.3f)
            {
                player.InterruptWeapon(lastPos, transform.position);
            }
        }
    }
}
