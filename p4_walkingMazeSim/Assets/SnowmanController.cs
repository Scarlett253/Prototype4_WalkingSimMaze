using UnityEngine;
using UnityEngine.AI;

public class SnowmanController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.6f;
    [SerializeField] float rotationSpeed = 6f;
    [SerializeField] float stopDistance = 1.2f;
    [SerializeField] float catchDistance = 1.1f;

    Transform _target;
    NavMeshAgent _agent;
    bool _isWatched;
    bool _isActive;
    bool _hasCaught;

    public bool IsActive => _isActive;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null)
        {
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = stopDistance;
            _agent.updateRotation = false;
            _agent.autoBraking = true;
        }
    }

    public void Init(Transform player)
    {
        _target = player;
        _isActive = true;
        _hasCaught = false;

        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = false;
    }

    public void SetWatched(bool watched) => _isWatched = watched;

    void Update()
    {
        if (!_isActive || _target == null || _hasCaught) return;

        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        if (_agent == null || !_agent.isOnNavMesh) return;

        _agent.isStopped = _isWatched;

        if (!_isWatched)
            _agent.SetDestination(_target.position);

        FaceTarget(toTarget);
    }

    void FaceTarget(Vector3 toTarget)
    {
        if (toTarget.sqrMagnitude < 0.001f) return;
        Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
    }

    void CatchPlayer()
    {
        _hasCaught = true;
        if (_agent != null) _agent.isStopped = true;
        SnowmanDirector.Instance.OnPlayerCaught();
    }
}
