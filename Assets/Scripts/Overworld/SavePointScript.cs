using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SavePointScript : WorldObject, ITextSpeaker, IDashHopTrigger, ISlashTrigger, ISmashTrigger, IStompTrigger, IHeadHitTrigger
{
    public MeshRenderer sphere;
    private MaterialPropertyBlock propertyBlockA;

    public float hitAnimTime = 0;
    public float saveCooldown = 0;
    public bool saveActive = false;

    public float lifetime = 0;

    void Start()
    {
        if (sphere != null)
        {
            propertyBlockA = new MaterialPropertyBlock();
        }
    }

    public void SaveEffect()
    {
        GameObject eo = null;
        EffectScript_Sparkle es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Effect_Sparkle"), gameObject.transform);
        eo.transform.position = transform.position;
        es_s = eo.GetComponent<EffectScript_Sparkle>();
        es_s.Setup(new Color(0.2f, 1.0f, 1.0f, 1.0f), 0.5f, 8, 0.25f, 0.75f);
    }

    public override void WorldUpdate()
    {
        lifetime += Time.deltaTime;
        if (sphere != null)
        {
            float rotationDuration = 3;
            float time = (lifetime % rotationDuration) / rotationDuration;
            propertyBlockA.SetFloat("_AngleDelta", time);
            sphere.SetPropertyBlock(propertyBlockA);

            if (hitAnimTime > 0)
            {
                hitAnimTime -= Time.deltaTime;
            } else
            {
                hitAnimTime = 0;
            }

            sphere.transform.localScale = Vector3.one * Mathf.Max(1.25f * hitAnimTime + 0.75f, 1 + Mathf.Sin(lifetime) * 0.125f);
        }

        if (saveCooldown > 0)
        {
            saveCooldown -= Time.deltaTime;
        } else
        {
            saveCooldown = 0;
        }
    }

    public void StartSave()
    {
        if (saveCooldown > 0 || saveActive)
        {
            return;
        }

        SaveEffect();
        saveCooldown = 0.5f;
        hitAnimTime = 0.5f;
        StartCoroutine(SaveCutscene());
    }

    public IEnumerator SaveCutscene()
    {
        saveActive = true;
        string[][] testTextFile = new string[4][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[0][0] = "Do you want to save?<prompt,Save,1,Don't save,2,1>";
        testTextFile[1][0] = "Saving...<end>";  //end tag forces this to end early
        testTextFile[2][0] = "Save completed.";
        testTextFile[3][0] = "<color,red>Save failed. Check file integrity.</color>";

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out int state);

        if (state == 2)
        {
            saveActive = false;
            saveCooldown = 0.5f;
            yield break;
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));

        bool success = MainManager.Instance.Save();

        if (success)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
        }
        else
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
        }

        yield return null;
        saveActive = false;
        saveCooldown = 0.5f;
    }


    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        StartSave();
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        StartSave();
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        StartSave();
        return true;
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        StartSave();
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        StartSave();
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
