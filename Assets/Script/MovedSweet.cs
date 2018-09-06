using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovedSweet : MonoBehaviour {

    private GameSweet sweet;

    private IEnumerator moveCoroutine;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    //开启或者结束一个协成
    public void Move(int newX,int newY,float time)
    {
        if(moveCoroutine!=null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX,newY,time);
        StartCoroutine(moveCoroutine);
    }

    //负责移动的协成
    private IEnumerator MoveCoroutine(int newX,int newY,float time)
    {
        sweet.X = newX;
        sweet.Y = newY;

        //每一帧移动一点点
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPosition(newX,newY);

        for(float t=0;t<time;t+=Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos,endPos,t/time);
            yield return 0;
        }

        sweet.transform.position = endPos;
    }
}
