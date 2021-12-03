using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [Header("需在项目面板填充的字段：")]
    public Sprite lightHero; 
    public Sprite darkHero; //暂时两个，后期多了可以换成sprite数组

    public float speed = 5;
    public int dirHeld = -1; //方向移动键是否从键盘上按下
    public int facing = 1; //面向方向

    private SpriteRenderer sRend;
    private Rigidbody rigid;
    private Animator anim;

    private Vector3[] directions = new Vector3[]{
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 vel = Vector3.zero;
        if(dirHeld == -1)
        {
            anim.CrossFade("Dray_Walk_"+facing, 0);
            anim.speed = 0;
        }
        else
        {
            vel = directions[dirHeld];
            anim.CrossFade("Dray_Walk_"+facing, 0);
            anim.speed = 1;
        }
        
        rigid.velocity = vel * speed;
    }

    void LateUpdate() 
    {
        //重置主角移动
        dirHeld = -1;

        //获取游戏对象的半网格位置
        Vector2 rPos = GetRoomPosOnGrid(0.5f);
    }

    void OnCollisionEnter(Collision coll) 
    {
        if(coll.gameObject.tag == "End")
        {
            SceneController.instance.Hero1End = true;
        }
    }

    public int GetFacing()
    {
        return facing;
    }

    public float gridMult
    {
        get{return inRoom.gridMult;}
    }
}