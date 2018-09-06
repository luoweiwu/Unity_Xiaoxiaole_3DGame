using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    /*甜品相关的成员变量*/
    #region
    //甜品的种类
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROE_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT   //标记类型
    }

    //甜品预制体的字典，我们可以通过甜品的种类来得到对应的甜品游戏物体
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;

    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetPrefab[] sweetPrefabs;

    #endregion

    //单例实例化
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    //大网格的行列数
    public int xColumn;
    public int yRow;

    //填充时间
    public float fillTime;

    public GameObject gridPrefab;

    public int playerScore;

    public Text playerScoreText;

    private float addScoreTime;
    private float currentScore;

    //甜品数组
    private GameSweet[,] sweets;

    //要交换的两个甜品对象
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    //有关游戏UI显示的内容
    public Text timeText;

    private float gameTime=60;

    //判断游戏是否失败
    private bool gameOver;

    public GameObject gameOverPanel;

    public Text finalScoreText;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start() {

        //字典的实例化
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();

        for(int i=0;i<sweetPrefabs.Length;i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type,sweetPrefabs[i].prefab);
            }
        }

       for(int x = 0; x < xColumn; x++)
        {
            for (int y=0;y<yRow;y++)
            {
                GameObject chocolate = Instantiate(gridPrefab,CorrectPosition(x,y),Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }

        sweets = new GameSweet[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateNewSweet(x, y, SweetsType.EMPTY);
            }
        }

        //在(4,4)这个坐标点生成障碍物
        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, SweetsType.BARRIER);
        Destroy(sweets[4, 3].gameObject);
        CreateNewSweet(4, 3, SweetsType.BARRIER);
        Destroy(sweets[1, 1].gameObject);
        CreateNewSweet(1, 1, SweetsType.BARRIER);
        Destroy(sweets[1, 1].gameObject);
        CreateNewSweet(1, 1, SweetsType.BARRIER);
        Destroy(sweets[7, 1].gameObject);
        CreateNewSweet(7, 1, SweetsType.BARRIER);
        Destroy(sweets[1, 6].gameObject);
        CreateNewSweet(1, 6, SweetsType.BARRIER);
        Destroy(sweets[7, 6].gameObject);
        CreateNewSweet(7, 6, SweetsType.BARRIER);

        StartCoroutine(AllFill());
    }
	
	// Update is called once per frame
	void Update () {

        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            //显示我们的失败面板
            //播放失败面板的动画
            gameOverPanel.SetActive(true);
            finalScoreText.text = playerScore.ToString();
            gameOver = true;
        }

        timeText.text = gameTime.ToString("0");
        if (addScoreTime<=0.05f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if (currentScore < playerScore)
            {
                currentScore++;
                playerScoreText.text = currentScore.ToString();
                addScoreTime = 0;
            }
        }
	}

    public Vector3 CorrectPosition(int x ,int y)
    {
        //实际需要实例化巧克力的X位置 = GameManager位置的X坐标-大网格长度的一半+行列对应的X坐标
        //　实际需要实例化巧克力的Y位置 = GameManager位置的Y坐标-大网格长度的一半+行列对应的Y坐标
        return new Vector3(transform.position.x-xColumn/2f+x,transform.position.y+yRow/2f-y);
    }

    //产生甜品的方法
    public GameSweet CreateNewSweet(int x,int y,SweetsType type)
    {
        GameObject newSweet =  Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x,y,this,type);

        return sweets[x, y];
    }

    //填充甜品的方法
    public IEnumerator AllFill()
    {
        bool needRefill = true;

        while(needRefill)
        {
            //完成上次填充动画
            yield return new WaitForSeconds(fillTime);
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }

            //清除所有我们意见匹配好的甜品
            needRefill = ClearAllMatchedSweet();
        }

    }

    //分布填充
    public bool Fill()
    {
        bool FilledNotFinshed = false;  //用来判断本次是否完成

        //行遍历
        for(int y=yRow-2;y>=0;y--)
        {
            for(int x=0;x<xColumn;x++)
            {
                GameSweet sweet = sweets[x, y]; //得到当前元素位置

                //如果无法移动，则无法往下填充
                if (sweet.CanMove())
                {
                    GameSweet sweetBelow = sweets[x, y + 1]; 

                    if(sweetBelow.Type == SweetsType.EMPTY)//垂直填充
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponet.Move(x,y+1,fillTime);
                        sweets[x, y + 1] = sweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        FilledNotFinshed = true;
                    }
                    else
                    {
                        //-1代表左，1代表右
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;
                                //排除边界的时候
                                //左下方
                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;    //用来判断垂直填充是否可以满足填充要求
                                        for (int aboutY = y; aboutY >= 0; aboutY--)
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboutY];
                                            if (sweetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponet.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            FilledNotFinshed = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
               
            }

        }
        //最上排的特殊情况
        for (int x = 0; x < xColumn; x++)
           {
               GameSweet sweet = sweets[x, 0];

              if(sweet.Type == SweetsType.EMPTY)
              {
                    GameObject newSweet = Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x,-1), Quaternion.identity);
                    newSweet.transform.parent = transform;

                    sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                    sweets[x, 0].Init(x, -1,this,SweetsType.NORMAL);
                    sweets[x, 0].MovedComponet.Move(x, 0,fillTime);
                    sweets[x, 0].ColorComponet.SetColor((ColorSweet.ColorType)Random.Range(0,sweets[x,0].ColorComponet.NumColors));
                FilledNotFinshed = true;      
              }
        }
        return FilledNotFinshed;
    }

    //甜品是否相邻的判断方法
    private bool IsFriend(GameSweet sweet1,GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y-sweet2.Y)==1)||(sweet1.Y==sweet2.Y&&Mathf.Abs(sweet1.X-sweet2.X)==1);
    }

    //交换两个甜品
    private void ExchangeSweets(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null || MatchSweets(sweet2, sweet1.X, sweet1.Y) != null)
            {
                int tempX = sweet1.X;
                int tempY = sweet1.Y;


                sweet1.MovedComponet.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponet.Move(tempX, tempY, fillTime);

                ClearAllMatchedSweet();
                //消除甜品时两侧甜品进行填充
                StartCoroutine(AllFill());

            }
            else 
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }

        }
    }

    /*玩家对甜品操作进行处理的方法*/
    #region
    public void PressSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        pressedSweet = sweet;
    }

    public void EnterSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        enteredSweet = sweet;
    }

    public void ReleaseSweet()
    {
        if (gameOver)
        {
            return;
        }
        if (IsFriend(pressedSweet, enteredSweet))
        {
            ExchangeSweets(pressedSweet, enteredSweet);
        }
    }
    #endregion

    /*清除匹配的方法*/
    #region
    //匹配方法
    public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            ColorSweet.ColorType color = sweet.ColorComponet.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            //行匹配
            matchRowSweets.Add(sweet);

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xColumn)
                    {
                        break;
                    }

                    if (sweets[x, newY].CanColor() && sweets[x, newY].ColorComponet.Color == color)
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColorComponet.Color == color)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchLineSweets.Count < 2)
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }

            matchRowSweets.Clear();
            matchLineSweets.Clear();

            matchLineSweets.Add(sweet);

            //列匹配

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow)
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColorComponet.Color == color)
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newY - xDistance;
                            }
                            else
                            {
                                x = newY + xDistance;
                            }
                            if (x < 0 || x >= xColumn)
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColorComponet.Color == color)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }
        }

        return null;
    }


    //清除方法
    public bool ClearSweet(int x,int y)
    {
        if (sweets[x, y].CanClear()&&!sweets[x,y].ClearedCompent.IsClearing)
        {
            sweets[x, y].ClearedCompent.Clear();
            CreateNewSweet(x,y,SweetsType.EMPTY);


            ClearBarrier(x,y);
            return true;
        }
        return false;
    }

    //清除饼干的方法
    //坐标是被清除掉的甜品对象的坐标
    private void ClearBarrier(int x,int y)
    {
        //左右清除
        for(int fiendX = x-1;fiendX <= x + 1; fiendX++)
        {
            if (fiendX!=x && fiendX>0 && fiendX<xColumn)
            {
                if (sweets[fiendX, y].Type == SweetsType.BARRIER && sweets[fiendX, y].CanClear())
                {
                    sweets[fiendX, y].ClearedCompent.Clear();
                    CreateNewSweet(fiendX,y,SweetsType.EMPTY);
                }
            }
        }

        //上下清除
        for (int fiendY = y - 1; fiendY <= y + 1; fiendY++)
        {
            if (fiendY != y && fiendY >= 0 && fiendY < yRow)
            {
                if (sweets[x, fiendY].Type == SweetsType.BARRIER && sweets[x, fiendY].CanClear())
                {
                    sweets[x, fiendY].ClearedCompent.Clear();
                    CreateNewSweet(x,fiendY, SweetsType.EMPTY);
                }
            }
        }

    }

    //清除全部完成匹配的甜品
    private bool ClearAllMatchedSweet()
    {
        bool needRefill = false;

        for(int y = 0; y < yRow; y++)
        {
            for (int x=0;x<xColumn;x++)
            {
                if (sweets[x, y].CanClear())
                {
                     List<GameSweet> matchList = MatchSweets(sweets[x,y],x,y);

                    if (matchList != null)
                    {
                        for (int i=0;i<matchList.Count;i++)
                        {
                            if (ClearSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }
                    }
                }
            }
        }
        return needRefill;
    }
    #endregion

    //
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }

}
