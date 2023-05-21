using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class CharaController : MonoBehaviour
{
    public float speed; 
    
    private　List<Vector3> humanPosList = new List<Vector3>();
    
    
    private NavMeshAgent navMesh;
    
    private Vector3 clickPosition;
    
    private Camera mainCamera;
    private Vector3 currentPosition = Vector3.zero;
    
    private State currentState = State.search;//現在のステート
    private bool stateEnter = true;


    //前方に障害物があるかどうか
    private bool existsObstacle = false;
    //人間を探すかどうか
    private bool isSearchHuman = true;
    //現在人間を追跡しているかどうか
    //private bool isTracking = true;

    
    enum State
    {
        search,
        moving,
        avoid,
        goTowards,
        bePleassed,
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
        
        if (Input.GetMouseButtonDown(0))
        {
            //var distance = Vector3.Distance(player.transform.position, mainCamera.transform.position);
            var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y);
 
            currentPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            Debug.Log("LeftClick:"+currentPosition );
            
            ChangeState(State.goTowards);
            
            
            humanPosList.Add(currentPosition);
        }

        switch (currentState)
        {
            case State.search:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.message.text = "キョロキョロ";

                    StartCoroutine("Searching");

                }
                
                break;
            case State.moving:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("トコトコトコトコ");
                    GameManager.instance.message.text = "トコトコトコトコ";
                }

                if (existsObstacle)
                {
                    Debug.Log(currentState +"中に壁にぶつかった");
                    GameManager.instance.message.text = currentState +"中に壁にぶつかった";
                    ChangeState(State.avoid);

                    
                    break;
                }
                
                var transform1 = transform;
                transform1.position += transform1.forward * (speed * Time.deltaTime); 
                break;
            
            case State.goTowards:
                if (stateEnter)
                {
                    stateEnter = false;
                    navMesh.isStopped = false;
                    Debug.Log("向かってます");

                    GameManager.instance.message.text = "向かってます";


                    //目標地点がリセットされた状態なら新しく目的地を
                    if (currentPosition != Vector3.zero)
                    {

                    }
                }
                navMesh.SetDestination(currentPosition);

                if (navMesh.remainingDistance <= 3f && !navMesh.pathPending)
                {
                    Debug.Log("人にたどり着いた");

                    ChangeState(State.bePleassed);
                    navMesh.isStopped = true;
                    return;
                }
                //Debug.Log(navMesh.remainingDistance);

                break;
            
            case State.avoid:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("避ける");

                    GameManager.instance.message.text = "避ける";

                    
                    StartCoroutine("Avoid");
                }
                
                break;
            
            case State.bePleassed:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("嬉しい！");
                    GameManager.instance.message.text = "嬉しい!";
                    StartCoroutine("Turn");
                }
                
                break;
            
            case State.goHome:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("お家に帰る");
                    GameManager.instance.message.text = "お家に帰る";
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

            //GameManager.instance.message.text = "壁にぶつかった";


            //existsObstacle = true;

            //ChangeState(State.rotating);
        }
    }
    
    // CollisionDetectorのonTriggerExitにセットし、衝突判定を受け取るメソッド
    public void OutDetectObject(Collider collider)
    {
        if (collider.gameObject.layer == 9)
        {
            Debug.Log("壁を避けた");

            //GameManager.instance.message.text = "壁を避けた";


            existsObstacle = false;
        }
    }
    
    public void OnDetectHuman(Collider collider)
    {
        if (isSearchHuman)
        {
            if (collider.gameObject.layer == 8)
            {
                navMesh.isStopped = false;

                
                // 衝突位置を取得する
                Vector3 hitPos = collider.transform.position;
                //navMesh.SetDestination(hitPos);
                currentPosition = hitPos;
                
                //既に人間を追跡しているなら以下の処理を飛ばす
                if (currentState == State.goTowards || currentState == State.bePleassed)
                {
                    return;
                }
                
                Debug.Log("人を見つけた");

                GameManager.instance.message.text = "人を見つけた";
                
                ChangeState(State.goTowards);
            }
        }
    }

    //キョロキョロと周りを見渡す
    IEnumerator Searching()
    {
        for (int turn=0; turn<45; turn++)
        {
            transform.Rotate(0,1,0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int turn=0; turn<90; turn++)
        {
            transform.Rotate(0,-1,0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int turn=0; turn<45; turn++)
        {
            transform.Rotate(0,1,0);
            yield return new WaitForSeconds(0.01f);
        }
        ChangeState(State.moving);
    }
    
    //人の近くにたどり着くと回転して喜ぶ
    IEnumerator Turn()
    {
        isSearchHuman = false;
        for (int turn=0; turn<140; turn++)
        {
            transform.Rotate(0,3,0);
            yield return new WaitForSeconds(0.01f);
        }
        isSearchHuman = true;

        //追跡位置をリセットする
        currentPosition = Vector3.zero;
        ChangeState(State.search);

    }
    
    //右か左にランダムに避ける
    IEnumerator Avoid()
    {
        int randomIndex = Random.Range(0, 2);
        if (randomIndex == 0)
        {
            for (int turn=0; turn<120; turn++)
            {
                transform.Rotate(0,1,0);
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            for (int turn=0; turn<120; turn++)
            {
                transform.Rotate(0,-1,0);
                yield return new WaitForSeconds(0.01f);
            }
        }

        
        ChangeState(State.moving);
    }

    void OnDrawGizmos()
    {
        if (currentPosition != Vector3.zero) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentPosition, 0.5f);
        }
    }
}
