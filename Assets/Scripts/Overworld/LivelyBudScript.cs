using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivelyBudScript : WorldObject, ITextSpeaker, IDashHopTrigger, ISlashTrigger, ISmashTrigger, IStompTrigger, IHeadHitTrigger
{
    public MeshRenderer model;
    public MeshRenderer leaves;

    public float hitAnimDuration = 0.1f;
    public float hitAnimTime = 0;
    public float healCooldown = 0;
    public bool healActive = false;

    public float lifetime = 0;

    public void HealEffect()
    {
        GameObject eo = null;
        EffectScript_Sparkle es_s = null;

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.Menu_LivelyBud);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Effect_Sparkle"), gameObject.transform);
        eo.transform.position = transform.position;
        es_s = eo.GetComponent<EffectScript_Sparkle>();
        es_s.Setup(new Color(1.0f, 0.2f, 1.0f, 1.0f), 0.5f, 8, 0.25f, 0.75f);

        List<WorldEntity> healedEntities = new List<WorldEntity>();
        if (WorldPlayer.Instance != null)
        {
            healedEntities.Add(WorldPlayer.Instance);

            for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
            {
                healedEntities.Add(WorldPlayer.Instance.followers[i]);
            }
        }

        for (int i = 0; i < healedEntities.Count; i++)
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Effect_Sparkle"), gameObject.transform);
            eo.transform.position = healedEntities[i].transform.position;
            es_s = eo.GetComponent<EffectScript_Sparkle>();
            es_s.Setup(new Color(1.0f, 0.8f, 0.5f, 1.0f), 0.5f, 6, 0.25f, 0.5f);
        }
    }

    public override void WorldUpdate()
    {
        lifetime += Time.deltaTime;
        if (model != null)
        {
            float rotationDuration = 3;
            float time = (lifetime % rotationDuration) / rotationDuration;

            if (hitAnimTime > 0)
            {
                hitAnimTime -= Time.deltaTime;
            }
            else
            {
                hitAnimTime = 0;
            }

            float upScale = Mathf.Max(0.5f * (hitAnimTime / hitAnimDuration) + 1f, 1 + Mathf.Sin(lifetime * 2) * 0.0625f);
            float sideScale = Mathf.Min(-0.5f * (hitAnimTime / hitAnimDuration) + 1f, 1.35f - Mathf.Sin(lifetime * 2) * 0.0625f);

            model.transform.localScale = (Vector3.forward + Vector3.right) * sideScale + Vector3.up * upScale;
            //interpolate between 1 and model scale
            //so that they move separately somewhat
            leaves.transform.localScale = Vector3.Lerp(Vector3.one, model.transform.localScale, (hitAnimTime / hitAnimDuration) * 0.75f + 0.25f);
        }

        if (healCooldown > 0)
        {
            healCooldown -= Time.deltaTime;
        }
        else
        {
            healCooldown = 0;
        }
    }

    public void StartHeal()
    {
        if (healCooldown > 0 || healActive)
        {
            return;
        }

        HealEffect();
        MainManager.Instance.playerData.FullHeal();
        MainManager.Instance.SetHUDTime();
        healCooldown = 0.5f;
        hitAnimTime = hitAnimDuration;
        StartCoroutine(SaveCutscene());
    }

    public IEnumerator SaveCutscene()
    {
        healActive = true;
        yield return null;
        healCooldown = 0.5f;
        healActive = false;
    }


    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        StartHeal();
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        StartHeal();
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        StartHeal();
        return true;
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        StartHeal();
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        StartHeal();
    }



    public string RequestTextData(string request)
    {
        return "";
    }

    public void SendTextData(string data)
    {

    }

    public void EnableSpeakingAnim()
    {
    }

    public bool SpeakingAnimActive()
    {
        return false;
    }

    public void DisableSpeakingAnim()
    {
    }

    public void SetAnimation(string animationID, bool force = false)
    {
    }

    public void SendAnimationData(string data)
    {
    }

    public Vector3 GetTextTailPosition()
    {
        return transform.position;
    }

    public void TextBleep()
    {
    }

    public void SetFacing(Vector3 facingTarget)
    {
    }

    public void EmoteEffect(TagEntry.Emote emote)
    {
    }
}
