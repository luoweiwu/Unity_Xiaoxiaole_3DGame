using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {

    private int x;
    private int y;
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            if (CanMove())
            {
                x = value;
            }
        }
    }
    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (CanMove())
            {
                y = value;
            }
        }
    }

    private GameManager.SweetsType type;
    public GameManager.SweetsType Type
    {
        get
        {
            return type;
        }
    }


    [HideInInspector]
    public GameManager gameManager;

    public MovedSweet MovedComponet
    {
        get
        {
            return movedComponet;
        }
    }
    private MovedSweet movedComponet;


    public ColorSweet ColorComponet
    {
        get
        {
            return coloredCompent;
        }
    }

    public ClearedSweet ClearedCompent
    {
        get
        {
            return clearedCompent;
        }
    }

    private ColorSweet coloredCompent;

    private ClearedSweet clearedCompent;


    //判断甜品是否可以移动
    public bool CanMove()
    {
        return movedComponet != null;
    }

    //判断是否可以着色
    public bool CanColor()
    {
        return coloredCompent != null;
    }

    //判断是否可以清除
    public bool CanClear()
    {
        return clearedCompent != null;
    }

    private void Awake()
    {
        movedComponet = GetComponent<MovedSweet>();
        coloredCompent = GetComponent<ColorSweet>();
        clearedCompent = GetComponent<ClearedSweet>();
    }

    public void Init(int _x,int _y,GameManager _gameManager,GameManager.SweetsType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    //鼠标点击
    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }

    //鼠标按下
    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }

    //鼠标抬起
    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
