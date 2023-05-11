using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void OnDataReceived(Message message)
    {

        if (message.address == "/pos")
        {
            float X = float.Parse(message.values[0].ToString());
            float Y = float.Parse(message.values[1].ToString());
            
            Debug.Log(X + "," + Y);
            Instantiate(mark, new Vector3( X * num, Y * num, 0.0f), Quaternion.identity);
        }
    }

    void InstantiateMark(float X)
    {
        Instantiate(mark, new Vector3( X, 0.0f, 0.0f), Quaternion.identity);
    }

}
