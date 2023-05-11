using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Human : MonoBehaviour
{
    public List<FoodData> foodDatabase = new List<FoodData>();
    
    private NavMeshAgent navMesh;

    enum State
    {
        goToBed,
        sleep,
        goToToilet,
        doTilet,
        goToWashHand,
        washHand,
        doNothing,
        goToKitchen,
        eat,
        starveToDeath,
        
    }

    enum DesireType
    {
        sleep,
        eat,
        toilet,
        
    }

    class Desire
    {
        public DesireType type { get; private set; }
        public float value;

        public Desire(DesireType _type)
        {
            type = _type;
            value = 0f;
        }
    }

    class Desires
    {
        public List<Desire> desireList { get; private set; } = new List<Desire>();

        public Desire GetDesire(DesireType type)
        {
            foreach (Desire desire in desireList)
            {
                if (desire.type == type)
                {
                    return desire;
                }
            }
            return null;
        }

        public void SortDesire()
        {
            desireList.Sort((desire1,desire2)=>desire2.value.CompareTo(desire1.value));
        }
        //コンストラクタ
        public Desires()
        {
            int desireNum = System.Enum.GetNames(typeof(DesireType)).Length;

            for (int i = 0; i < desireNum; i++)
            {
                DesireType type = (DesireType)System.Enum.ToObject(typeof(DesireType), i);
                Desire newDesire = new Desire(type);
                
                desireList.Add(newDesire);
            }
        }
    }

    private Desires desires = new Desires();

    private float sleepDesireUpSpeed = 15f;//睡眠欲がMaxになるまでの時間
    private float sleepDesireDownSpeed = 5f;//睡眠欲がゼロになるまでの時間

    private float toiletDesireUpSpeed = 10f;//便意がMaxになるまでの時間
    private float toiletTime = 2f;//トイレにかかる時間

    private float handBacteria = 0;//手の汚れ
    private float handWashSpeed = 2f;//手を洗うスピード

    private float hungrySpeed = 9;//空腹度がMaxになるまでの時間
    private float eatSpeed = 3;//食べるのにかかる時間

    private State currentState = State.doNothing;//現在のステート
    private bool stateEnter = true;
    
    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
    }

    private void Start()
    {
        navMesh = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        //睡眠欲の上昇
        if (currentState != State.sleep)
        {
            desires.GetDesire(DesireType.sleep).value += Time.deltaTime / sleepDesireUpSpeed;
        }
        
        //便意の上昇
        if (currentState != State.doTilet)
        {
            desires.GetDesire(DesireType.toilet).value += Time.deltaTime / toiletDesireUpSpeed;
        }
        
        //食欲の上昇
        if (currentState != State.eat)
        {
            desires.GetDesire(DesireType.eat).value += Time.deltaTime / hungrySpeed;
        }

        //食欲パラメータが3を上回ったら死ぬ
        if (currentState != State.starveToDeath && desires.GetDesire(DesireType.eat).value >= 2.0f)
        {
            ChangeState(State.starveToDeath);
            return;
        }
        
        switch (currentState)
        {
            case State.starveToDeath :
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("腹が減って死んだぞ");
                }
                break;
            
            case State.doNothing:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "暇だなー。やる事ないなー";
                    Debug.Log(GM.instance.livingRoomPos.position);
                    navMesh.SetDestination(GM.instance.livingRoomPos.position);
                }

                ChoiceAction();
                break;
            
            case State.goToKitchen:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "冷蔵庫に何かあるかな";
                    navMesh.SetDestination(GM.instance.kitchenPos.position);

                }

                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                {
                    ChangeState(State.eat);
                    return;
                }
                
                break;
            
            case State.eat:
                if (stateEnter)
                {
                    stateEnter = false;

                    int randomIndex = Random.Range(0, foodDatabase.Count);
                    FoodData data = foodDatabase[randomIndex];
                    GM.instance.text.text =((data.dring ? "喉が乾いたから" : "腹減ったから")+data.name+(data.dring ? "を飲もう！" : "を食おう！")+data.eatSound);
                    desires.GetDesire(DesireType.eat).value = 1;
                }

                desires.GetDesire(DesireType.eat).value -= Time.deltaTime / eatSpeed;

                if (desires.GetDesire(DesireType.eat).value <= 0)
                {
                    if (!ChoiceAction())
                    {
                        ChangeState(State.doNothing);
                    }
                }

                break;
            
            case State.goToToilet:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "トイレに行こう！";
                    navMesh.SetDestination(GM.instance.toiletPos.position);
                }
                
                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                {
                    ChangeState(State.doTilet);
                    return;
                }
                break;
            
            case State.doTilet:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "ジャアアアアア！";
                    handBacteria += 1;
                    desires.GetDesire(DesireType.toilet).value = 1;
                }

                desires.GetDesire(DesireType.toilet).value -= Time.deltaTime / toiletTime;

                if (desires.GetDesire(DesireType.toilet).value <= 0)
                {
                    desires.SortDesire();
                    ChangeState(State.goToWashHand);
                }

                break;
            
            case State.goToWashHand:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "スッキリした！手を洗おう！";
                    navMesh.SetDestination(GM.instance.washHandPos.position);
                }
                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                {
                    ChangeState(State.washHand);
                    return;
                }
                break;
            
            case State.washHand:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "ジャブジャブ";
                }

                handBacteria -= Time.deltaTime / handWashSpeed;

                if (handBacteria <= 0)
                {
                    if (!ChoiceAction())
                    {
                        ChangeState(State.doNothing);
                    }
                }
                break;
            
            case State.goToBed:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "寝るか";
                    navMesh.SetDestination(GM.instance.bedPos.position);
                }
                
                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                {
                    ChangeState(State.sleep);
                    return;
                }
                break;
            
            case State.sleep:
                if (stateEnter)
                {
                    stateEnter = false;
                    GM.instance.text.text = "ZZZZZZZZZ";
                    desires.GetDesire(DesireType.sleep).value = 1;
                }

                
                desires.GetDesire(DesireType.sleep).value -= Time.deltaTime / sleepDesireDownSpeed;
                
                if (desires.GetDesire(DesireType.sleep).value <= 0)
                {
                    if (!ChoiceAction())
                    {
                        ChangeState(State.doNothing);
                    }
                }
                break;
        }
    }

    bool ChoiceAction()
    {
        desires.SortDesire();
        if (desires.desireList[0].value >= 1)
        {
            Desire desire = desires.desireList[0];
            switch (desire.type)
            {
                case DesireType.eat:
                    ChangeState(State.goToKitchen);
                    return true;
                case DesireType.sleep:
                    ChangeState(State.goToBed);
                    return true;
                case DesireType.toilet:
                    ChangeState(State.goToToilet);
                    return true;
            }
        }

        return false;
    }
}
