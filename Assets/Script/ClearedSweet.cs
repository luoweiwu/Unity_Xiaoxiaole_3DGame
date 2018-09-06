using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearedSweet : MonoBehaviour {

    //清除脚本的动画
    public AnimationClip clearAnimation;

    private bool isClearing;

    public AudioClip destoryAudio;

    public bool IsClearing
    {
        get
        {
            return isClearing;
        }
    }

    //为了方便后期做拓展，设置成protected
    protected GameSweet sweet;

    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator!=null)
        {
            animator.Play(clearAnimation.name);
            //玩家得分+1，播放清除声音

            GameManager.Instance.playerScore++;
            AudioSource.PlayClipAtPoint(destoryAudio,transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }

}
