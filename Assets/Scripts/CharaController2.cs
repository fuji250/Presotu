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
    
    private Vector3 currentPosition = Vector3.zero;
 
    private State currentState = State.search; //現在のステート
    private bool stateEnter = true;

    public int serchNum = default;
    public int approachDistance = default;


    //範囲に人間がいるかどうか
    private bool existHuman = false;

    //一度人を見つけて追いかけているかどうか
    private bool beFinding = false;
    
    //人間と離れたかどうか
    private bool isFar = false;

    private bool lastOdo = false;


    Coroutine serchCoroutine = null;
    Coroutine joyCoroutine = null;
    Coroutine lostSerchCoroutine = null;
    Coroutine odoodoCoroutine = null;


    public float approachSpeed;
    public float goHomeSpeed;

    public GameObject mesh;

    public bool coward;
    public bool pullWalk;
    
    int count = 0;

    
    enum State
    {
        search,
        moving,
        approach,
        happy,
        lost,
        goHome,
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
        GameManager.instance.state.text = currentState.ToString();
        Debug.Log(currentState);
        
        //キョロキョロ止める
        if (serchCoroutine != null)  
        {
            StopCoroutine(serchCoroutine);
            serchCoroutine = null;
        }
        if (lostSerchCoroutine != null)  
        {
            StopCoroutine(lostSerchCoroutine);
            lostSerchCoroutine = null;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        navMesh = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float randomXPos;
        float randomZPos;
        int PlusMinus;


        switch (currentState)
        {
            case State.search:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.message.text = "キョロキョロ";
                    
                    //コルーチンを複数回さないために、既に動かしているコルーチンを止める
                    if (serchCoroutine != null)  
                    {
                        StopCoroutine(serchCoroutine);  
                    }
                    
                    Debug.Log(serchNum);
                    // StartCoroutineの戻り値が停止させるコルーチン  
                    serchCoroutine = StartCoroutine(Searching(serchNum));

                    serchNum = 1;
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

                    navMesh.SetDestination(new Vector3(randomXPos, 0, randomZPos));
                }
                
                //目的地の近くまで着いたらSerchする
                if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
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
                    navMesh.speed = approachSpeed;

                    GameManager.instance.message.text = "向かってます";
                    beFinding = true;
                    
                    
                    //コルーチンを複数回さないために、既に動かしているコルーチンを止める
                    if (odoodoCoroutine != null)  
                    {
                        StopCoroutine(odoodoCoroutine);  
                    }
                    if (coward)
                    {
                        odoodoCoroutine = StartCoroutine(OdoOdo());
                    }
                    
                    count = 0;
                }

                if (pullWalk)
                {
                    if (count % 25 == 0)
                    {
                        //ここに処理
                        navMesh.isStopped = true;
                    }
                    if (count % 50 == 0)
                    {
                        navMesh.isStopped = false;
                        count = 0;

                    }
                    count++; // カウントアップ
                }
                
                


                Approach();

                break;

            case State.happy:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("嬉しい！");
                    GameManager.instance.message.text = "嬉しい!";

                    serchNum = 3;
                    
                    lastOdo = true;

                }
                //コルーチンを複数回さないために、既に動かしているコルーチンを止める
                if (joyCoroutine == null)  
                {
                    // StartCoroutineの戻り値が停止させるコルーチン  
                    joyCoroutine = StartCoroutine(JoyDance());  
                }

                Approach();

                break;

            case State.lost:
                if (stateEnter)
                {
                    navMesh.isStopped = true;

                    stateEnter = false;
                    Debug.Log("見失った！");
                    GameManager.instance.message.text = "見失った!";
                    
                    //コルーチンを複数回さないために、既に動かしているコルーチンを止める
                    if (lostSerchCoroutine == null)  
                    {
                        // StartCoroutineの戻り値が停止させるコルーチン  
                        lostSerchCoroutine = StartCoroutine(LostSearching(2));  
                    }
                }

                break;
            
            case State.goHome:
                if (stateEnter)
                {
                    stateEnter = false;
                    navMesh.isStopped = false;
                    Debug.Log("お家に帰る");
                    GameManager.instance.message.text = "お家に帰る";

                    navMesh.speed = goHomeSpeed;
                    navMesh.SetDestination(Vector3.zero);
                }

                if (navMesh.remainingDistance <= 7f && !navMesh.pathPending)
                {
                    Debug.Log("家に帰った");

                    navMesh.isStopped = true;

                    ChangeState(State.search);
                    return;
                }

                break;
        }
        existHuman = false;
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

    public void OutDetectHuman(Collider collider)
    {
        Debug.Log("deta!!!!!!!!!!!!!!!!!!!!!!!");
    }

    //キョロキョロと周りを見渡す
    IEnumerator Searching(int x)
    {
        for (int i = 0; i < x; i++)
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
        }
        
        if (currentState == State.search)
        {
            ChangeState(State.moving);
        }
        yield break;
    }
    
    IEnumerator LostSearching(int x)
    {
        int randomX = Random.Range(1,4);
        float waitNum = Random.Range(0.3f, 1f);
        
        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < x; i++)
        {
            for (int turn = 0; turn < 45/randomX; turn++)
            {
                transform.Rotate(0, randomX, 0);
                yield return new WaitForSeconds(0.01f);
            }
            
            yield return new WaitForSeconds(waitNum);

            for (int turn = 0; turn < 90/randomX; turn++)
            {
                transform.Rotate(0, -randomX, 0);
                yield return new WaitForSeconds(0.01f);
            }
            
            yield return new WaitForSeconds(waitNum);


            for (int turn = 0; turn < 45/randomX; turn++)   
            {
                transform.Rotate(0, randomX, 0);
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(waitNum);

        }
        ChangeState(State.goHome);
        
        yield break;
    }

    //人の近くにたどり着くと回転して喜ぶ
    IEnumerator JoyDance()
    {
        for (int turn = 0; turn < 22.5f; turn++)
        {
            mesh.transform.Rotate(0, 2, 0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 45; turn++)
        {
            mesh.transform.Rotate(0, -2, 0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 45; turn++)
        {
            mesh.transform.Rotate(0, 2, 0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 22.5f; turn++)
        {
            mesh.transform.Rotate(0, -2, 0);
            yield return new WaitForSeconds(0.01f);
        }

        joyCoroutine = null;
        //人がいなくなったらApproachに移行
        if (!existHuman)
        {
            ChangeState(State.lost);
        }
    }
    
    IEnumerator OdoOdo()
    {
        for (int turn = 0; turn < 10f; turn++)
        {
            mesh.transform.Rotate(0, 4, 0);
            mesh.transform.Translate (0.001f, 0, 0);

            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 20; turn++)
        {
            mesh.transform.Rotate(0, -4, 0);
            mesh.transform.Translate (-0.001f, 0, 0);

            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 20; turn++)
        {
            mesh.transform.Rotate(0, 4, 0);
            mesh.transform.Translate (0.001f, 0, 0);

            yield return new WaitForSeconds(0.01f);
        }
        for (int turn = 0; turn < 10f; turn++)
        {
            mesh.transform.Rotate(0, -4, 0);
            mesh.transform.Translate (-0.001f, 0, 0);

            yield return new WaitForSeconds(0.01f);
        }

        if (lastOdo)
        {　
            lastOdo = false;
            //ここの関数終了宣言
            yield break;
        }
        StartCoroutine("OdoOdo");
    }

    void Approach()
    {
        navMesh.SetDestination(currentPosition);

        if (isFar)
        {
            if (navMesh.remainingDistance <= approachDistance && !navMesh.pathPending)
            {
                isFar = false;

                Debug.Log("人にたどり着いた");

                //StartCoroutine("JoyTurn");

                ChangeState(State.happy);
                //navMesh.isStopped = true;
                return;
            }
        }

        if (navMesh.remainingDistance > 3f && !navMesh.pathPending)
        {
            isFar = true;
        }
                

        //人を見失ったらSerchする
        if (!existHuman)
        {
            ChangeState(State.search);
        }
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