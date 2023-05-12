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
    
    private NavMeshAgent navMesh;
    
    private Vector3 clickPosition;
    
    private Camera mainCamera;
    private Vector3 currentPosition = Vector3.zero;
    
    private State currentState = State.search;//現在のステート
    private bool stateEnter = true;

    private Rigidbody rb;

    private bool existsObstacle = false;
    
    enum State
    {
        search,
        moving,
        rotating,
        goTowards,
        

    }
    
    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        navMesh = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;
        
        rb = this.GetComponent<Rigidbody> ();  // rigidbodyを取得

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
            navMesh.isStopped = false;
            ChangeState(State.goTowards);
        }

        switch (currentState)
        {
            case State.search:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.text.text = "キョロキョロ";

                    StartCoroutine("Searching");

                }
                
                break;
            case State.moving:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("トコトコトコトコ");
                    GameManager.instance.text.text = "トコトコトコトコ";
                }

                if (existsObstacle)
                {
                    ChangeState(State.rotating);
                    Debug.Log(currentState +"中に壁にぶつかった");
                    GameManager.instance.text.text = currentState +"中に壁にぶつかった";
                    
                    break;
                }
                
                int randomIndex = Random.Range(0, 3);
                var transform1 = transform;
                transform1.position += transform1.forward * (speed * Time.deltaTime); 
                break;
            case State.goTowards:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("向かってます");
                    GameManager.instance.text.text = "向かってます";

                    navMesh.SetDestination(currentPosition);
                }
                
                if (navMesh.remainingDistance <= 0.2f && !navMesh.pathPending)
                {
                    ChangeState(State.search);
                    navMesh.isStopped = true;
                    return;
                }
                break;
            
            case State.rotating:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.text.text = "キョロキョロ";

                    
                    StartCoroutine("Rotating");
                }
                
                break;
        }
    }
    

    // CollisionDetectorのonTriggerStayにセットし、衝突判定を受け取るメソッド
    public void OnDetectObject(Collider collider)
    {
        Debug.Log("壁にぶつかった");
        GameManager.instance.text.text = "壁にぶつかった";


        existsObstacle = true;

        //ChangeState(State.rotating);
    }
    
    // CollisionDetectorのonTriggerExitにセットし、衝突判定を受け取るメソッド
    public void OutDetectObject(Collider collider)
    {
        Debug.Log("壁を避けた");
        GameManager.instance.text.text = "壁を避けた";


        existsObstacle = false;

    }

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
    
    IEnumerator Rotating()
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
