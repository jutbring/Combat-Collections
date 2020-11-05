using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Health;

public class TurnBasedManager : MonoBehaviour
{
    public static string enemyParentName = "enemyParent";
    public static string allyParentName = "allyParent";
    public enum Actors { Player, Enemy }
    Actors currentActor = Actors.Player;
    public enum States { ActionSelection, EnemySelection, Aftermath }
    States currentState = States.ActionSelection;

    [Header("Entity Selection")]
    [SerializeField] KeyCode[] ForwardScrollingKeys = null;
    [SerializeField] KeyCode[] BackwardScrollingKeys = null;
    [SerializeField] GameObject entitySelectionCursor = null;
    [SerializeField] Vector3 entitySelectionCursorOffset = new Vector3(0, 0, 0);
    [SerializeField] float entitySelectionCursorMoveSpeed = 1f;
    [SerializeField] Animator entitySelectionCursorAnimator = null;
    int currentSelectedEnemy = 0;
    [SerializeField] float timeScale = 1f;
    GameObject allyParent = null;
    GameObject enemyParent = null;
    List<EntityHealth> enemies = new List<EntityHealth>();
    EntityHealth player;
    Animator animator = null;
    EntityHealth attacker;

    void Start()
    {
        Time.timeScale = timeScale;
        entitySelectionCursor = Instantiate(entitySelectionCursor, transform);
        entitySelectionCursorAnimator = entitySelectionCursor.GetComponent<Animator>();
        CreateParents(enemyParentName);
        CreateParents(allyParentName);
        animator = GetComponent<Animator>();
    }
    void CreateParents(string parentName)
    {
        var parent = new GameObject();
        parent.transform.position = new Vector3(0, 0, 0);
        parent.transform.parent = null;
        parent.name = parentName;
        if (parentName == allyParentName)
        {
            allyParent = parent;
        }
        else
        {
            enemyParent = parent;
        }
    }
    private void Update()
    {
        GetEntityList();
        UpdateEntityList();
        UpdateCursor();
        if (player)
        {
            Confirm();
        }
    }
    void GetEntityList()
    {
        if (enemies.Count == 0)
        {
            enemies = new List<EntityHealth>();
            for (int i = 0; i < enemyParent.transform.childCount; i++)
            {
                enemies.Add(enemyParent.transform.GetChild(i).GetComponent<EntityHealth>());
            }
        }
        if (allyParent.transform.childCount > 0)
        {
            player = allyParent.transform.GetChild(0).GetComponent<EntityHealth>();
        }
    }
    void UpdateEntityList()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
            }
        }
        if (currentSelectedEnemy >= enemies.Count)
        {
            currentSelectedEnemy = Mathf.Max(enemies.Count - 1, 0);
        }
    }
    void UpdateCursor()
    {
        bool playerSelection = currentActor == Actors.Player && currentState == States.EnemySelection;
        bool enemiesExist = enemies.Count > 0;
        entitySelectionCursor.SetActive(playerSelection && enemiesExist);
        if (enemiesExist)
        {
            entitySelectionCursor.transform.position = Vector3.Lerp(entitySelectionCursor.transform.position,
                enemies[currentSelectedEnemy].transform.position + entitySelectionCursorOffset,
                entitySelectionCursorMoveSpeed * Time.deltaTime * 100);
            if (playerSelection)
            {
                for (int i = 0; i < ForwardScrollingKeys.Length; i++)
                {
                    if (Input.GetKeyDown(ForwardScrollingKeys[i]))
                    {
                        UpdateSelectedEnemy(1);
                    }
                }
                for (int i = 0; i < BackwardScrollingKeys.Length; i++)
                {
                    if (Input.GetKeyDown(BackwardScrollingKeys[i]))
                    {
                        UpdateSelectedEnemy(-1);
                    }
                }
            }
        }
    }
    void Confirm()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            switch (currentState)
            {
                case States.ActionSelection:
                    currentState = States.EnemySelection;
                    SelectAction();
                    break;
                case States.EnemySelection:
                    currentState = States.Aftermath;
                    Attack(currentActor);
                    break;
                case States.Aftermath:
                    switch (currentActor)
                    {
                        case Actors.Player:
                            currentActor = Actors.Enemy;
                            currentState = States.ActionSelection;
                            break;
                        case Actors.Enemy:
                            currentActor = Actors.Player;
                            currentState = States.ActionSelection;
                            break;
                    }
                    break;
            }
        }
    }
    void UpdateSelectedEnemy(int jump)
    {
        if (enemies.Count > 1)
        {
            if (enemies.Count <= 1)
            {
                return;
            }
            currentSelectedEnemy += jump;
            if (currentSelectedEnemy < 0)
            {
                currentSelectedEnemy = enemies.Count - 1;
            }
            if (currentSelectedEnemy == enemies.Count)
            {
                currentSelectedEnemy = 0;
            }
            entitySelectionCursorAnimator.SetTrigger("Move");
        }
    }
    public void SelectAction()
    {
        switch (currentActor)
        {
            case Actors.Player:

                break;
            case Actors.Enemy:

                break;
        }
    }
    public void Attack(Actors actor)
    {
        switch (actor)
        {
            case Actors.Player:
                animator.SetTrigger("EnemyDamage");
                enemies[currentSelectedEnemy].DealDamage(player.damage);
                break;
            case Actors.Enemy:
                animator.SetTrigger("PlayerDamage");
                if (enemies.Count > 0)
                {
                    attacker = enemies[Random.Range(0, enemies.Count)];
                    player.DealDamage(attacker.damage);
                }
                break;
        }
    }
}
