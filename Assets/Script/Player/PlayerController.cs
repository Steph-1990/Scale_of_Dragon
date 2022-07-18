using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 5f; // Vitesse de d�placement du personnage lorsqu'il marche
    [SerializeField] private float _sprintSpeed = 10f; // Vitesse de d�placement du personnage en sprint
    [SerializeField] private BoolVariable _collision; // Passe � true si collision avec l'ennemi

    private float _currentSpeed; // Vitesse de d�placement actuel du personnage
    private Vector2 _lastDirection; // La derni�re direction dans laquelle le personnage regarde avant de s'arr�ter
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private PlayerInput _playerInput;

    public float WalkSpeed { get => _walkSpeed; set => _walkSpeed = value; }
    public float SprintSpeed { get => _sprintSpeed; set => _sprintSpeed = value; }
    public float CurrentSpeed { get => _currentSpeed; set => _currentSpeed = value; }

    private void Awake()
    {
        _rigidbody = GetComponentInParent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        SetAnimatorParameters();
        SaveLastDirection();
    }

    private void FixedUpdate()
    {
        ApplyInput();
    }

    private void ApplyInput() // On applique les entr�es clavier/manettes au Rigidbody pour d�placer le personnage ou l'emp�cher de bouger si il est entr� en collision avec l'ennemi
    {
        if (_collision.value)
        {
            _playerInput.enabled = false;
            _rigidbody.velocity = Vector2.zero;
        }
        else
        {
            _playerInput.enabled = true;
            _rigidbody.velocity = _playerInput.NormalizedMovement * _currentSpeed;
        }
    }

    private void SetAnimatorParameters() // Ajuste les param�tres de l'animator en fonction des d�placements du personnage
    {
        _animator.SetFloat("VectorX", _lastDirection.x);
        _animator.SetFloat("VectorY", _lastDirection.y);
    }

    private void SaveLastDirection() // On sauvegarde la derni�re direction du joueur
    {
        if (_playerInput.HasMovement) 
        {
            _lastDirection = _playerInput.NormalizedMovement; 
        }
    }
}
