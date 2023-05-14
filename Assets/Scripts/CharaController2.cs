using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class CharaController2 : MonoBehaviour
{
    private NavMeshAgent navMesh;

    private Vector3 clickPosition;

    private Camera mainCamera;
    private Vector3 currentPosition = Vector3.zero;

    private State currentState = State.search; //現在のステート
    private bool stateEnter = true;
    //private bool firstSerch = true;


    //前方に障害物があるかどうか
    private bool existsObstacle = false;

    //人間を探すかどうか
    private bool isSearchHuman = true;

    //範囲に人間がいるかどうか
    private bool existHuman = false;

    //一度人を見つけて追いかけているかどうか
    private bool beFinding = false;

    //serchが終わったかどうか
    //private bool isFinishedSerch = false;

    Coroutine coroutine = null;


    enum State
    {
        search,
        moving,
        approach,
        happy,
        goHome,

        doNothing,
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
        GameManager.instance.state.text = currentState.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        navMesh = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        existHuman = false;

        float randomXPos;
        float randomZPos;
        int PlusMinus;

        Debug.Log(navMesh.isStopped);

        switch (currentState)
        {
            case State.search:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.message.text = "キョロキョロ";
                    
                    //コルーチンを複数回さないために、既に動かしているコルーチンを止める
                    if (coroutine != null)  
                    {
                        StopCoroutine(coroutine);  
                    }
                    
                    // StartCoroutineの戻り値が停止させるコルーチン  
                    coroutine = StartCoroutine(Searching());  
                }
                break;
            
            case State.moving:
                if (stateEnter)
                {
                    navMesh.isStopped = false;
                    stateEnter = false;
                    
                    //追いかけていた人を見失ったら一回帰る
                    if (beFinding)
                    {
                        beFinding = false;
                        ChangeState(State.goHome);
                    }
                    
                    Debug.Log("トコトコトコトコ");
                    GameManager.instance.message.text = "トコトコトコトコ";
                    
                    randomXPos = Random.Range(0f, 17f);
                    randomZPos = Random.Range(0f, 6.5f);
                    //XとZそれぞれに2分の1の確率で-1を掛け算する。
                    PlusMinus = Random.Range(1, 3);
                    if (PlusMinus == 2)
                    {
                        randomXPos *= (-1);
                    }

                    PlusMinus = Random.Range(1, 3);
                    if (PlusMinus == 2)
                    {
                        randomZPos *= (-1);
                    }

                    //Debug.Log(randomXPos + "," + randomZPos);
                    navMesh.SetDestination(new Vector3(randomXPos, 0, randomZPos));
                }
                
                //目的地の近くまで着いたらSerchする
                if (navMesh.remainingDistance <= 0.01f && !navMesh.pathPending)
                {
                    Debug.Log("目的地にたどり着いた");

                    navMesh.isStopped = true;

                    ChangeState(State.search);
                }
                break;

            case State.approach:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("向かってます");

                    navMesh.isStopped = false;

                    GameManager.instance.message.text = "向かってます";
                    beFinding = true;
                    
                }

                navMesh.SetDestination(currentPosition);

                if (navMesh.remainingDistance <= 3f && !navMesh.pathPending)
                {
                    Debug.Log("人にたどり着いた");

                    ChangeState(State.happy);
                    navMesh.isStopped = true;
                    return;
                }
                //Debug.Log(navMesh.remainingDistance);

                break;

            case State.happy:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("嬉しい！");
                    GameManager.instance.message.text = "嬉しい!";
                    StartCoroutine("JoyTurn");
                }

                break;

            case State.goHome:
                if (stateEnter)
                {
                    stateEnter = false;
                    navMesh.isStopped = false;
                    Debug.Log("お家に帰る");
                    GameManager.instance.message.text = "お家に帰る";

                    navMesh.SetDestination(Vector3.zero);
                }

                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                {
                    Debug.Log("家に帰った");

                    navMesh.isStopped = true;

                    ChangeState(State.search);
                    return;
                }

                break;

            case State.doNothing:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("何もしません");
                    GameManager.instance.message.text = "何もしません";
                }

                break;
        }
    }


    // CollisionDetectorのonTriggerStayにセットし、衝突判定を受け取るメソッド
    public void OnDetectObject(Collider collider)
    {
        if (collider.gameObject.layer == 9)
        {
            Debug.Log("壁にぶつかった");
        }
    }

    // CollisionDetectorのonTriggerExitにセットし、衝突判定を受け取るメソッド
    public void OutDetectObject(Collider collider)
    {
        if (collider.gameObject.layer == 9)
        {
            Debug.Log("壁を避けた");
        }
    }

    public void OnDetectHuman(Collider collider)
    {
        if (isSearchHuman)
        {
            if (collider.gameObject.layer == 8)
            {
                existHuman = true;

                // 衝突位置を取得する
                Vector3 hitPos = collider.transform.position;
                //navMesh.SetDestination(hitPos);
                currentPosition = hitPos;

                //既に人間を追跡しているなら以下の処理を飛ばす
                if (currentState == State.approach || currentState == State.happy)
                {
                    return;
                }

                Debug.Log("人を見つけた");

                GameManager.instance.message.text = "人を見つけた";

                ChangeState(State.approach);
            }
        }
    }

    public void OutDetectHuman(Collider collider)
    {
        Debug.Log("deta!!!!!!!!!!!!!!!!!!!!!!!");
    }

    //キョロキョロと周りを見渡す
    IEnumerator Searching()
    {
        for (int turn = 0; turn < 45; turn++)
        {
            transform.Rotate(0, 1, 0);
            yield return new WaitForSeconds(0.01f);
        }

        for (int turn = 0; turn < 90; turn++)
        {
            transform.Rotate(0, -1, 0);
            yield return new WaitForSeconds(0.01f);
        }

        for (int turn = 0; turn < 45; turn++)
        {
            transform.Rotate(0, 1, 0);
            yield return new WaitForSeconds(0.01f);
        }

        if (currentState == State.search)
        {
            ChangeState(State.moving);
        }
        yield break;
    }

    //人の近くにたどり着くと回転して喜ぶ
    IEnumerator JoyTurn()
    {
        isSearchHuman = false;
        for (int turn = 0; turn < 135; turn++)
        {
            transform.Rotate(0, 3, 0);
            yield return new WaitForSeconds(0.01f);
        }

        isSearchHuman = true;
        ChangeState(State.search);
    }

    void OnDrawGizmos()
    {
        if (currentPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentPosition, 0.5f);
        }
    }
}