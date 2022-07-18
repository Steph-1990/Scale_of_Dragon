using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionWithPlayer : MonoBehaviour
{
    [SerializeField] private EnemyData _enemyData; // Scriptable Object qui contient des infos sur l'ennemi et notamment son type
    [SerializeField] private EnemyData _collidedEnemy; // Scriptable Object qui enregistre le type du dernier ennemi entr� en contact avec le player
    [SerializeField] private BoolVariable _collision; // Passe � true lors de la collision avec le player
    [SerializeField] private FloatVariable _transitionDelay; // D�lai de transition entre les sc�nes
    [SerializeField] private BoolVariable _transitionIsOver;
    
    private ExplorationAudioManager _audioManager;
    private float _collisionTime; // Temps lors de la collision

    private void Update()
    {
        if (_collisionTime != 0 && Time.time > _collisionTime + (_transitionDelay.value - 0.1f))
        {
            Destroy(gameObject); // Si collision, on d�truit l'ennemi juste avant la fin de la transition
        }
    }

    private void OnTriggerStay2D(Collider2D collision) // OnTriggerOnStay au cas o� un deuxi�me ennemi se trouve dans la zone de collision au moment du d�part pour la sc�ne suivante
    {
        if (collision.CompareTag("Player") && !_transitionIsOver.value)
        {
            DetectEnemyType();
            _collision.value = true;
            _collisionTime = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_transitionIsOver.value)
        {
            _audioManager = FindObjectOfType<ExplorationAudioManager>();
            _audioManager.PlayEnemyCollisionSFX();
        }
    }

    private void DetectEnemyType() // En fonction du type de l'ennemi entr� en collision avec le joueur on entre un index dans le scriptable pour savoir quel prefab instancier dans la sc�ne suivante
    {
        if(_enemyData.type == EnemyType.SKELETON)  
        {
            _collidedEnemy.type = EnemyType.SKELETON;
        }
        else if (_enemyData.type == EnemyType.MAIDZOMBIE)
        {
            _collidedEnemy.type = EnemyType.MAIDZOMBIE;
        }
        else if (_enemyData.type == EnemyType.GHOST)
        {
            _collidedEnemy.type = EnemyType.GHOST;
        }
    }
}
