using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent _navAgent;
    public Animator _animator;

    [SerializeField] Animator colorAnimator;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private GameObject _radar;
    [SerializeField]
    private GameObject _model;

    
    private SkinnedMeshRenderer _meshRender;
    private AudioSource _audio;

    private Vector3 startPoint;
    private Vector3 endPoint;

    delegate void CurrentAction();
    CurrentAction currentAction;


    private void Awake()
    {
        startPoint = new Vector3(transform.position.x, 0, transform.position.z); 
        _navAgent = GetComponent<NavMeshAgent>();
        _meshRender = _model.GetComponent<SkinnedMeshRenderer>();
        _audio = GetComponent<AudioSource>();
        _animator = GetComponentInChildren<Animator>();
        currentAction = Patrule;
        
    }
    void Start()
    {
        
    }

    public void SetEndPoint(Vector3 destination)
    {
        endPoint = new Vector3(destination.x, destination.y, destination.z);
        _navAgent.SetDestination(endPoint);
    }

    public void SetTargetAndStartFollow(Transform target)
    {
        _audio.Play();
        this.target = target;

        //_meshRender.material.color = new Color(255, 0, 0);
        colorAnimator.SetInteger("state", 1);
        
        _radar.SetActive(false);
        
        currentAction = FollowPlayer;
    }

    private void FixedUpdate()
    {
        currentAction();
    }

    private void Update()
    {
        if(_navAgent.speed > 0)
        {
            _animator.SetInteger("State", 1);
        } 
        else
        {
            _animator.SetInteger("State", 0);
        }
    }

    private void Patrule()
    {
        if(endPoint != null)
        if((transform.position - endPoint).sqrMagnitude < 0.1f)
        {
            FlipPoints();

            _navAgent.SetDestination(endPoint);
        }
    }

    private void FlipPoints()
    {
        var emptyPoint = startPoint;
        startPoint = endPoint;
        endPoint = emptyPoint;
    }

    private void FollowPlayer()
    {
        if(target != null)
        {
            _navAgent.SetDestination(target.position);
        }
    }

    public void StopFollow()
    {
        _navAgent.SetDestination(this.transform.position);
        _navAgent.speed = 0;
        currentAction = DoNothing;
    }

    private void DoNothing()
    {

    }
}
