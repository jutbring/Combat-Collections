using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public Item item = null;
    public BattleSystem battleSystem = null;
    public Animator animator = null;
    [SerializeField] SpriteRenderer spriteRenderer = null;
    [SerializeField] AnimationCurve moveSpeed = null;
    [SerializeField] AnimationCurve heightOffet = null;
    [SerializeField] AnimationCurve rotation = null;
    [SerializeField] float animationTime = 2f;
    [SerializeField] Inventory targetInventory = null;
    [SerializeField] ParticleSystem pickupEffect = null;
    [SerializeField] float timeUntilSpeechBubble = 1f;
    [SerializeField] bool seekPlayer = true;
    [SerializeField] Menu inventoryMenu = null;
    [SerializeField] SpeechBubble speechBubblePrefab = null;

    Transform player = null;
    Vector2 startPosition = new Vector2(0, 0);
    float startMoveTime = 0f;
    float startPickupTime = 0f;
    bool clicked = false;
    bool clickable = false;
    Menu inventory = null;
    GameObject speechBubble = null;
    public int itemToReplace = -1;
    void Start()
    {
        SetValues();
    }
    void SetValues()
    {
        itemToReplace = -1;
        if (!battleSystem)
        {
            battleSystem = GameObject.FindWithTag("GameController").GetComponent<BattleSystem>();
        }
        startPosition = transform.position;
        float itemSize = item.itemSprite.pivot.x + item.itemSprite.pivot.y;
        spriteRenderer.transform.localPosition = new Vector2(0, -(item.itemSprite.pivot.x - item.itemSprite.pivot.y) / itemSize);
        spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0/* -45 + (item.itemSprite.pivot.y / item.itemSprite.rect.size.y * 90)*/);

        spriteRenderer.sprite = item.itemSprite;
        player = GameObject.FindWithTag("Player").transform;
    }
    private void Update()
    {
        GetInputs();
        MoveItem();
        UpdateTime();
    }
    void GetInputs()
    {
        bool mouseInRange = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position) < GameSettings.itemMouseReach;
        animator.SetBool("Outlined", mouseInRange && clickable);
        bool mouseInput = false;
        bool otherInput = false;
        for (int i = 0; i < GameSettings.confirmKeys.Count; i++)
        {
            if (GameSettings.confirmKeys[i].ToString().Contains("Mouse"))
            {
                mouseInput = mouseInput || mouseInRange && Input.GetKeyDown(GameSettings.confirmKeys[i]);
            }
            else
            {
                otherInput = otherInput || Input.GetKeyDown(GameSettings.confirmKeys[i]);
            }
        }
        if ((mouseInput || otherInput) && !clicked && clickable && !inventory)
        {
            AllowPickup();
        }
    }
    void MoveItem()
    {
        animator.SetBool("Idle", !clicked);
        if (clicked)
        {
            bool inventoryFull = targetInventory.items.Count >= targetInventory.maxItems && itemToReplace < 0;
            if (inventoryFull)
            {
                InstantiateMessage("Inventory full!");
                inventory = Instantiate(inventoryMenu.gameObject, battleSystem.transform).GetComponent<Menu>();
                inventory.itemInstantiator = this;
                clicked = false;
                return;
            }
            if (seekPlayer)
            {
                float time = (Time.time - startMoveTime) / animationTime;
                if (time <= 1)
                {
                    transform.position = Vector2.Lerp(startPosition, player.transform.position + new Vector3(0, 0.5f + heightOffet.Evaluate(time), 0), moveSpeed.Evaluate(time));
                    transform.rotation = Quaternion.Euler(0, 0, rotation.Evaluate(time));
                }
                else
                {
                    StartPickup();
                }
            }
            else
            {
                StartPickup();
            }
        }
    }
    public void CancelPickup()
    {
        animator.SetTrigger("Destroy");
        if (speechBubble != null)
            Destroy(speechBubble);
    }
    void StartPickup()
    {
        animator.SetTrigger("Pickup");
        clicked = false;
    }
    void UpdateTime()
    {
        if (startPickupTime != 0)
        {
            float passedTime = Time.time - startPickupTime;
            if (passedTime > timeUntilSpeechBubble && speechBubble == null && clickable && !clicked)
            {
                InstantiateMessage("Pick up!");
            }
        }
    }
    void InstantiateMessage(string messageText)
    {
        if (speechBubble != null)
            Destroy(speechBubble);
        var message = Instantiate(speechBubblePrefab.gameObject, transform);
        message.GetComponent<SpeechBubble>().message = messageText;
        speechBubble = message;
    }
    public void CanMove()
    {
        clickable = true;
        startPickupTime = Time.time;
    }
    public void PickupEffect()
    {
        pickupEffect.Play();
        pickupEffect.transform.parent = null;
        battleSystem.PlayImpactEffect(1, 1);
    }
    void PickupItem()
    {
        bool picked = false;
        if (itemToReplace > -1)
        {
            picked = targetInventory.ReplaceItem(item, itemToReplace);
        }
        else
        {
            picked = targetInventory.AddToList(item);
        }
        if (picked)
            Destroy(gameObject);
    }
    public void AllowPickup()
    {
        if (speechBubble != null)
        {
            Destroy(speechBubble);
        }
        startMoveTime = Time.time;
        clicked = true;
        clickable = false;
    }
}
