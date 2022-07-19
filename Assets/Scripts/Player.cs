using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _navAgent;
    private Rigidbody _rigidBody;
    private Vector2 _movement;
    
    [SerializeField]
    private float noiseUpSpeed;
    [SerializeField]
    private float noiseDownSpeed;
    [SerializeField]
    private float maxNoise;
    [SerializeField]
    private GameObject noiseBar;
    
    [SerializeField]
    private AudioSource _audio;

    private float noiseLevel = 0;
    private Vector3 _direction;

    public UnityEvent Die;
    public UnityEvent MaxNoise;
    public UnityEvent Win;


    private bool inGame = true;
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _rigidBody = GetComponent<Rigidbody>();
        _direction = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if (!inGame) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        _direction = new Vector3(horizontal * 0.6f, 0, vertical * 0.6f);

        _navAgent.SetDestination(transform.position + _direction);

        
        if (_direction.sqrMagnitude > 0)
        {
            noiseLevel += noiseUpSpeed * Time.deltaTime;
            _animator.SetInteger("State", 1);
        }
        else
        {
            noiseLevel -= noiseDownSpeed * Time.deltaTime;
            _animator.SetInteger("State", 0);
        }
        if(noiseLevel < 0)
        {
            noiseLevel = 0;
        }
        if(noiseLevel > maxNoise)
        {
            noiseLevel = maxNoise;
            MaxNoise.Invoke();
        }

        noiseBar.transform.localScale = new Vector3(noiseLevel / maxNoise, 1, 1);
        _audio.volume = noiseLevel / maxNoise;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            StopPlayer();
            _animator.SetInteger("State", 2);
            Die.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            StopPlayer();
            _animator.SetInteger("State", 7);
            Win.Invoke();
        }
    }

    private void StopPlayer()
    {
        inGame = false;
        _audio.volume = 0;
        _navAgent.SetDestination(transform.position);
        _navAgent.speed = 0;
        _animator.speed = 1;
    }

    public void SetNoise(float max, float noiseUSpeed, float noiseDSpeed)
    {
        maxNoise = max;
        noiseUpSpeed = noiseUSpeed;
        noiseDownSpeed = noiseDSpeed;
    }
}
