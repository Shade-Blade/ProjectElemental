using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    //controls creation of player entities
    //controls player actions during player turn
    private static PlayerTurnController instance;
    public static PlayerTurnController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerTurnController>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    /*
    public enum MenuState
    {
        None,
        BaseMenu,
        MoveMenu,
        ItemMenu,
        TacticMenu,
        Selection,
        OmniSelection,
        MessageBox
    }
    */
    //public MenuState menuState; //unused

    public bool inMenu;
    public MenuHandler menu; //holds the base menu
    public List<PlayerEntity> movableParty; //can move, and hasn't moved
    public int maxMovable;
    public List<PlayerEntity> immovableParty; //can't move, or has already moved

    //I have to handle double bite specifically
    //This feels like spaghetti but I have no choice
    public MetaItem_Multi MultiSupply;
    public bool MultiSupplyCancel;


    public const bool DEBUG_PRINTING = true;

    public enum MenuExitType {
        None,
        MoveExecute,
        ActionExecute,
        Switch,
        SuperSwap,
        Null,       //error condition (can't do anything)
        Interrupt
    }
    public MenuExitType mexitType = MenuExitType.None;
    public bool interruptQueued;
    public MenuResult mresult;
    //public PlayerData.PlayerDataEntry toSwitch;

    //use this to globally add the shared tactics and moves
    public T GetOrAddComponent<T>() where T : Component
    {
        if (gameObject.GetComponent<T>())
            return gameObject.GetComponent<T>();
        else
            return gameObject.AddComponent<T>() as T;
    }

    public bool MaxMovable()
    {
        return movableParty.Count == maxMovable;
    }
    public int LivePlayerCount()
    {
        return BattleControl.Instance.GetEntities((e) => (e.posId <= 0) && e.hp > 0).Count;
    }

    public void SetPlayerSprites()
    {
        foreach (PlayerEntity p in movableParty)
        {
            p.SetInactiveColor(false);
        }
        foreach (PlayerEntity p in immovableParty)
        {
            p.SetInactiveColor(true);
        }
    }
    public void ResetPlayerSprites()
    {
        foreach (PlayerEntity p in BattleControl.Instance.GetPlayerEntities())
        {
            p.SetInactiveColor(false);
        }
    }
    public void ResetActionCommands()
    {
        /*
        if (movableParty != null)
        {
            for (int i = 0; i < movableParty.Count; i++)
            {
                movableParty[i].actionCommandSuccesses = 0;
                movableParty[i].blockSuccesses = 0;
            }
        }
        if (immovableParty != null)
        {
            for (int i = 0; i < immovableParty.Count; i++)
            {
                immovableParty[i].actionCommandSuccesses = 0;
                immovableParty[i].blockSuccesses = 0;
            }
        }
        */
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities(false);
        foreach (PlayerEntity p in players)
        {
            p.actionCommandSuccesses = 0;
            p.blockSuccesses = 0;
        }
    }

    public void ResetGuardTimers()
    {
        if (immovableParty == null)
        {
            immovableParty = BattleControl.Instance.GetPlayerEntities();
        }
        if (movableParty != null)
        {
            for (int i = 0; i < movableParty.Count; i++)
            {
                movableParty[i].ResetGuardTimers();
            }
        }
        if (immovableParty != null)
        {
            for (int i = 0; i < immovableParty.Count; i++)
            {
                immovableParty[i].ResetGuardTimers();
            }
        }
    }

    public void MakeMobile(PlayerEntity b)
    {
        if (immovableParty.Contains(b))
        {
            movableParty.Add(b);
            immovableParty.Remove(b);
        }
    }
    public void MakeImmobile(PlayerEntity b)
    {
        if (movableParty.Contains(b))
        {
            movableParty.Remove(b);
            immovableParty.Add(b);
            b.SetInactiveColor(true);
        }
    }
    public void MakeImmobileStacking(PlayerEntity b)
    {
        if (!b.CanMove())
        {
            b.InflictEffect(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION), Effect.NULL_CASTERID, Effect.EffectStackMode.AdditivePot);
        }

        if (b.HasEffect(Effect.EffectType.BonusTurns))
        {
            b.TokenRemoveOne(Effect.EffectType.BonusTurns);
        } else
        {
            if (movableParty.Contains(b))
            {
                movableParty.Remove(b);
                immovableParty.Add(b);
                b.SetInactiveColor(true);
            }
        }
    }
    public IEnumerator TakeTurn()
    {
        movableParty = new List<PlayerEntity>();

        //movableParty = BattleControl.Instance.GetEntities((e) => (e.posId < 0));
        movableParty = BattleControl.Instance.GetPlayerEntities();

        foreach (PlayerEntity p in movableParty)
        {
            p.SetInactiveColor(false);
        }

        foreach (PlayerEntity p in movableParty)
        {
            //premove here
            yield return StartCoroutine(p.PreMove());

            //Debug.Log(p.attackLastTurn);

            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Sloth))
            {
                if (!p.HasEffect(Effect.EffectType.Slow))
                {
                    p.InflictEffectForce(p, new Effect(Effect.EffectType.Slow, 1, Effect.INFINITE_DURATION));
                }
            }
        }

        //death check?
        for (int i = 0; i < movableParty.Count; i++)
        {
            movableParty[i].DeathCheck();
        }

        immovableParty = movableParty.FindAll((e) => !e.CanMove());
        movableParty = movableParty.FindAll((e) => e.CanMove());
        maxMovable = movableParty.Count;

        foreach (PlayerEntity p in immovableParty)
        {
            p.SetInactiveColor(true);
        }

        //redeem cooldown tokens
        List<PlayerEntity> cooldownParty = movableParty.FindAll((e) => e.HasEffect(Effect.EffectType.Cooldown));
        for (int i = 0; i < cooldownParty.Count; i++)
        {
            movableParty.Remove(cooldownParty[i]);
            immovableParty.Add(cooldownParty[i]);
            cooldownParty[i].SetInactiveColor(true);
            cooldownParty[i].TokenRemoveOne(Effect.EffectType.Cooldown);
        }

        //better place to do this then at EOT
        GlobalItemScript.Instance.ClearItemMoves();

        Debug.Log("--PLAYER TURN--");

        //failsafe to prevent wrong animations
        foreach (PlayerEntity pe in movableParty)
        {
            pe.SetIdleAnimation();
        }
        foreach (PlayerEntity pe in immovableParty)
        {
            pe.SetIdleAnimation();
        }

        //BattleControl.Instance.UpdatePlayerData();

        //Make the front character move first by default (just a personal stylistic thing)
        //also prioritize berserk

        movableParty.Sort((a, b) =>
        {
            //positive means A is before
            if (a.AutoMove() ^ b.AutoMove())
            {
                return a.AutoMove() ? 1 : -1;
            }

            int c = -(a.posId - b.posId);

            return c;
        });

        //Dispatch scripts specific for each player
        while (movableParty.Count > 0)
        {
            SetPlayerSprites();
            int movableCount = movableParty.Count;

            PlayerEntity mover = movableParty[0];

            //berserk gets priority
            //push them up the list if there is one
            //(but lazy pushing where only the first one gets to the front is probably fine)

            if (!movableParty[0].AutoMove() && movableCount > 1)
            {
                for (int i = 1; i < movableCount; i++)
                {
                    if (movableParty[i].AutoMove())
                    {
                        PlayerEntity p = movableParty[i];
                        movableParty.Remove(p);
                        movableParty.Insert(0, p);
                        mover = p;
                        break;
                    }
                }
            }

            Debug.Log(mover);
            //Debug.Log(mover.actionCounter);

            //Normally do your turn
            //new change: MultiSupplyCancel forces it to go back around
            do
            {
                MultiSupplyCancel = false;
                yield return StartCoroutine(TakeTurnSpecific(mover));
            } while (MultiSupplyCancel);

            //Death check
            mover.DeathCheck();
            yield return new WaitUntil(() => (!mover.immediateInEvent));

            List<PlayerEntity> newImmovable = movableParty.FindAll((e) => (!e.CanMove() || e.HasEffect(Effect.EffectType.Cooldown)));

            //Check for enemy special events
            if (mexitType != MenuExitType.Switch || newImmovable.Count > 0)
            {
                yield return StartCoroutine(BattleControl.Instance.RunOutOfTurnEvents());
            }

            if (newImmovable.Count > 0)
            {
                movableParty = movableParty.FindAll((e) => !newImmovable.Contains(e));
                for (int i = 0; i < newImmovable.Count; i++)
                {
                    newImmovable[i].DeathCheck();
                    //note: mover will be put in the immovable list later
                    if (newImmovable[i].CanMove() && newImmovable[i].HasEffect(Effect.EffectType.Cooldown) && newImmovable[i] != mover)
                    {
                        newImmovable[i].TokenRemoveOne(Effect.EffectType.Cooldown);
                    }
                    immovableParty.Add(newImmovable[i]);
                    newImmovable[i].SetInactiveColor(true);
                }
            }

            //weird special case
            List<PlayerEntity> deleteList = movableParty.FindAll((e) => (!BattleControl.IsPlayerControlled(e, true)));
            if (deleteList.Count > 0)
            {
                movableParty = movableParty.FindAll((e) => (BattleControl.IsPlayerControlled(e, true)));
            }
            deleteList = immovableParty.FindAll((e) => (!BattleControl.IsPlayerControlled(e, true)));
            if (deleteList.Count > 0)
            {
                immovableParty = immovableParty.FindAll((e) => (BattleControl.IsPlayerControlled(e, true)));
            }
            SetPlayerSprites();

            //yield return StartCoroutine(BattleControl.Instance.CheckEndBattle());

            if (mexitType == MenuExitType.Interrupt)
            {
                //Do nothing
                Debug.Log("interrupt");
            } else if (mexitType == MenuExitType.Switch) 
            {
                PlayerEntity b = mover;
                if (movableParty.Contains(b))
                {
                    movableParty.RemoveAt(0);
                    movableParty.Add(b);
                }
            }
            /*
            else if (mexitType == MenuExitType.SwitchCharacter)
            {
                //clean up the lists
                for (int i = 0; i < movableParty.Count; i++)
                {
                    //Debug.Log("Contains: "+ BattleControl.Instance.HasEntity(movableParty[i]) + " "+movableParty[i].entityID);
                    if (!BattleControl.Instance.HasEntity(movableParty[i]))
                    {
                        movableParty.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < immovableParty.Count; i++)
                {
                    //Debug.Log("Contains: " + BattleControl.Instance.HasEntity(immovableParty[i]) + " " + immovableParty[i].entityID);
                    if (!BattleControl.Instance.HasEntity(immovableParty[i]))
                    {
                        immovableParty.RemoveAt(i);
                        i--;
                    }
                }

                List<BattleEntity> totalEntities = new List<BattleEntity>();
                totalEntities = BattleControl.Instance.GetEntities((e) => (e.posId < 0));
                int index = -1;
                for (int i = 0; i < totalEntities.Count; i++)
                {
                    if (!movableParty.Contains(totalEntities[i]) && !immovableParty.Contains(totalEntities[i]))
                    {
                        index = i;
                        break;
                    }
                }
                BattleEntity newEntity = totalEntities[index];
                //Debug.Log(newEntity.id + " " + newEntity.entityID);

                //determine which list to add to
                if (movableParty.Count <= movableCount - 2)
                {
                    movableParty.Add(newEntity);
                } else
                {
                    immovableParty.Add(newEntity);
                }
            }
            */
            else 
            {
                //Tick down quick bite if necessary
                //Debug.Log("quick bite tick " + mover.QuickSupply);
                if (mover.QuickSupply > 0)
                {
                    mover.QuickSupply--;
                }
                if (!mover.CanMove())
                {
                    mover.QuickSupply = 0;
                }

                //you took your turn, now you can't move anymore

                //Or redeem bonus turn tokens         
                if (movableParty.Count > 0 && movableParty[0] == mover)
                {
                    if (mover.HasEffect(Effect.EffectType.BonusTurns))
                    {
                        mover.TokenRemoveOne(Effect.EffectType.BonusTurns);
                    }
                    else
                    {
                        immovableParty.Add(mover);
                        mover.SetInactiveColor(true);
                        movableParty.RemoveAt(0);
                    }
                }

                //redeem other bonus turn tokens
                for (int i = 0; i < immovableParty.Count; i++)
                {
                    PlayerEntity pe = immovableParty[i];
                    if (pe.HasEffect(Effect.EffectType.BonusTurns))
                    {
                        pe.TokenRemoveOne(Effect.EffectType.BonusTurns);
                        movableParty.Add(pe);
                        immovableParty.Remove(pe);
                        pe.SetInactiveColor(false);
                    }
                }

                //SetPlayerSprites();
            }
        }

        //need to keep item moves around due to reaction moves
        //GlobalItemScript.Instance.ClearItemMoves();
        Debug.Log("--PLAYER TURN END--");

        ResetPlayerSprites();

        yield return null;
    }

    //should probably refactor/rewrite this later (kind of hard to follow)
    //controls battle menu at a higher level (handles what you choose)
    public IEnumerator TakeTurnSpecific(PlayerEntity caller)
    {
        SetPlayerSprites();

        //debug
        if (DEBUG_PRINTING)
        {
            string infoString = "Information about current entity " + caller.GetName() + ":\n";
            infoString = infoString + caller.GetName() + ": Attack Bonus = E(" + caller.GetEffectAttackBonus() + ") B(" + caller.GetBadgeAttackBonus() + ")\n";
            infoString = infoString + caller.GetName() + ": Defense Bonus = E(" + caller.GetEffectDefenseBonus() + ") B(" + caller.GetBadgeDefenseBonus() + ")\n";
            infoString = infoString + caller.GetName() + ": Endurance Bonus = E(" + caller.GetEffectEnduranceBonus() + ") B(" + caller.GetBadgeEnduranceBonus() + ")\n";
            infoString = infoString + caller.GetName() + ": Haste Bonus = E(" + caller.GetEffectHasteBonus() + ") B(" + caller.GetBadgeAgilityBonus() + ")\n";
            infoString = infoString + caller.GetName() + ": Flow Bonus = E(" + caller.GetEffectFlowBonus() + ") B(" + caller.GetBadgeFlowBonus() + ")\n";
            infoString = infoString + caller.GetName() + ": Resistance Bonus = E(" + caller.GetEffectResistanceBonus() + ") B(" + caller.GetBadgeResistanceBonus() + ")";
            Debug.Log(infoString);
        }

        //cleanse the state
        if (MultiSupply != null)
        {
            Destroy(MultiSupply);
        }


        //if berserker: no menu
        if (caller.AutoMove())
        {
            yield return StartCoroutine(AutoMove(caller));

            yield break;
        }

        if (!caller.CanMove())
        {
            //No
            yield break;
        }

        inMenu = true;
        menu = BaseBattleMenu.buildMenu(caller);
        mexitType = MenuExitType.None;
        menu.transform.parent = transform;

        if (!caller.HasEffect(Effect.EffectType.Dizzy))
        {
            caller.SetAnimation("idlethinking");
        }

        //new system is that they get showed at the start of your turn and hidden as you execute the move
        BattleControl.Instance.ShowHPBars();
        BattleControl.Instance.ShowEffectIcons();
        BattleControl.Instance.RebuildStatDisplayers();

        while (true) //Repeat menu stuff since some menu stuff doesn't lose your turn
        {
            bool exit = false;  //escape the while loop (*but I set it up like this for some reason)

            //New setup: I can make the menu be interrupted
            //Problem is that if anything happens I have to delete the menu
            while (inMenu)
            {
                List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
                bool specialInterrupt = false;
                foreach (PlayerEntity pe in pel)
                {
                    if (pe.alive && pe.hp == 0)
                    {
                        //Illegal condition, force the game to interrupt stuff
                        specialInterrupt = true;
                    }
                }
                if (interruptQueued || specialInterrupt)
                {
                    menu.gameObject.SetActive(false);
                    bool interrupt = BattleControl.Instance.ReactionExists() || specialInterrupt;
                    if (interrupt)
                    {
                        menu.ActiveClear(); //make sure everything related to the current menu is gone
                        Destroy(menu.gameObject);
                        yield return StartCoroutine(BattleControl.Instance.RunOutOfTurnEvents());
                        interruptQueued = false;
                        mexitType = MenuExitType.Interrupt;
                        break;
                    }
                    else
                    {
                        interruptQueued = false;
                        menu.gameObject.SetActive(true);
                    }
                }
                yield return null;
            }
            //Wait for menu to resolve
            //yield return new WaitUntil(() => (!inMenu));


            //menu exited, but how?
            BattleControl.Instance.RebuildStatDisplayers();
            //Debug.Log(mexitType);
            switch (mexitType)
            {
                case MenuExitType.MoveExecute:
                case MenuExitType.Null:
                    exit = true;
                    break;
                case MenuExitType.ActionExecute:
                    //use menu details to set stuff
                    //mresult = menu.GetFullResult();
                    BattleAction b = (BattleAction)mresult.subresult.output;
                    b.ChooseAction(caller);
                    caller.QueueEvent(BattleHelper.Event.Tactic);
                    caller.curTarget = (BattleEntity)mresult.subresult.subresult.output;

                    //BattleAction b = menu.GetComponentInChildren<BoxMenu>().GetAction();
                    //caller.target = menu.GetComponentInChildren<BattleSelectionMenu>().GetCurrent();

                    menu.ActiveClear(); //make sure everything related to the current menu is gone
                    Destroy(menu.gameObject);

                    //enforce this
                    BattleControl.Instance.HideHPBars();
                    BattleControl.Instance.HideEffectIcons();

                    caller.SetIdleAnimation();
                    yield return b.Execute(caller);
                    //BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Tactic);

                    if (b.ForfeitTurn()) //this skips rebuilding the menu
                    {
                        caller.actionCounter++; //note: the forfeit turn actions will increment the counter (but not the 0 turn ones)
                        exit = true;
                        break;
                    }

                    yield return StartCoroutine(BattleControl.Instance.RunOutOfTurnEvents());
                    SetPlayerSprites();
                    //Debug.Log("g");


                    //rebuild menu
                    inMenu = true;
                    menu = BaseBattleMenu.buildMenu(caller);
                    mexitType = MenuExitType.ActionExecute;
                    menu.transform.parent = transform;

                    BattleControl.Instance.ShowHPBars();
                    BattleControl.Instance.ShowEffectIcons();
                    break;
                /*
                case MenuExitType.SwitchCharacter:
                    mresult = menu.GetFullResult();
                    toSwitch = (PlayerData.PlayerDataEntry)mresult.subresult.output;
                    caller.curTarget = (BattleEntity)mresult.subresult.subresult.output;

                    //toSwitch = menu.GetComponentInChildren<SwitchCharactersBoxMenu>().GetPDataEntry();
                    //caller.target = menu.GetComponentInChildren<BattleSelectionMenu>().GetCurrent();

                    //rebuild menu
                    menu.ActiveClear(); //make sure everything related to the current menu is gone
                    Destroy(menu.gameObject);

                    BattleAction ba = BattleControl.Instance.switchCharacter;
                    yield return ba.Execute(caller);

                    BattleControl.Instance.RebuildStatDisplayers();
                    exit = true;
                    break;
                */
                case MenuExitType.Switch:
                    exit = true;
                    menu.ActiveClear();
                    break;
                case MenuExitType.SuperSwap:
                    BattleAction ba2 = BattleControl.Instance.superSwap;

                    //Destroy menu before swapping!
                    menu.ActiveClear();
                    Destroy(menu.gameObject);

                    //enforce this
                    BattleControl.Instance.HideHPBars();
                    BattleControl.Instance.HideEffectIcons();

                    caller.SetIdleAnimation();
                    yield return ba2.Execute(caller);
                    //yield return StartCoroutine(BattleControl.Instance.RunOutOfTurnEvents());

                    BattleControl.Instance.RebuildStatDisplayers();

                    //failsafe thing for edge cases?
                    if (caller.AutoMove())
                    {
                        yield return StartCoroutine(AutoMove(caller));
                        yield break;
                    }

                    //rebuild menu
                    inMenu = true;
                    menu = BaseBattleMenu.buildMenu(caller);
                    mexitType = MenuExitType.SuperSwap;
                    menu.transform.parent = transform;

                    BattleControl.Instance.ShowHPBars();
                    BattleControl.Instance.ShowEffectIcons();
                    break;
                case MenuExitType.Interrupt:
                    exit = true;    //act like switch but don't actually do anything
                    //rebuild menu
                    inMenu = true;
                    menu = BaseBattleMenu.buildMenu(caller);
                    mexitType = MenuExitType.Interrupt;
                    menu.transform.parent = transform;

                    BattleControl.Instance.ShowHPBars();
                    BattleControl.Instance.ShowEffectIcons();
                    break;
            }
            if (exit)
            {
                caller.SetIdleAnimation();
                break;
            }
        }

        int moveLevel = 1;

        if (mexitType == MenuExitType.MoveExecute)
        {
            //use menu details to set stuff
            MenuResult m = mresult; //menu.GetFullResult();

            //BaseBattleMenu baseMenu = menu.GetComponent<BaseBattleMenu>();
            //BoxMenu boxMenu = menu.GetComponentInChildren<BoxMenu>();
            //BattleSelectionMenu selectionMenu = menu.GetComponentInChildren<BattleSelectionMenu>();

            /*
            switch ((BaseBattleMenu.BaseMenuName)m.output)
            {
                case BaseBattleMenu.BaseMenuName.Jump:
                    break;
                case BaseBattleMenu.BaseMenuName.Weapon:
                    break;
            }
            */

            /*
            if ((BaseBattleMenu.BaseMenuName)m.output == BaseBattleMenu.BaseMenuName.Jump)
            {
                caller.currMove = caller.moveset[0];
                caller.currMove.ChooseMove(caller);
                caller.curTarget = (BattleEntity)m.subresult.output;
            }
            */

            if (m.subresult.output is MoveBoxMenu.MoveMenuResult)
            {
                //jump, weapon, soul move end up here
                MoveBoxMenu.MoveMenuResult mmr = (MoveBoxMenu.MoveMenuResult)m.subresult.output;

                caller.currMove = mmr.playerMove;
                caller.currMove.ChooseMove(caller, mmr.level);
                moveLevel = mmr.level;
                caller.curTarget = (BattleEntity)m.subresult.subresult.output;
            } else
            {
                //item move ends up here
                if (m.subresult.output is MetaItemMove) //you have double bite, quick bite...
                {
                    //more stuff needs to be set up
                    //Double bite is extremely complex and needs to be handled specially
                    MetaItemMove mim = (MetaItemMove)m.subresult.output;
                    if (mim.GetMove() == MetaItemMove.Move.Multi)
                    {
                        //Very difficult to handle case
                        caller.currMove = (Move)m.subresult.output;

                        MetaItem_Multi multi = ((MetaItem_Multi)(caller.currMove));

                        multi.itemMove = (ItemMove)m.subresult.subresult.output;

                        if (multi.itemMoves == null)
                        {
                            multi.itemMoves = new List<ItemMove>();
                        }
                        if (multi.targets == null)
                        {
                            multi.targets = new List<BattleEntity>();
                        }
                        if (multi.indices == null)
                        {
                            multi.indices = new List<int>();
                        }

                        caller.curTarget = (BattleEntity)m.subresult.subresult.subresult.output;

                        multi.itemMoves.Add(multi.itemMove);
                        multi.targets.Add(caller.curTarget);                        

                        MultiSupply = multi;


                        bool MultiSupplyActive = true;


                        List<Item> inv = BattleControl.Instance.playerData.itemInventory;
                        List<bool> backgroundList = new List<bool>();  //also is the blocked list
                        List<Color> colorList = new List<Color>();


                        Color ColorByValue(int i)
                        {
                            return MainManager.ColorMap(1 + 2 * (i / (BattleControl.Instance.GetItemInventory(caller).Count + 0f)));
                        }


                        for (int i = 0; i < inv.Count; i++)
                        {
                            backgroundList.Add(false);
                            colorList.Add(ColorByValue(0));
                        }

                        //try to use the box menu to get the right item
                        BoxMenu b = FindObjectOfType<ItemBoxMenu>();
                        int index = b.menuIndex;

                        multi.indices.Add(index);
                        backgroundList[index] = true;
                        colorList[index] = ColorByValue(multi.itemMoves.Count - 1);

                        while (MultiSupplyActive)
                        {
                            //Now: Make a new menu!
                            //this is extremely sus coding
                            inMenu = true;

                            if (menu != null)
                            {
                                menu.ActiveClear(); //make sure everything related to the current menu is gone
                                Destroy(menu.gameObject);
                            }

                            menu = BoxMenu.BuildSpecialItemMenu(caller, inv, backgroundList, colorList, backgroundList, "Use <buttonsprite,Z> to end your turn early.");
                            b = (BoxMenu)menu;
                            mexitType = MenuExitType.None;
                            menu.transform.parent = transform;

                            //Wait for menu to resolve
                            yield return new WaitUntil(() => (!inMenu || (menu.GetActiveMenu() == null)));
                            index = b.menuIndex;
                            m = mresult; //menu.GetFullResult();
                            //if no active menu: double bite cancel
                            //(exit menu without inMenu being set properly)
                            if (menu.GetActiveMenu() == null && inMenu)
                            {
                                if (index == -2)
                                {
                                    //Z press
                                    //execute the move
                                    //empty thing
                                    inMenu = false;
                                    MultiSupplyActive = false;
                                    mexitType = MenuExitType.MoveExecute;
                                    caller.currMove.ChooseMove(caller, 1);
                                    //Debug.Log("Z press case");
                                } else
                                {
                                    //Debug.Log("Cancel case");
                                    inMenu = false;
                                    MultiSupplyCancel = true;
                                    MultiSupplyActive = false;
                                    menu.Clear(); //the menu can become inactive (thus GetActiveMenu returns null) so we have to make sure it gets really cleared
                                    Destroy(menu.gameObject);
                                    Destroy(multi);             //to cleanse the state
                                    MultiSupply = null;
                                    menu = null;
                                }
                            } else
                            {
                                inMenu = false;
                                //fill in more data
                                ItemMove im = (ItemMove)m.output;
                                BattleEntity target = (BattleEntity)m.subresult.output;

                                multi.itemMoves.Add(im);
                                multi.targets.Add(target);

                                b = FindObjectOfType<ItemBoxMenu>();
                                index = b.menuIndex;
                                multi.indices.Add(index);
                                backgroundList[index] = true;
                                colorList[index] = ColorByValue(multi.itemMoves.Count - 1);

                                //done
                                if (multi.itemMoves.Count >= (MainManager.Instance.Cheat_InfiniteBite ? 1000 : 3) || multi.itemMoves.Count >= BattleControl.Instance.GetItemInventory(caller).Count)
                                {
                                    MultiSupplyActive = false;
                                    caller.currMove.ChooseMove(caller, 1);
                                }
                            }
                        }

                    } else
                    {
                        //Easy to handle
                        caller.currMove = (Move)m.subresult.output;
                        ((MetaItemMove)(caller.currMove)).itemMove = (ItemMove)m.subresult.subresult.output;
                        caller.currMove.ChooseMove(caller, 1);
                        caller.curTarget = (BattleEntity)m.subresult.subresult.subresult.output;
                    }
                } else
                {
                    caller.currMove = (Move)m.subresult.output;
                    caller.currMove.ChooseMove(caller, 1);
                    caller.curTarget = (BattleEntity)m.subresult.subresult.output;
                }
            }

            /*
            if (baseMenu.baseMenuOptions[baseMenu.baseMenuIndex].oname == BaseBattleMenu.BaseMenuName.Attack)
            {
                caller.currMove = caller.moveset[0];
                caller.currMove.ChooseMove(caller);
                caller.target = selectionMenu.GetCurrent();
            }
            else
            {
                caller.currMove = boxMenu.GetCurrent();
                caller.currMove.ChooseMove(caller);
                caller.target = selectionMenu.GetCurrent();
            }
            */
        }

        if (menu != null)
        {
            //Debug.Log("Move execute case? " + mexitType);
            menu.Clear(); //the menu can become inactive (thus GetActiveMenu returns null) so we have to make sure it gets really cleared
            Destroy(menu.gameObject);

            if (mexitType == MenuExitType.MoveExecute)
            {
                //enforce this
                BattleControl.Instance.HideHPBars();
                BattleControl.Instance.HideEffectIcons();

                //now execute move :P
                caller.moveExecuting = true;
                caller.moveActive = true;
                caller.actionCounter++;
                StartCoroutine(caller.ExecuteMoveCoroutine(moveLevel));
                //BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Skill);
                yield return new WaitUntil(() => caller == null || !caller.moveActive);
                //yield return caller.currMove.Execute(caller);
            }
        }

        SetPlayerSprites();
    }

    public IEnumerator AutoMove(PlayerEntity caller)
    {
        //Note: automove may not be synonymous with berserk (only berserker forces targetting)
        BattleEntity berserkTarget = caller.GetBerserkTarget();
        if (berserkTarget != null)
        {
            //choose first move, try to hit target
            //if not possible, choose first possible target (or do nothing)

            //weapon moves get priority (because their targetting is worse than jump targetting)
            caller.currMove = caller.weaponMoves[0];
            bool result = caller.currMove.GetTargetArea(caller).GetCheckerResult(caller, berserkTarget);

            result &= !caller.BadgeEquipped(Badge.BadgeType.SoftPower); //soft power -> no weapon moves

            if (result)
            {
                caller.currMove.ChooseMove(caller);
                caller.curTarget = berserkTarget;
            }
            else
            {
                caller.currMove = caller.jumpMoves[0];
                bool result2 = caller.currMove.GetTargetArea(caller).GetCheckerResult(caller, berserkTarget);

                result2 &= !caller.BadgeEquipped(Badge.BadgeType.MetalPower); //metal power -> no jump moves

                if (result2)
                {
                    caller.currMove.ChooseMove(caller);
                    caller.curTarget = berserkTarget;
                }
                else
                {
                    //if not possible, choose first move, first possible target (or do nothing)

                    //1 = false, 2 = true, 3 = false...
                    bool moveType = (BattleControl.Instance.turnCount - 1) % 2 == 1;

                    if (moveType)
                    {
                        caller.currMove = caller.weaponMoves[0];
                        if (caller.BadgeEquipped(Badge.BadgeType.SoftPower))
                        {
                            caller.currMove = caller.jumpMoves[0];
                        }
                    }
                    else
                    {
                        caller.currMove = caller.jumpMoves[0];
                        if (caller.BadgeEquipped(Badge.BadgeType.MetalPower))
                        {
                            caller.currMove = caller.weaponMoves[0];
                        }
                    }

                    if (caller.BadgeEquipped(Badge.BadgeType.SoftPower) && caller.BadgeEquipped(Badge.BadgeType.MetalPower))
                    {
                        caller.currMove = null;
                    }

                    //List<BattleEntity> targetList = caller.currMove.GetTargetArea(caller).SelectFrom(caller, BattleControl.Instance.GetEntities());
                    List<BattleEntity> targetList = caller.currMove != null ? BattleControl.Instance.GetEntitiesSorted(caller, caller.currMove.GetTargetArea(caller)) : null;
                    if (targetList != null && targetList.Count > 0)
                    {
                        caller.currMove.ChooseMove(caller);
                        caller.curTarget = targetList[0];
                    }
                    else
                    {
                        //Do nothing
                        caller.currMove = null;
                        caller.curTarget = null;                                              
                    }
                }
            }
        }
        else
        {
            //if not possible, choose first move, first possible target (or do nothing)

            //1 = false, 2 = true, 3 = false...
            bool moveType = (BattleControl.Instance.turnCount - 1) % 2 == 1;

            if (moveType)
            {
                caller.currMove = caller.weaponMoves[0];
                if (caller.BadgeEquipped(Badge.BadgeType.SoftPower))
                {
                    caller.currMove = caller.jumpMoves[0];
                }
            }
            else
            {
                caller.currMove = caller.jumpMoves[0];
                if (caller.BadgeEquipped(Badge.BadgeType.MetalPower))
                {
                    caller.currMove = caller.weaponMoves[0];
                }
            }

            if (caller.BadgeEquipped(Badge.BadgeType.SoftPower) && caller.BadgeEquipped(Badge.BadgeType.MetalPower))
            {
                caller.currMove = null;
            }

            //List<BattleEntity> targetList = caller.currMove.GetTargetArea(caller).SelectFrom(caller, BattleControl.Instance.GetEntities());
            List<BattleEntity> targetList = caller.currMove != null ? BattleControl.Instance.GetEntitiesSorted(caller, caller.currMove.GetTargetArea(caller)) : null;
            if (targetList != null && targetList.Count > 0)
            {
                caller.currMove.ChooseMove(caller);
                caller.curTarget = targetList[0];
            }
            else
            {
                //try the other move?
                if (moveType)
                {
                    caller.currMove = caller.jumpMoves[0];
                    if (caller.BadgeEquipped(Badge.BadgeType.MetalPower))
                    {
                        caller.currMove = null;
                    }
                }
                else
                {
                    caller.currMove = caller.weaponMoves[0];
                    if (caller.BadgeEquipped(Badge.BadgeType.SoftPower))
                    {
                        caller.currMove = null;
                    }
                }
                if (caller.BadgeEquipped(Badge.BadgeType.SoftPower) && caller.BadgeEquipped(Badge.BadgeType.MetalPower))
                {
                    caller.currMove = null;
                }
                targetList = caller.currMove != null ? BattleControl.Instance.GetEntitiesSorted(caller, caller.currMove.GetTargetArea(caller)) : null;

                if (targetList != null && targetList.Count > 0)
                {
                    caller.currMove.ChooseMove(caller);
                    caller.curTarget = targetList[0];
                }
                else
                {
                    //Do nothing
                    caller.currMove = null;
                    caller.curTarget = null;
                }
            }
        }

        //now execute move (?)
        if (caller.currMove != null)
        {
            caller.actionCounter++;
            caller.moveExecuting = true;
            caller.moveActive = true;
            StartCoroutine(caller.ExecuteMoveCoroutine());
            yield return new WaitUntil(() => caller == null || !caller.moveActive);
            //BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Skill);
            //yield return caller.currMove.Execute(caller);
        }

        //to make things work
        mexitType = MenuExitType.MoveExecute;

        /*
        //Death check
        mover.DeathCheck();
        yield return new WaitUntil(() => (!mover.immediateInEvent));

        //Check for enemy special events
        yield return StartCoroutine(BattleControl.Instance.RunOutOfTurnEvents());
        */
    }

    //The normal way you exit the menu
    //Suspend isn't really used
    public void ExitMenu(MenuExitType t)
    {
        switch (t)
        {
            case MenuExitType.MoveExecute:
            case MenuExitType.ActionExecute:
            case MenuExitType.Null:
                mresult = menu.GetFullResult();
                menu.ActiveClear();
                break;
            case MenuExitType.Switch:
                break;
        }
        mexitType = t;
        inMenu = false;
        return;
    }

    //These are "filters"
    //(so you can disable stuff remotely)
    //  (This is mainly for tutorials where you are scripted to use certain moves)
    public bool CanChoose(Move pm, BattleEntity b)
    {
        return true;
    }
    public bool CanChoose(BattleAction pm, BattleEntity b)
    {
        return true;
    }
}
