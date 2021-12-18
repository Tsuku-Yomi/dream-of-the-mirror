using System.Collections;
using UnityEngine;

public class DeerEnemy : Enemy
{
    [Header("Deer: ")]
    //鹿最少攻击思考时间
    public float timeThinkMin = 4f;
    //鸟最大攻击思考时间
    public float timeThinkMax = 6f;

    public Health playerHealth;

    public float timeNextDecision = 0;
    //远程两种攻击方式
    private string[] remoteAttack = new string[]{
        "Magic", "Jump"
    };

    protected override void Start()
    {
        base.Start();
        playerHealth = GetComponent<Health>();
        anim.SetBool("Walk", true);
        anim.SetBool("Impact", false);
    }

    protected override void Update()
    {
        base.Update();
        if (enemyAttackConsciousness.heroDistance > enemyAttackConsciousness.attackConsciousnessRange)
        {
            return;
        }
        anim.SetBool("IdleToWalk", true);
        if (playerHealth.currentHealth <= playerHealth.maxHealth/2 && curAnimIs("Deer_Walk"))
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Impact", true);
        }

        //远程攻击判定
        if (Time.time > timeNextDecision)
        {
            anim.SetTrigger(remoteAttack[Random.Range(0, remoteAttack.Length)]);

            //进行这一次攻击，同时为下一次攻击做准备
            float thinkTime = Random.Range(timeThinkMin, timeThinkMax);
            timeNextDecision = Time.time + thinkTime;
        }
    }

    public void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.GetComponent<Health>() == null)
        {
            return;
        }
        if (other.tag == "Hero")
        {
            other.GetComponent<Health>().TakeDamage(closeDamage);
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Health>() != null)
        {
            if (Time.time > other.gameObject.GetComponent<Health>().nextInvincibleTime)
            {
                if (other.tag == "Hero")
                {
                    other.GetComponent<Health>().TakeDamage(closeDamage);
                }
            }   
        } 
    }
}
