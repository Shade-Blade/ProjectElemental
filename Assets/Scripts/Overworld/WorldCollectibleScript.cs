using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldCollectibleScript : WorldObject
{
    public const float ITEM_HITBOX_SIZE = 0.275f;
    public const float SMALL_HITBOX_SIZE = 0.15f;

    public float attractRadius;
    public float attractDelta;
    public bool attract;
    public bool antigravity;
    public bool intangible; //blocks you from collecting or attracting
    private bool doAttract;

    public bool touchingObject = false;

    public WorldPlayer worldPlayer;

    public PickupUnion pickupUnion;

    public SphereCollider colliderC;
    public SphereCollider colliderI;
    public SpriteRenderer sprite;

    public bool setup = false;

    public string globalFlag;
    public string areaFlag = "";

    public Vector3 startPos;

    //Why
    public bool isCollected;

    public float lifetime;
    public float maxLifetime;
    public bool isFlickering;
    public const float NO_INTERACT_TIME = 0.4f; //time that collectibles aren't attracted or collectible (prevents you from instantly collecting certain things)
    public const float FLICKER_LIFETIME = 3f;   //flicker if close to despawning

    public static WorldCollectibleScript MakeCollectible(PickupUnion pu, Vector3 position, Vector3 velocity = default, float maxLifetime = 10)
    {
        //Debug.Log("Drop something: " + pu.type);
        WorldCollectibleScript wcs = Instantiate(MainManager.Instance.defaultCollectible, MainManager.Instance.mapScript.transform).GetComponent<WorldCollectibleScript>();
        wcs.Setup(pu);
        wcs.Mutate();
        wcs.transform.position = position;
        wcs.startPos = position;
        wcs.rb.velocity = velocity;
        wcs.lifetime = 0;
        wcs.globalFlag = "";
        wcs.areaFlag = "";
        wcs.maxLifetime = maxLifetime;
        return wcs;
    }


    public void Setup(PickupUnion pu)
    {
        pickupUnion = pu.Copy();
        transform.localScale = Vector3.one * PickupUnion.GetScale(pu);
        sprite.sprite = PickupUnion.GetSprite(pu);
        sprite.material = MainManager.Instance.defaultSpriteMaterial;
        
        switch (pu.type)
        {
            case PickupUnion.PickupType.Coin:
            case PickupUnion.PickupType.SilverCoin:
            case PickupUnion.PickupType.GoldCoin:
                attract = true;
                break;
            case PickupUnion.PickupType.None:
            case PickupUnion.PickupType.Shard:
            case PickupUnion.PickupType.Item:
            case PickupUnion.PickupType.KeyItem:
            case PickupUnion.PickupType.Badge:
            case PickupUnion.PickupType.Ribbon:
            case PickupUnion.PickupType.Misc:
                attract = false;
                break;
        }

        if (pu.type == PickupUnion.PickupType.Item)
        {
            sprite.material = GlobalItemScript.GetItemModifierMaterial(pu.item.modifier);
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        setup = true;
    }

    public void Mutate()
    {
        pickupUnion.Mutate();
    }
    public void Unmutate()
    {
        pickupUnion.Unmutate();
    }

    public override void ProcessCollision(Collision collision)
    {
        touchingObject = true;
    }

    public void Start()
    {
        worldPlayer = WorldPlayer.Instance;
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (!setup)
        {
            Mutate();
            Setup(pickupUnion);
            startPos = transform.position;
        }

        if (antigravity)
        {
            rb.useGravity = false;
        }

        bool delete = false;
        if (globalFlag.Length > 1)
        {
            delete = MainManager.Instance.GetGlobalFlag(globalFlag);
        }
        else if (areaFlag.Length > 1)
        {
            delete = MainManager.Instance.GetAreaFlag(areaFlag);
        }
        if (delete)
        {
            Destroy(gameObject);
        }
    }

    public override void WorldUpdate()
    {
        sprite.transform.eulerAngles = Vector3.up * MainManager.Instance.GetWorldspaceYaw();
        if (!MainManager.Instance.inCutscene)   //Pause despawn timer in cutscenes
        {
            lifetime += Time.deltaTime;
        }
        if (maxLifetime != 0)
        {
            if (!isFlickering && lifetime > maxLifetime - FLICKER_LIFETIME)
            {
                isFlickering = true;
                sprite.material = MainManager.Instance.defaultSpriteFlickerMaterial;
            }
            if (lifetime > maxLifetime)
            {
                //Debug.Log("Lifetime expired");
                Destroy(gameObject);
            }
        }
    }

    public override void WorldFixedUpdate()
    {
        if (worldPlayer == null)
        {
            worldPlayer = WorldPlayer.Instance;
        }
        if (antigravity)
        {
            rb.useGravity = false;
        } else if (!doAttract)
        {
            rb.useGravity = true;
        }
        //remove this? what if a cutscene shoots out an item somehow
        if (worldPlayer != null && MainManager.Instance.inCutscene)
        {
            //if (!rb.isKinematic)
            //{
            //    bufferVelocity = rb.velocity;
            //}

            //rb.isKinematic = true;
        } else
        {
            //if (rb.isKinematic)
            //{
            //    rb.velocity = bufferVelocity;
            //}

            //rb.isKinematic = false;
        }
        if (lifetime > NO_INTERACT_TIME && !intangible && attract && worldPlayer != null)
        {
            Vector3 delta = (transform.position - worldPlayer.transform.position);
            if (doAttract || delta.magnitude < attractRadius && delta != Vector3.zero)
            {
                doAttract = true;
                rb.useGravity = false;
                rb.velocity = -delta.normalized * ((1 / (0.1f + delta.magnitude)) + 0.3f) * attractDelta * Time.fixedDeltaTime;
            } 

            //snap
            //prevent situation where coins whip around you when you can't collect them in a dialogue box
            if (doAttract && rb.velocity.magnitude * Time.fixedDeltaTime > delta.magnitude)
            {
                if (rb.velocity.magnitude > 0)
                {
                    rb.velocity = rb.velocity * (delta.magnitude / (rb.velocity.magnitude * Time.fixedDeltaTime));
                }

            }

            /*else
            {
                rb.velocity -= 0.25f * (rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward);
            }*/
        }

        if (transform.position.y < -50)
        {
            Debug.LogWarning("Collectible fell into the void: " + gameObject.name + " at " + transform.position);
            transform.position = startPos;
            startPos = startPos + Vector3.up * 0.1f;   //if it falls in again, offset the position upward until it works (Note: may keep going if it clips through the ground after some point)
            rb.velocity = Vector3.zero;

            //Idea
            RaycastHit rc;
            if (Physics.Raycast(startPos, Vector3.down, out rc, 100, 311))
            {
                transform.position = rc.point + Vector3.up * 0.25f;
            }
        }

        if (touchingObject)
        {
            rb.velocity = (rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward) * Mathf.Pow(0.75f, Time.fixedDeltaTime) + rb.velocity.y * Vector3.up;
        }

        touchingObject = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }

    public void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }

    public void ProcessTrigger(Collider other)
    {
        WorldPlayer w = other.transform.GetComponent<WorldPlayer>();

        if (lifetime > NO_INTERACT_TIME && !intangible && w != null && !MainManager.Instance.inCutscene)
        {
            Pickup();
        }
    }

    public void Pickup()
    {
        if (!isCollected)
        {
            isCollected = true;
            switch (pickupUnion.type)
            {
                case PickupUnion.PickupType.None:
                    break;
                case PickupUnion.PickupType.Coin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Coin);
                    break;
                case PickupUnion.PickupType.SilverCoin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Coin);
                    break;
                case PickupUnion.PickupType.GoldCoin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 2);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Coin);
                    break;
                case PickupUnion.PickupType.Shard:
                    Particles(new Color(0.25f, 0.75f, 0.75f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Shard);
                    break;
                case PickupUnion.PickupType.Item:
                    Particles(new Color(0.25f, 0.75f, 0.25f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Item);
                    break;
                case PickupUnion.PickupType.KeyItem:
                    Particles(new Color(0.25f, 0.75f, 0.25f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_KeyItem);
                    break;
                case PickupUnion.PickupType.Badge:
                    Particles(new Color(0.75f, 0.25f, 0.25f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Badge);
                    break;
                case PickupUnion.PickupType.Ribbon:
                    Particles(new Color(0.75f, 0.25f, 0.75f, 1f), 1);
                    MainManager.Instance.PlaySound(WorldPlayer.Instance.gameObject, MainManager.Sound.SFX_Pickup_Ribbon);
                    break;
            }

            if (globalFlag.Length > 1)
            {
                MainManager.Instance.SetGlobalFlag(globalFlag, true);
            }
            else if (areaFlag.Length > 1)
            {
                MainManager.Instance.SetAreaFlag(areaFlag, true);
            }

            MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(pickupUnion));

            //Debug.Log("Interact destroy");
            Destroy(gameObject);
        }
    }

    public IEnumerator PickupCoroutine()
    {
        if (!isCollected)
        {
            isCollected = true;
            switch (pickupUnion.type)
            {
                case PickupUnion.PickupType.None:
                    break;
                case PickupUnion.PickupType.Coin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 1);
                    break;
                case PickupUnion.PickupType.SilverCoin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 1);
                    break;
                case PickupUnion.PickupType.GoldCoin:
                    Particles(new Color(1f, 0.75f, 0.5f, 1f), 2);
                    break;
                case PickupUnion.PickupType.Shard:
                    Particles(new Color(0.25f, 0.75f, 0.75f, 1f), 1);
                    break;
                case PickupUnion.PickupType.Item:
                    Particles(new Color(0.25f, 0.75f, 0.25f, 1f), 1);
                    break;
                case PickupUnion.PickupType.KeyItem:
                    Particles(new Color(0.25f, 0.75f, 0.25f, 1f), 1);
                    break;
                case PickupUnion.PickupType.Badge:
                    Particles(new Color(0.75f, 0.25f, 0.25f, 1f), 1);
                    break;
                case PickupUnion.PickupType.Ribbon:
                    Particles(new Color(0.75f, 0.25f, 0.75f, 1f), 1);
                    break;
            }

            if (globalFlag.Length > 1)
            {
                MainManager.Instance.SetGlobalFlag(globalFlag, true);
            }
            else if (areaFlag.Length > 1)
            {
                MainManager.Instance.SetAreaFlag(areaFlag, true);
            }

            //Dangerous? the rest of this script executes without this game object existing
            //Should be fine though since this immediately gets passed to pickup
            Destroy(gameObject);

            yield return MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(pickupUnion));
        }

        yield return null;
    }

    public void Particles(Color color, int power)
    {
        //int power = 2;

        float newScale = 0.75f; 
        if (worldPlayer == null)
        {
            worldPlayer = WorldPlayer.Instance;
        }
        Vector3 position = worldPlayer.transform.position + Vector3.up * (0.375f);

        GameObject eo = null;
        EffectScript_Sparkle es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Effect_Sparkle"), MainManager.Instance.mapScript.gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Sparkle>();
        es_s.Setup(color, 0.5f, power, 0.25f, newScale);
    }
}
