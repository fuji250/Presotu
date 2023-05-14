using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using uOSC;

public class OSCController : MonoBehaviour
{
    public GameObject mark;
    public float num = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        var server = GetComponent<uOscServer>();
        server.onDataReceived.AddListener(OnDataReceived);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log(Input.mousePosition);
            
            Ray ray = Camera.main.ScreenPointToRay(new Vector3( 0.0f, 0.0f, 0.0f));
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit,30.0f))
            {
                Debug.Log(hit.collider.gameObject.transform.position);
            }
            Debug.DrawRay(ray.origin, ray.direction * 30, Color.cyan, 1);
        }
        
    }
    private void OnDataReceived(Message message)
    {
        
        if (message.address == "/pos")
        {
            float X = float.Parse(message.values[0].ToString());
            float Y = float.Parse(message.values[1].ToString());


            //センサーに近づぎる値は外す
            if (Mathf.Abs(X) <= 0.1f && Mathf.Abs(Y) <= 0.1f)
            {
                return;
            }

            Vector3 rayPosition = new Vector3(X * num, 0.5f, Y * num);

                Ray ray = new Ray(rayPosition, Vector3.down);
                Debug.DrawRay(rayPosition, Vector3.down, Color.yellow,1f);
                RaycastHit hit;

                //その地点に既にオブジェクトがある　または
                //人間以外のレイヤーなら球は追加しない
                if(Physics.Raycast(ray, out hit, 1)　&& hit.collider.gameObject.layer != 8)
                { 
                    //Debug.Log("Hit");
                    
                    return;
                }
                //Debug.Log(X + "," + Y);

                Instantiate(mark, new Vector3( X * num, 0.0f, Y * num), Quaternion.identity);
            
        }
        
    }

    void InstantiateMark(float X)
    {
        Instantiate(mark, new Vector3( X, 0.0f, 0.0f), Quaternion.identity);
    }

}
