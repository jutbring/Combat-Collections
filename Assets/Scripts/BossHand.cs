using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossHand : MonoBehaviour
{
    public enum Phases { Stalk, Poke, Clap, Idle }
    Phases currentPhase = Phases.Idle;
    enum phaseStates { Charge, Action }
    phaseStates phaseState = phaseStates.Charge;
    [SerializeField] bool isRight;
    [SerializeField] SpriteRenderer sprite = null;
    [SerializeField] Unit head = null;
    [SerializeField] Phases startPhase = Phases.Idle;

    // private
    bool offsetPattern = false;

    [Header("Charge")]
    [SerializeField] Sprite chargeSprite;
    [SerializeField] AnimationCurve chargeMovement = null;
    [SerializeField] float rotationSpeed = 1f;

    [Header("Stalk")]
    [SerializeField] Sprite stalkSprite;
    [SerializeField] float stalkDuration = 5f;
    [SerializeField] float stalkSpeed = 1f;

    [Header("Poke")]
    [SerializeField] Sprite pokeSprite;
    [SerializeField] int pokeAmount = 3;
    [SerializeField] float pokeChargeTime = 1f;
    [SerializeField] float pokeTime = 1f;
    [SerializeField] float pokeDistance = 1f;
    [SerializeField] AnimationCurve pokeMovement = null;
    Vector3 hitPosition = new Vector3(0, 0, 0);

    [Header("Clap")]
    [SerializeField] Sprite clapSprite;
    [SerializeField] int clapAmount = 3;
    [SerializeField] float clapChargeTime = 1f;
    [SerializeField] float clapTime = 1f;
    [SerializeField] Vector2 clapOffset = Vector2.zero;
    [SerializeField] AnimationCurve ClapMovementX = null;
    [SerializeField] AnimationCurve ClapMovementY = null;

    Unit player = null;
    BossHand otherHand = null;
    float elapsed = 0f;
    bool canAttack = false;
    void Start()
    {
        currentPhase = startPhase;
        player = GameObject.FindWithTag("Player").GetComponent<Unit>();
        foreach (GameObject hand in GameObject.FindGameObjectsWithTag("BossHand"))
        {
            if (hand != gameObject)
                otherHand = hand.GetComponent<BossHand>();
        }
        if (sprite)
        {
            Vector2 startsize = sprite.transform.localScale;
            sprite.transform.localScale = new Vector2(startsize.x - (startsize.x * 2 * Convert.ToInt32(!isRight)), startsize.y);
        }
        DetermineNextPhase();
    }
    public void Update()
    {
        if (head.currentHealth != 0)
        {
            if (!player)
            {
                player = head;
            }
            PokeBehaviour();
            StalkBehaviour();
            ClapBehaviour();
        }
    }
    void DetermineNextPhase()
    {
        elapsed = 0;
        switch (currentPhase)
        {
            case Phases.Stalk:
                if (isRight && offsetPattern)
                    StartCoroutine(Clap());
                else
                    StartCoroutine(Clap());
                break;
            case Phases.Poke:
                if (!isRight && offsetPattern)
                    StartCoroutine(Clap());
                else
                    StartCoroutine(Stalk());
                break;
            default:
                if (isRight && offsetPattern)
                    StartCoroutine(Stalk());
                else
                    StartCoroutine(Poke());
                break;
        }
    }
    void StalkBehaviour()
    {
        if (currentPhase != Phases.Stalk) return;

        sprite.sprite = stalkSprite;
        transform.up = Vector3.Lerp(transform.up, player.transform.position - transform.position, rotationSpeed * Time.deltaTime * 100);
        float stalkSpeedScaled = stalkSpeed * Time.deltaTime;
        int direction = 1;
        if (!isRight)
            direction = 1;
        else
            direction = -1;
        Vector3 desiredPosition = player.transform.position * direction + player.swordOffset;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, stalkSpeedScaled * Mathf.Clamp(Vector2.Distance(player.transform.position * direction, transform.position) / 2, 1, 10));
    }
    IEnumerator Stalk()
    {
        canAttack = true;
        currentPhase = Phases.Stalk;
        yield return new WaitForSeconds(stalkDuration);
        canAttack = false;
        DetermineNextPhase();
    }
    Vector3 pokePosition = Vector3.zero;
    Vector3 startDirection = Vector3.zero;
    Vector3 startPosition = Vector3.zero;
    Vector3 firstPosition = Vector3.zero;
    void PokeBehaviour()
    {
        if (currentPhase != Phases.Poke) return;

        if (firstPosition == Vector3.zero)
            firstPosition = transform.position;
        switch (phaseState)
        {
            case phaseStates.Charge:
                sprite.sprite = chargeSprite;
                if (startDirection == Vector3.zero)
                {
                    startDirection = (transform.position - player.transform.position).normalized;
                    float blendModule = UnityEngine.Random.Range(0f, 1f);
                    if (isRight)
                        startDirection = ((new Vector3(-startDirection.y, startDirection.x) * blendModule) + (startDirection * (1 - blendModule))).normalized;
                    else
                        startDirection = ((new Vector3(startDirection.y, -startDirection.x) * blendModule) + (startDirection * (1 - blendModule))).normalized;
                    startDirection *= pokeDistance / 2;
                }
                transform.right = Vector3.Lerp(transform.right, (player.transform.position - transform.position) * (1 + -2 * Convert.ToInt32(!isRight)), rotationSpeed * Time.deltaTime * 100);
                startPosition = player.transform.position + player.swordOffset + startDirection;
                transform.position = Vector3.Lerp(firstPosition, startPosition, chargeMovement.Evaluate(elapsed / pokeChargeTime));
                break;
            case phaseStates.Action:
                sprite.sprite = pokeSprite;
                if (pokePosition == Vector3.zero)
                    pokePosition = player.transform.position + player.swordOffset + (transform.position - player.transform.position).normalized * pokeDistance / -2;
                transform.position = Vector3.Lerp(startPosition, pokePosition, pokeMovement.Evaluate(elapsed / pokeTime));
                break;
            default:
                phaseState = phaseStates.Charge;
                break;
        }
        elapsed += Time.deltaTime;
    }
    IEnumerator Poke()
    {
        currentPhase = Phases.Poke;
        for (int i = 0; i < pokeAmount; i++)
        {
            firstPosition = Vector3.zero;
            startPosition = Vector3.zero;
            pokePosition = Vector3.zero;
            startDirection = Vector3.zero;
            elapsed = 0;
            phaseState = phaseStates.Charge;
            yield return new WaitForSeconds(pokeChargeTime);
            elapsed = 0;
            canAttack = true;
            phaseState = phaseStates.Action;
            yield return new WaitForSeconds(pokeTime);
            canAttack = false;
        }
        DetermineNextPhase();
    }
    void ClapBehaviour()
    {
        if (currentPhase != Phases.Clap) return;

        if (firstPosition == Vector3.zero)
            firstPosition = transform.position;
        switch (phaseState)
        {
            case phaseStates.Charge:
                sprite.sprite = chargeSprite;
                transform.right = Vector3.Lerp(transform.right, (player.transform.position - transform.position) * (1 + -2 * Convert.ToInt32(!isRight)), rotationSpeed * Time.deltaTime * 100);
                startPosition = new Vector3(clapOffset.x - (clapOffset.x * 2 * Convert.ToInt32(isRight)), clapOffset.y);
                transform.position = Vector3.Lerp(firstPosition, startPosition, chargeMovement.Evaluate(elapsed / clapChargeTime));
                break;
            case phaseStates.Action:
                sprite.sprite = clapSprite;
                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime * 100);
                transform.up = Vector3.Lerp(transform.up, Vector3.up, rotationSpeed * Time.deltaTime * 100);
                if (pokePosition == Vector3.zero)
                    pokePosition = player.transform.position + player.swordOffset;
                transform.position = new Vector3(Mathf.Lerp(startPosition.x, player.transform.position.x, ClapMovementX.Evaluate(elapsed / clapTime)), Mathf.Lerp(startPosition.y, pokePosition.y, ClapMovementY.Evaluate(elapsed / clapTime)));
                break;
            default:
                phaseState = phaseStates.Charge;
                break;
        }
        elapsed += Time.deltaTime;
    }
    IEnumerator Clap()
    {
        currentPhase = Phases.Clap;
        for (int i = 0; i < clapAmount; i++)
        {
            firstPosition = Vector3.zero;
            startPosition = Vector3.zero;
            pokePosition = Vector3.zero;
            elapsed = 0;
            phaseState = phaseStates.Charge;
            yield return new WaitForSeconds(clapChargeTime);
            elapsed = 0;
            canAttack = true;
            phaseState = phaseStates.Action;
            yield return new WaitForSeconds(clapTime);
            canAttack = false;
        }
        DetermineNextPhase();
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (!player) return;
        if (collision.transform.parent.parent != player.transform || !canAttack || !head) return;
        head.Attack(player);
    }
}
