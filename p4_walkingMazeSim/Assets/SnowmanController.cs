using Unity.VisualScripting;
using UnityEngine;

public class SnowmanController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.6f;
    [SerializeField] float rotationSpeed = 6f;
    [SerializeField] float stopDistance = 1.2f;

    [SerializeField] float catchDistance = 1.1f;

    [SerializeField] float groundOffset = 0f;

    Transform _target;
    bool _isWatched;
    bool _isActive;
    bool _hasCaught;

    public bool IsActive => _isActive;

    public void Init(Transform player)
    {
        _target = player;
        _isActive = true;
        _hasCaught = false;
    }

    void Start()
    {
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                Init(player.transform);
            else
                Debug.LogWarning("SnowmanController: Didn't found any onject with Player tag");
        }
    }

    public void SetWatched(bool watched) => _isWatched = watched;

    void Update()
    {
        if (!_isActive || _target == null || _hasCaught) return;

        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <=catchDistance)
        {
            CatchPlayer();
            return;
        }

        FaceTarget(toTarget);

        if (!_isWatched)
            MoveTowards(toTarget);
    }

    void FaceTarget(Vector3 toTarget)
    {
        if (toTarget.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
    }

    void MoveTowards(Vector3 toTarget)
    {
        if (toTarget.magnitude <= stopDistance) return;

        Vector3 step = toTarget.normalized * (moveSpeed * Time.deltaTime);
        step.y = 0f;
        transform.position += step;
        transform.position = new Vector3(transform.position.x, transform.position.y + groundOffset, transform.position.z);

    }

    void CatchPlayer()
    {
        _hasCaught = true;
        SnowmanDirector.Instance?.OnPlayerCaught();
    }
}
