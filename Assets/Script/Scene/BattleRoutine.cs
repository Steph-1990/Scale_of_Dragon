using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleRoutine : MonoBehaviour
{
    [SerializeField] private GameObject _actionMenu; // Le menu de selection d'action
    [SerializeField] private Transform _playerCursor; // Curseur permettant de savoir quel h�ros est selectionn� lors du choix d'action dans le menu
    [SerializeField] private Transform _enemyCursor; // Le cursor pr�sent sur la t�te d'un ennemi lors du choix de l'ennemi � attaquer
    [SerializeField] private RectTransform _damageText;
    [SerializeField] private Camera _camera;
    [SerializeField] private AudioSource _cursorSFX;
    [SerializeField] private float _charactersAnimationSpeed; // Vitesse d'animation des personnages

    [SerializeField] public List<Characters> _charactersList = new List<Characters>(); // Liste de tous les personnages pr�sents sur la sc�ne
    [SerializeField] private List<PlayerHealth> _heroesList = new List<PlayerHealth>(); // Liste de tous les h�ros pr�sents sur la sc�ne
    [SerializeField] private List<EnemyHealth> _enemiesList = new List<EnemyHealth>(); // Liste de tous les ennemis pr�sents sur la sc�ne
    [SerializeField] private List<GameObject> _applyDamageToCharacter = new List<GameObject>(); // Liste de l'ordre dans lequel les personnages doivent subir des d�g�ts
    [SerializeField] private List<int> _heroesAttackOrder = new List<int>(); // Ordre dans lequel attaque les h�ros
    [SerializeField] private List<bool> _defend = new List<bool>(); // Liste de bool�en qui permet de savoir si le personnage est en train de d�fendre
    
    private int _sortedCharactersIndex; // Index de la liste des personnages tri�s en fonction de leur agilit�
    private int _movementIndex; // Index qui pr�cise quel vecteur de mouvement doit �tre utilis� en fonction de chaque animation
    private int _enemyIndex; // Index de la liste d'ennemis
    private int _heroToAttackIndex; // Nombre al�atoire compris entre 0 et 2 pour conna�tre l'ennemi � attaquer 
    private float _animationTimer; //Timer qui se lance � chaque d�but d'animation
    private float _safetyMargin = 0.01f; // Marge de s�curit� pour emp�cher l'animation d'un personnage de se lancer une seconde fois
    private bool _joystickIsPressed; // Passe � true si le joystick est pouss� vers l'avant ou vers l'arri�re
    private bool _attackButtonReleased; // Index pour g�rer l'appui des boutons de la manette lors de la navigation menu
    private bool _attackButtonPressed; // Passe � true lors de l'appui sur le bouton "Attaquer"
    private bool _enemyIsDead;
    private bool _playerHasMadeChoice; // Passe � true si le joueur a fini de choisir les actions � effectuer pour ses personnages  
    private bool _defendButtonPressed; // Passe � true si le joueur choisi "D�fendre"
    private Vector3[] _enemiesMovement; // Vecteurs de mouvement des ennemies
    private Vector3[] _heroesMovement;  // Vecteurs de mouvement des h�ros
    private Vector3[] _charactersMovement; // Vecteurs de mouvement des personnages qui varie suivant qu'il soit un h�ros ou un ennemi
    private Vector2 _screenPos; // Position de l'affichage des d�g�ts
    private Animator _characterAnimator; // Stocke temporairement l'animator d'un personnage lorsque ce dernier est actif
    private Transform _characterTransform; // Stocke temporairement le transform d'un personnage lorsque ce dernier est actif
    private PlayerHealth _playerHealth;
    private EnemyHealth _enemyHealth;
    private EndFight _endFight;
    private BattleAudioManager _battleAudioManager;



    public bool EnemyIsDead { get => _enemyIsDead; set => _enemyIsDead = value; }
    public bool AttackButtonPressed { get => _attackButtonPressed; set => _attackButtonPressed = value; }
    public bool DefendButtonPressed { get => _defendButtonPressed; set => _defendButtonPressed = value; }
    public int SortedCharactersIndex { get => _sortedCharactersIndex; set => _sortedCharactersIndex = value; }
    public List<bool> Defend { get => _defend; set => _defend = value; }

    private void Awake()
    {
        _playerHealth = FindObjectOfType<PlayerHealth>();
        _battleAudioManager = FindObjectOfType<BattleAudioManager>();
        _endFight = GetComponent<EndFight>();
    }

    private void Start()
    {
        _charactersList = new List<Characters>(FindObjectsOfType<Characters>()); // Au Start pour laisser le temps aux ennemies d'appara�tre
        _enemiesList = new List<EnemyHealth>(FindObjectsOfType<EnemyHealth>());
        _enemiesMovement = new Vector3[] { new Vector3(4*_charactersAnimationSpeed, 0, 0), new Vector3(0, 0, 0), new Vector3(-4*_charactersAnimationSpeed, 0, 0) };
        _heroesMovement = new Vector3[] { new Vector3(-4*_charactersAnimationSpeed, 0, 0), new Vector3(0, 0, 0), new Vector3(4*_charactersAnimationSpeed, 0, 0) };     
        SortCharacters(); // On trie les personnages en fonction de leur agilit�
    }

    private void Update()
    {
        if (_enemyIsDead)
        {
            UpdateCharactersNumber(); // On met � jour les personnages pr�sents sur la sc�ne
            _enemyIsDead = false; // On repasse la valeur � false pour check si d'autres ennemis meurt
        }
        else if (_enemiesList.Count == 0 && _movementIndex == 0) // Si il n'y a pas d'ennemi sur la sc�ne et que tous les h�ros ont finis leur action, on repasse sur la sc�ne exploration
        {
            _endFight.ReturnExplorationScene();
        }
        else
        {
            RunBattleRoutine(); // Sinon on ex�cute la routine de combat
        }
    }


    private void UpdateCharactersNumber() // Mise � jour de la liste des personnages pr�sent sur la sc�ne lors de la mort d'un personnage
    {
        _charactersList = new List<Characters>(FindObjectsOfType<Characters>());
        SortCharacters();
        _enemiesList.Clear();
        _enemiesList = new List<EnemyHealth>(FindObjectsOfType<EnemyHealth>());

        if (_sortedCharactersIndex != 0)
        {
            _sortedCharactersIndex--;
        }
    }

    private void RunBattleRoutine() // Lance la routine de combat
    {
        if (_sortedCharactersIndex == _charactersList.Count) // Lorsque l'on a parcouru toute la liste de personnages on passe du mode selection menu au mode animation ou inversement
        {
            _playerHasMadeChoice = !_playerHasMadeChoice;           
            _sortedCharactersIndex = 0;
        }

        if (_playerHasMadeChoice) // Si le player a fait son choix 
        {
            PlayAnimations(); // On lance les animations
        }
        else
        {
            SelectMenu(); // Sinon on laisse le joueur faire son choix dans le menu       
        }        
    }

    private void RetrieveCharacterComponents() // R�cup�re les composants animator et transform des personnages
    {
        if (_characterAnimator == null)
        {
            _characterAnimator = _charactersList[_sortedCharactersIndex].gameObject.GetComponent<Animator>();
        }

        if (_characterTransform == null)
        {
            _characterTransform = _charactersList[_sortedCharactersIndex].gameObject.transform;
        }      
    }

    private void RetrieveAppropriateVectors() // R�cup�re les vecteurs de mouvements ad�quats suivant que le personnages soit un ennemi ou un h�ros
    {
        if (_charactersList[_sortedCharactersIndex].CharactersData is HeroData)
        {
            _charactersMovement = _heroesMovement;
        }
        else
        {
            _charactersMovement = _enemiesMovement;
        }
    }

    private void MoveCharacters() // D�place le personnage lors de l'animation d'attaque
    {
        if (_movementIndex < _charactersMovement.Length)
        {
            _characterTransform.Translate(_charactersMovement[_movementIndex] * Time.deltaTime); 
        }
        else
        {
            _defend.RemoveAt(0);
            _characterAnimator.SetBool("AnimationIsTriggered", false);
            _characterAnimator = null;
            _characterTransform = null;
            _sortedCharactersIndex++;
            _movementIndex = 0;
        }
    }

    private void PlayAnimations() // Joue les animations des personnages
    {
        RetrieveCharacterComponents(); // On r�cup�re les composants n�cessaires
        RetrieveAppropriateVectors(); // On r�cup�re les vecteurs de mouvements ad�quats

        if (_defend[0])
        {
            _sortedCharactersIndex++;
            _movementIndex = 0;
            _characterAnimator.SetBool("AnimationIsTriggered", false);
            _characterAnimator = null;
            _characterTransform = null;
            _defend.RemoveAt(0);
            _heroesAttackOrder.RemoveAt(0);
        }
        else
        {
            _characterAnimator.speed = _charactersAnimationSpeed; // On applique la vitesse de l'animator en fonction de celle choisit par l'utilisateur
            _animationTimer += Time.deltaTime;

            if (_animationTimer < (_characterAnimator.GetCurrentAnimatorStateInfo(0).length - _safetyMargin))
            {
                _characterAnimator.SetBool("AnimationIsTriggered", true); // on lance l'animation
                MoveCharacters(); // On d�place le personnage tant que le timer ne d�passe pas la dur�e de l'animation
            }
            else // Un fois que le timer d�passe la dur�e de l'animation en cours
            {
                _animationTimer = 0; // On reset le timer
                _movementIndex++; // On passe au mouvement suivant

                if (_movementIndex == 1)
                {
                    ApplyAttackDamage();
                }
            }
        }  
    } 

    private void ApplyAttackDamage() // Applique les dommages 
    {
        _damageText.gameObject.SetActive(false);

        if (_charactersList[_sortedCharactersIndex].CharactersData is HeroData)
        {
            if (_applyDamageToCharacter[0].gameObject != null)
            {
                _applyDamageToCharacter[0].GetComponent<EnemyHealth>().LoseHp();
                DisplayDamage(_applyDamageToCharacter[0].gameObject);
            }
            else
            {
                _enemiesList[0].LoseHp();
                DisplayDamage(_enemiesList[0].gameObject);
            }
            _applyDamageToCharacter.RemoveAt(0); // Une fois les d�g�ts appliqu�s sur le personnage, on supprime ces deg�ts pour ne pas les appliquer une seconde fois
            _heroesAttackOrder.RemoveAt(0);
        }
        else
        {
            _heroToAttackIndex = Random.Range(0, 3);
            _heroesList[_heroToAttackIndex].LoseHP();
            DisplayDamage(_heroesList[_heroToAttackIndex].gameObject);
        }
    }

    private void DisplayDamage(GameObject character) // Affiche les d�g�ts subit sur les personnages
    {
        _screenPos = _camera.WorldToScreenPoint(character.transform.position);
        _damageText.position = _screenPos + new Vector2(0, 80);
        _damageText.gameObject.SetActive(true);
    }

    private void MenuAndCursorDisplayManager(Transform cursor, Vector2 characterPosition, bool showMenu) // G�re l'affichage des curseurs et du menu 
    {
        _actionMenu.SetActive(showMenu); // On active ou non le menu
        cursor.gameObject.SetActive(true); // On active le curseur de selection
        cursor.position = new Vector2(characterPosition.x, characterPosition.y + 3.5f); // On place le curseur au dessus de la t�te du personnage actif
    }

    private void SelectEnemyToAttack() // Choix de la selection de l'ennemi
    {
        Vector2 _enemyPosition = _enemiesList[_enemyIndex].transform.position;
        MenuAndCursorDisplayManager(_enemyCursor, _enemyPosition, false);

        if (Input.GetButtonUp("Submit"))
        {
            _attackButtonReleased = true;
            _battleAudioManager.PlayConfirmButtonSound();
        }

        if (Input.GetButtonDown("Submit") && _attackButtonReleased)
        {
            _battleAudioManager.PlayConfirmButtonSound();
            _heroesAttackOrder.Add(_sortedCharactersIndex);
            _applyDamageToCharacter.Add(_enemiesList[_enemyIndex].gameObject);
            _enemyIndex = 0; // Replace le curseur sur le premier ennemi � chaque d�but de tour
            _enemyCursor.gameObject.SetActive(false); // On d�sactive le curseur de selection de l'ennemi
            _playerCursor.gameObject.SetActive(false); // On d�sactive le curseur de selection du personnage en cours
            _sortedCharactersIndex++; // On passe au personnage suivant
            _attackButtonPressed = false; // On repasse la variable � false pour permettre l'appui sur le bouton
            _attackButtonReleased = false;
            _defend.Add(false);
        }
        else if (Input.GetAxisRaw("Vertical") == 0)
        {
            _joystickIsPressed = false;
        }
        else if (Input.GetAxisRaw("Vertical") == 1 && _enemyIndex < _enemiesList.Count - 1 && _joystickIsPressed == false)
        {
            _battleAudioManager.PlayCursorSound();
            _joystickIsPressed = true;
            _enemyIndex++;
        }
        else if (Input.GetAxisRaw("Vertical") == -1 && _enemyIndex > 0 && _joystickIsPressed == false)
        {
            _battleAudioManager.PlayCursorSound();
            _joystickIsPressed = true;
            _enemyIndex--;
        }
    }

    private void SelectMenu() // Autorise le joueur � faire ses choix d'actions dans le menu pour chaque personnage
    {
        if (_charactersList[_sortedCharactersIndex].CharactersData is EnemyData)
        {
            _sortedCharactersIndex++; // Si le personnage actuel est un ennemi, on passe au personnage suivant
            _defend.Add(false); // L'ennemi ne defend pas donc on passe la valeur � false pour chaque ennemi
        }
        else
        {
            Vector2 _characterPosition = _charactersList[_sortedCharactersIndex].gameObject.transform.position;
            MenuAndCursorDisplayManager(_playerCursor, _characterPosition, true); // Affiche le menu et le curseur au dessus de la t�te du personnage actif
            
            if (_attackButtonPressed) // Le joueur a selectionn� "Attaque"
            {
                SelectEnemyToAttack(); // Permet au joueur de s�lectionner l'ennemi � attaquer
            }
            else if (_defendButtonPressed) // Le joueur a selectionn� "D�fendre"
            {
                _charactersList[_sortedCharactersIndex].gameObject.GetComponent<Animator>().SetBool("isDefend", true);

                _defend.Add(true); // Indique que le personnage est en d�fense en passant la valeur � true
                _heroesAttackOrder.Add(_sortedCharactersIndex); // R�cup�re l'index du h�ros actuel dans la liste de personnages et le stock dans une liste
                _defendButtonPressed = false; // Passe � true si le joueur s�lectionne "d�fendre", on le repasse donc � false directement pour check le prochain appui
                _battleAudioManager.PlayConfirmButtonSound(); // Bruit du bouton annuler
                _playerCursor.gameObject.SetActive(false); // On d�sactive le curseur de selection du personnage en cours
                _sortedCharactersIndex++; // On passe au personnage suivant
            }

            if (Input.GetButtonDown("Cancel"))
            {
                CancelButtonPressed(); // Annule la derni�re action
            }
        }
    } 

    private void CancelButtonPressed()
    {
        _battleAudioManager.PlayCancelSound();

        if (_attackButtonPressed) // Si le bouton d'attaque a d�j� �t� enfonc�
        {
            _attackButtonReleased = false;
            _attackButtonPressed = false;
            _enemyCursor.gameObject.SetActive(false); // On d�sactive le curseur de selection de l'ennemi
        }
        else if (_charactersList[_sortedCharactersIndex].gameObject != _heroesList[0].gameObject) // Si le personnage actif n'est pas le h�ros avec l'agilit� la plus forte
        {
            _sortedCharactersIndex = _heroesAttackOrder[_heroesAttackOrder.Count - 1]; // Alors on revient au h�ros pr�c�dent
            _heroesAttackOrder.RemoveAt(_heroesAttackOrder.Count - 1); 

            if (!_defend[_sortedCharactersIndex])
            {
                _applyDamageToCharacter.RemoveAt(_applyDamageToCharacter.Count - 1);
            }

            for (int i = 0; i < _defend.Count; i++)
            {
                if (i >= _sortedCharactersIndex)
                {
                    _defend.RemoveRange(i, _defend.Count - i);
                }
            }
        }
    }

    private void SortCharacters() // On trie les personnages en fonction de leur agilit� (de la plus forte � la plus faible)
    {
        _charactersList.Sort((charA, charB) => charA.CharactersData.agility > charB.CharactersData.agility ? -1 : 1); 

        foreach (var item in _charactersList)
        {
            if (item.CharactersData is HeroData)
            {
                _heroesList.Add(item.GetComponent<PlayerHealth>());
            }
        }

    }
}
