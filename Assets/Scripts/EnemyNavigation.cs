using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


/// <summary>
/// 敵のNavMesh上での挙動
/// </summary>
public class EnemyNavigation : MonoBehaviour
{
    [SerializeField,Range(5,30)] private float _runAwayDistance = 10f;

    public GameObject sphere;

    /// <summary>
    /// 自身にアタッチされたNavMeshAgent
    /// </summary>
    private NavMeshAgent _myAgent;

    /// <summary>
    /// プレイヤーが近くにいるかどうかのReactiveProperty
    /// </summary>
    private readonly ReactiveProperty<bool> _isNearPlayerReactiveProperty = new ReactiveProperty<bool>();

    void Start()
    {
        _myAgent = GetComponent<NavMeshAgent>();

        //プレイヤーが近くにいるか監視
        _isNearPlayerReactiveProperty
            .Where(isNear => !isNear)
            .Subscribe(_ => SetRandomDestinationAsync(this.GetCancellationTokenOnDestroy()).Forget())
            .AddTo(this);
    }

    void Update()
    {
        /*
        //カメラからプレイヤーの位置と自身までの距離を計算
        var playerTransform = sphere.transform;
        var distance = Vector3.Distance(playerTransform.position, transform.position);
        //NavMeshの目的地を計算
        var direction =  (transform.position - playerTransform.position).normalized;
        direction.y = 0;
        var destination = transform.position + direction * _runAwayDistance;

        //プレイヤーとの距離を判定
        if (distance >= _runAwayDistance)
        {
            _isNearPlayerReactiveProperty.Value = false;
        }
        else
        {
            _myAgent.SetDestination(destination);
            _isNearPlayerReactiveProperty.Value = true;
        }
        */
    }

    private void FixedUpdate()
    {
        //_isNearPlayerReactiveProperty.Value = true;

    }

    public void CheckTrueBool(Collider collider)
    {
        if (collider.gameObject.layer == 8)
        {
            //カメラからプレイヤーの位置と自身までの距離を計算
            var playerTransform = collider.transform;
            var distance = Vector3.Distance(playerTransform.position, transform.position);
            //NavMeshの目的地を計算
            var direction =  (transform.position - playerTransform.position).normalized;
            direction.y = 0;
            var destination = transform.position + direction * _runAwayDistance;

        
            _isNearPlayerReactiveProperty.Value = true;
            _myAgent.SetDestination(destination);
            Debug.Log("逃げろ");
        }
        
        
    }
    
    public void CheckFalseBool()
    {
    }

    /// <summary>
    /// ランダムな目的地を設定
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    private async UniTask SetRandomDestinationAsync(CancellationToken ct)
    {
        while (!_isNearPlayerReactiveProperty.Value)
        {
            var randomValueX = Random.Range(-23, 23);
            var randomValueZ = Random.Range(-14, 14);
            if (_myAgent == null) return;
            _myAgent.SetDestination(new Vector3(randomValueX,0,randomValueZ));
            await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: ct);
        }
    }
}