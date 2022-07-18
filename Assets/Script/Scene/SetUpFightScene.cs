using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUpFightScene : MonoBehaviour
{
    [SerializeField] private GameObject _skeletonPrefab;
    [SerializeField] private GameObject _maidZombiePrefab;
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private Transform[] _enemyPopPoints; // Points de pop des ennemis qui varie en fonction de leur nombre
    [SerializeField] private EnemyData _collidedEnemy; // R�f�rence au type de l'ennemi entr� en collision avec le joueur

    private GameObject _enemy; // Variable qui contient les ennemies instanci�s
    private UpdateUI _updateUI;
    private EnemyTransform _enemyTransform;
    private int _enemyNumber; // Nombre d'ennemi qui pop au d�but du fight
 
    public int EnemyNumber { get => _enemyNumber; set => _enemyNumber = value; }
    public EnemyData CollidedEnemy { get => _collidedEnemy; set => _collidedEnemy = value; }

    private void Awake()
    {
        _updateUI = FindObjectOfType<UpdateUI>();
        _enemyTransform = FindObjectOfType<EnemyTransform>();

        GenerateRandomNumber();
        InstantiateEnemy();
    }

    private void Update()
    {
        _updateUI.SetUIInformation(); // On met � jour les informations de l'UI au d�but du combat (nombre et type d'ennemies, PV des h�ros, etc...)
    }
    
    private void GenerateRandomNumber() // G�n�re un nombre entier al�atoire entre 1 et 3 (nombre d'ennemi � instancier) 
    {
        _enemyNumber = Random.Range(1, 4);
    }

    private Transform EnemyTransform(int i) // Renvoie le transform permettant de placer l'ennemi instanci� sur l'�cran
    {
        return _enemyPopPoints[_enemyNumber - 1].GetChild(i); // 3 ==> _enemyNumber
    }

    private void InstantiateEnemy() // On instancie le bon ennemi en fonction du type qui a �t� enregistr� lors de la collision
    {
        for (int i = 0; i < _enemyNumber; i++) // 3 ==> _enemyNumber
        {
            if (_collidedEnemy.type == EnemyType.SKELETON)
            {
                _enemy =  Instantiate(_skeletonPrefab, EnemyTransform(i));
            }
            else if (_collidedEnemy.type == EnemyType.MAIDZOMBIE)
            {
                _enemy = Instantiate(_maidZombiePrefab, EnemyTransform(i));
            }
            else if (_collidedEnemy.type == EnemyType.GHOST)
            {
                _enemy = Instantiate(_ghostPrefab, EnemyTransform(i));
            }

            _enemy.transform.SetParent(_enemyTransform.transform); // On replace les ennemies au bon endroit dans la hi�rarchie
        }
    } 
}
