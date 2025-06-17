using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handles menu stuff
public interface IMenuHandler
{
    void PushState(IMenuHandler menu); //open a submenu
    void PopState(); //close a submenu (Ends up here in this menu)
    void PopSelf(); //same as popstate but called in the submenu
    void Init(); //start your own menu
    void Clear(); //destroy your own menu (wait for submenu stuff?)
    void Destroy();
    IMenuHandler GetActiveMenu();
    void SetParent(IMenuHandler menuHandler);
    void Suspend();
    void UnSuspend();
    event System.EventHandler<MenuExitEventArgs> menuExit;
    IMenuHandler GetParent();
    IMenuHandler GetSubMenu();
    MenuResult GetFullResult();
    MenuResult GetResult();
}

public class MenuExitEventArgs : System.EventArgs
{
    public readonly MenuResult mr;

    public MenuExitEventArgs(MenuResult p_mr)
    {
        mr = p_mr;
    }
}

public class MenuResult
{
    public object output;
    public MenuResult subresult;

    public MenuResult(object p_output)
    {
        output = p_output;
        subresult = null;
    }
    public MenuResult(object p_output, MenuResult p_subresult)
    {
        output = p_output;
        subresult = p_subresult;
    }

    public override string ToString()
    {
        string o = output.ToString();
        MenuResult current = this;
        while (current.subresult != null)
        {
            current = current.subresult;
            o += "; " + current.output.ToString();
        }

        return o;
    }
}

public abstract class MenuHandler : MonoBehaviour, IMenuHandler
{
    protected bool active;
    protected bool suspended;
    protected IMenuHandler submenu;
    protected IMenuHandler parent;

    public float lifetime = 0;
    public const float MIN_SELECT_TIME = 0.125f;

    public abstract event EventHandler<MenuExitEventArgs> menuExit;

    public void SetParent(IMenuHandler menuHandler)
    {
        parent = menuHandler;
    }
    //open a submenu
    public virtual void PushState(IMenuHandler menu)
    {
        submenu = menu;
        submenu.SetParent(this);
        //submenu.Init();
        //It is intended that you call Init as you create the menu
        Clear();
    }
    //close a submenu
    public virtual void PopState()
    {
        submenu.Clear();
        submenu.Destroy();
        submenu = null;
        Reinit();
    }
    public void PopSelf()
    {
        if (parent == null)
        {
            return;
        }
        parent.PopState();
    }
    public void CloseActiveMenu()
    {
        GetActiveMenu().PopSelf();
    }
    public virtual void Reinit()
    {
        Init();
    }
    public virtual void Init()
    {
        active = true;
        lifetime = 0;
    }
    public virtual void Clear()
    {
        active = false;
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }

    public bool GetActive()
    {
        return active;
    }
    public IMenuHandler GetActiveMenu()
    {
        if (active)
        {
            return this;
        }
        else
        {
            //a submenu is active (?)
            if (submenu != null)
            {
                return submenu.GetActiveMenu();
            }
            else
            {
                //fail
                return null;
            }
        }
    }
    public void ActiveClear()
    {
        if (GetActiveMenu() != null)
        {
            GetActiveMenu().Clear();
        }
    }
    public void Suspend()
    {
        suspended = true;
    }
    public void UnSuspend()
    {
        suspended = false;
    }
    //the parent menu at the very top (Will cause stack overflow if you have a recursive loop of menus)
    public IMenuHandler FindAncestor()
    {
        IMenuHandler output = this;
        while (output.GetParent() != null)
        {
            output = output.GetParent();
        }
        return output;
    }
    public IMenuHandler GetParent()
    {
        return parent;
    }
    public IMenuHandler GetSubMenu()
    {
        return submenu;
    }
    public MenuResult GetFullResult()
    {
        if (GetActiveMenu() == null)
        {
            return null;
        }

        MenuResult output = GetActiveMenu().GetResult();
        IMenuHandler tempMenu = GetActiveMenu();
        MenuResult tempResult = output;

        while (tempMenu.GetParent() != null)
        {
            tempResult = output;
            tempMenu = tempMenu.GetParent();
            output = tempMenu.GetResult();
            output.subresult = tempResult;
        }

        return output;
    }
    public virtual MenuResult GetResult()
    {
        return new MenuResult(null,null);
    }
}