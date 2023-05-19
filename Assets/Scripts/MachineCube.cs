using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MachineCube : MonoBehaviour
{
    private NavMeshAgent navMesh;

    public float speed; 

    
    private State currentState = State.moving;//現在のステート
    private bool stateEnter = true;
    
    //前方に障害物があるかどうか
    private bool existsObstacle = false;
    
    enum State
    {
        moving,
        avoid,
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

    }

    // Update is called once per frame
    void Update()
    {


        switch (currentState)
        {
            case State.moving:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.message.text = "キョロキョロ";


                }

                var transform1 = transform;
                transform1.position += transform1.forward * (speed * Time.deltaTime);

                if (existsObstacle)
                {
                    ChangeState(State.avoid);
                }
                break;
            case State.avoid:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("キョロキョロ");
                    GameManager.instance.message.text = "キョロキョロ";
                    StartCoroutine("Avoid");


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

            existsObstacle = true;
        }

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

        existsObstacle = false;
        ChangeState(State.moving);

    }
}
