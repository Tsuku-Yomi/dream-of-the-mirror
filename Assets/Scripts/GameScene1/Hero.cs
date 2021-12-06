using System.Collections;
using UnityEngine;

// 玩家移动方向枚举（用于桌面移动）
public enum Directions 
{
    idle = -1,
    Right,
    Up,
    Left,
    Down
}

public enum Facing
{
    Right = 0,
    Up,
    Left,
    Down
}
public class Hero : MonoBehaviour
{
    //玩家移动方向（包括不移动）
    public Directions heroDirection = Directions.idle; 
    //玩家面向方向
    public Facing heroFacing = Facing.Right;
    //是第一张地图还是第二张
    public int mapNum; 
    //判断是否到达终点
    public bool HeroEnd = false;  
    //终点坐标
    public Vector2 mapFinish; 

    [Header("Movement Attribute")]
    //移动速度
    public float speed = 2f; 

    [Header("Reference")]
    //CharacterController组件
    CharacterController controller; 

    [Header("Body parts reference")]
    //虚拟轴游戏对象
    public GameObject input_direction; 

    [Header("Platform")]
    //是否使用游戏手柄
    public bool PC = false; 

    //动画组件
    private Animator anim; 

    private Vector3[] directions = new Vector3[]{
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    //用于电脑端
    private KeyCode[] keys = new KeyCode[]{
        KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow
    };

    public void SetHero(int eX, int eY)
    {
        transform.localPosition = new Vector3(eX, eY, 0);
    }

    //获取组件同时找到位于MobileJoyStickCanvas的虚拟轴游戏对象
    void Awake() 
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        input_direction = GameObject.Find("DirectionJoyStick");
    }

    //将该游戏对象赋给SceneController脚本同时启用ReachTheEnd()协程
    void Start()  
    {
        if(SceneController.instance.hero1 != null)
            SceneController.instance.hero1 = this;
        else
            SceneController.instance.hero2 = this;

        StartCoroutine(ReachTheEnd());
    }

    void FixedUpdate()
    {
        GridMove();
    }

    void Update() 
    {
        #region 
        //虚拟轴移动控制
        JoyStickMove();
        #endregion

        #region
        //桌面端输入控制
        //处理键盘输入和主角状态
        // heroDirection = (Directions)(-1); //这句不需要，因为前面虚拟轴移动控制以及使用过了
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(keys[i]))
                heroDirection = (Directions)i;
        }

        //填充Facing
        if(heroDirection != (Directions)(-1))
        {
            heroFacing = (Facing)heroDirection;
        }

        Vector3 vel = Vector3.zero;
        if(heroDirection == (Directions)(-1))
        {
            anim.CrossFade("Hero_Walk_"+(int)heroFacing, 0);
            anim.speed = 0;
        }
        else
        {
            vel = directions[(int)(heroDirection)];
            anim.CrossFade("Hero_Walk_"+(int)heroFacing, 0);
            anim.speed = 1;
        }
        
        controller.Move(vel * speed * Time.deltaTime);
        #endregion
    }    

    void JoyStickMove()
    {
        if(!PC)
        {
            heroDirection = (Directions)(-1);
            heroDirection = JudgeDirection(input_direction.GetComponent<MobileInputController>().Horizontal, input_direction.GetComponent<MobileInputController>().Vertical);

            //填充Facing
            if(heroDirection != (Directions)(-1))
            {
                heroFacing = (Facing)heroDirection;
            }

            if(heroDirection == (Directions)(-1))
            {
                anim.CrossFade("Hero_Walk_"+(int)heroFacing, 0);
                anim.speed = 0;
            }
            else
            {
                anim.CrossFade("Hero_Walk_"+(int)heroFacing, 0);
                anim.speed = 1;
            }

            controller.Move(Vector3.up * input_direction.GetComponent<MobileInputController>().Vertical * Time.deltaTime * speed+ Vector3.right * input_direction.GetComponent<MobileInputController>().Horizontal * Time.deltaTime * speed);
        }
        else
        {
            heroDirection = (Directions)(-1);
            heroDirection = JudgeDirection(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            //填充Facing
            if(heroDirection != (Directions)(-1))
            {
                heroFacing = (Facing)heroDirection;
            }

            if(heroDirection == (Directions)(-1))
            {
                anim.CrossFade("Hero_Walk_"+(int)heroFacing, 0);
                anim.speed = 0;
            }
            else
            {
                anim.CrossFade("Dray_Walk_"+(int)heroFacing, 0);
                anim.speed = 1;
            }

            controller.Move(Vector3.up * Input.GetAxisRaw("Vertical") * Time.deltaTime * speed + Vector3.right * Input.GetAxisRaw("Horizontal") * Time.deltaTime * speed); 
        }
    }

    public Directions JudgeDirection(float x, float y)
    {
        if(Mathf.Abs(x)<=0.05f && Mathf.Abs(y)<=0.05f)
            return Directions.idle;
        if(x>=0 && y>=0)
        {
            if(x>=y)
                return Directions.Right;
            else
                return Directions.Up;
        }
        if(x>=0 && y<0)
        {
            if(x>=Mathf.Abs(y))
                return Directions.Right;
            else
                return Directions.Down;
        }
        if(x<0 && y>=0)
        {
            if(Mathf.Abs(x)>=y)
                return Directions.Left;
            else
                return Directions.Up;
        }
        if(x<0 && y<0)
        {
            if(x<=y)
                return Directions.Left;
            else
                return Directions.Down;
        }
        return Directions.idle;
    }

    public Vector2 GetPosOnGrid(float mult = 0.5f)
    {
        Vector2 rPos = transform.localPosition;
        rPos /= mult;
        rPos.x = Mathf.Round(rPos.x);
        rPos.y = Mathf.Round(rPos.y);
        rPos *= mult; //将一个小数先乘2,再进行就近取整,再除以2和直接将这个小数就近取整会使结果会更接近原数（0.5的倍数）
        return rPos;
    }

    //让Hero在移动时可以逐渐移动到格子里面
    void GridMove() 
    {
        if(heroDirection == (Directions)(-1)) return;
        //如果在一个方向移动，分配到网格
        //首先，获取网格位置
        Vector2 rPosGrid = GetPosOnGrid(0.5f);
        
        //移动到网格行
        float delta = 0;
        if(heroDirection == (Directions)0 || heroDirection == (Directions)2)
        {
            //水平移动，分配到y网格
            delta = rPosGrid.y - transform.localPosition.y;
        }
        else
        {
            //垂直移动，分配到x网格
            delta = rPosGrid.x - transform.localPosition.x;
        }
        if(delta == 0) return;

        float move = speed * Time.fixedDeltaTime;
        move = Mathf.Min(move, Mathf.Abs(delta));
        if(delta < 0) move = -move;

        Vector3 tLocalPosition = transform.localPosition;
        if(heroDirection == (Directions)0 || heroDirection == (Directions)2)
        {

            tLocalPosition.y += move;
            transform.localPosition = tLocalPosition;
        }
        else
        {
            tLocalPosition.x += move;
            transform.localPosition = tLocalPosition;
        }
    }

    //判断该游戏对象是否达到中间（使用协程的目的是减少判定次数）
    IEnumerator ReachTheEnd() 
    {
        while(true)
        {
            if(mapNum == 1)
            {
                if(Mathf.Abs(SceneController.instance.mapFinish1.x-transform.localPosition.x)<=0.25f && Mathf.Abs(SceneController.instance.mapFinish1.y-transform.localPosition.y)<=0.25f)
                {
                    HeroEnd = true;
                }   
                else
                    HeroEnd = false;
            }
            else
            {
                if(Mathf.Abs(SceneController.instance.mapFinish2.x-transform.localPosition.x)<=0.25f && Mathf.Abs(SceneController.instance.mapFinish2.y-transform.localPosition.y)<=0.25f)
                {
                    HeroEnd = true;
                } 
                else
                    HeroEnd = false;
            }
            
            yield return new WaitForSeconds(0.01f);
        }
    }
}