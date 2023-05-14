using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollisionDetector : MonoBehaviour
{
    [SerializeField] private TriggerEvent onTriggerStay = new TriggerEvent();
    [SerializeField] private TriggerEvent onTriggerExit = new TriggerEvent();
    
    private void OnTriggerStay(Collider other)
    {
        // onTriggerStayで指定された処理を実行する
        onTriggerStay.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // onTriggerExitで指定された処理を実行する
        onTriggerExit.Invoke(other);
    }

    // UnityEventを継承したクラスに[Serializable]属性を付与することで、Inspectorウインドウ上に表示できるようになる。
    [Serializable]
    public class TriggerEvent : UnityEvent<Collider>
    {
    }
}