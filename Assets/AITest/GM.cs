using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    public static GM instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public Transform livingRoomPos;
    public Transform kitchenPos;
    public Transform bedPos;
    public Transform toiletPos;
    public Transform washHandPos;

    public TextMeshProUGUI text;
}
