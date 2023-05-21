using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class EscapeCubeController : MonoBehaviour
{
    private NavMeshAgent navMesh;
    
    private Vector3 currentPosition = Vector3.zero;

    private State currentState = State.search; //現在のステート
    private bool stateEnter = true;

    //範囲に人間がいるかどうか
    private bool existHuman = false;


    Coroutine serchCoroutine = null;

    public float approachSpeed;
    public float movingSpeed;

    [SerializeField,Range(5,30)] private float _runAwayDistance = 10f;

    public GameObject sphere;


    /// <summary>
    /// プレイヤーが近くにいるかどうかのReactiveProperty
    /// </summary>
    private readonly ReactiveProperty<bool> _isNearPlayerReactiveProperty = new ReactiveProperty<bool>();
    
    private Quaternion _initialRotation; // 初期回転
    
    enum State
    {
        search,
        moving,
        approach,
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
        GameManager.instance.state.text = currentState.ToString();
        Debug.Log(currentState);
    }

    // Start is called before the first frame update
    void Start()
    {
        navMesh = GetComponent<NavMeshAgent>();
        //キョロキョロ止める
        if (serchCoroutine != null)  
        {
            StopCoroutine(serchCoroutine);  
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float randomXPos;
        float randomZPos;
        int PlusMinus;

        //Debug.Log(navMesh.isStopped);

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
                    
                    // StartCoroutineの戻り値が停止させるコルーチン  
                    serchCoroutine = StartCoroutine(Searching());  
                }
                break;
            
            case State.moving:
                if (stateEnter)
                {
                    navMesh.isStopped = false;
                    stateEnter = false;
                    navMesh.speed = movingSpeed;
                    
                    
                    Debug.Log("トコトコトコトコ");
                    GameManager.instance.message.text = "トコトコトコトコ";
                    
                    randomXPos = Random.Range(14f, 23f);
                    randomZPos = Random.Range(6f, 14f);
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

                if (existHuman)
                {
                    ChangeState(State.approach);
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
                    
                }

                Escape();

                

                break;
        }

        existHuman = false;

    }


    public void OnDetectHuman(Collider collider)
    {
        if (collider.gameObject.layer == 8)
        {
            existHuman = true;
            //カメラからプレイヤーの位置と自身までの距離を計算
            var playerTransform = collider.transform;
            var distance = Vector3.Distance(playerTransform.position, transform.position);
            //NavMeshの目的地を計算
            var direction =  (transform.position - playerTransform.position).normalized;
            direction.y = 0;
            var destination = transform.position + direction * _runAwayDistance;

        
            _isNearPlayerReactiveProperty.Value = true;
            navMesh.SetDestination(destination);
            currentPosition = destination;
            Debug.Log("逃げろ");
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

    

    void Escape()
    {
        navMesh.SetDestination(currentPosition);

        //範囲に人がいなくなったら
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