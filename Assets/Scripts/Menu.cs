using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public enum MenuTypes { None, GameInventory, MapInventory, Victory, Defeat, ResetConfirm }
    [SerializeField] MenuTypes type = MenuTypes.None;
    [SerializeField] LevelSetup levelRestarter = null;
    [SerializeField] Button firstSelected = null;
    [SerializeField] Animator animator = null;
    public BattleSystem instantiator = null;
    [SerializeField] RectTransform panel = null;
    public ItemHolder itemInstantiator = null;

    [Header("Inventory")]
    [SerializeField] Vector3 inventoryPositionOffset = new Vector3(0, 0, 0);
    [SerializeField] float itemDistance = 1f;
    [SerializeField] int columnLength = 1;
    [SerializeField] Inventory playerInventory = null;
    List<SpriteRenderer> itemSprites = new List<SpriteRenderer>();
    List<Animator> itemAnimators = new List<Animator>();
    [SerializeField] GameObject slotPrefab = null;
    [SerializeField] Vector3 itemMoveSpeed = Vector3.one;
    [SerializeField] GameObject EquipButton = null;
    [SerializeField] GameObject UnequipButton = null;
    [SerializeField] GameObject RemoveButton = null;

    BattleSystem battleSystem = null;
    MapSystem1 mapSystem = null;
    int heldItemIndex = -1;
    int selectedItemIndex = -1;
    float timeSinceHold = 0f;
    bool isInventory = false;
    private void Start()
    {
        if (firstSelected)
        {
            firstSelected.Select();
        }
        isInventory = type.ToString().Contains("Inventory");
        CreateSlots();
    }
    void CreateSlots()
    {
        if (isInventory)
        {
            foreach (Animator animator in panel.GetComponentsInChildren<Animator>())
            {
                Destroy(animator.gameObject);
            }
            itemSprites.Clear();
            itemAnimators.Clear();
            for (int i = 0; i < playerInventory.maxItems; i++)
            {
                if (playerInventory.items.Count > i)
                {
                    if (playerInventory.items[i] == null)
                    {
                        playerInventory.items.RemoveAt(i);
                    }
                }
                var slot = Instantiate(slotPrefab, transform);
                slot.transform.parent = panel;
                slot.transform.localPosition = GetSlotPosition(i);
                slot.name = "Slot " + (i + 1).ToString();
                itemAnimators.Add(slot.GetComponent<Animator>());
                itemSprites.Add(slot.GetComponentInChildren<SpriteRenderer>());
                if (i < playerInventory.items.Count)
                {
                    itemSprites[i].sprite = playerInventory.items[i].itemSprite;
                    itemSprites[i].transform.localPosition = GameSettings.GetSpriteOffset(itemSprites[i].sprite);
                }
            }
        }
    }
    Vector3 GetSlotPosition(int index)
    {
        float column = (index % columnLength);
        int row = -Mathf.RoundToInt(index / columnLength);
        Vector3 spriteOffset = new Vector3(0, 0);
        return inventoryPositionOffset + new Vector3(column * itemDistance, row * itemDistance, 0) + spriteOffset;
    }
    private void Update()
    {
        GetInputs();
        UpdateInventory();
        UpdateHeldItem();
        UpdateButtons();
    }
    void GetInputs()
    {
        if (type == MenuTypes.MapInventory || type == MenuTypes.ResetConfirm)
        {
            for (int i = 0; i < GameSettings.inventoryKeys.Count; i++)
            {
                if (Input.GetKeyDown(GameSettings.inventoryKeys[i]))
                {
                    animator.SetTrigger("Close");
                }
            }
        }
    }
    void UpdateInventory()
    {
        if (isInventory)
        {
            for (int i = 0; i < playerInventory.items.Count; i++)
            {
                if (i != heldItemIndex)
                    itemSprites[i].transform.localPosition = Vector3.Lerp(itemSprites[i].transform.localPosition, GameSettings.GetSpriteOffset(itemSprites[i].sprite), itemMoveSpeed.x * Time.deltaTime * 100);
                itemSprites[i].sortingOrder = 200 + i;
                bool itemInRange = false;
                if (heldItemIndex > -1)
                    itemInRange = Vector2.Distance(itemSprites[heldItemIndex].transform.position, itemSprites[i].transform.position) < GameSettings.itemMouseReach
                        && itemSprites[heldItemIndex].sprite == itemSprites[i].sprite
                        && playerInventory.items[heldItemIndex].upgradeItem != null
                        && heldItemIndex != i;
                bool mouseInRange = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), itemAnimators[i].transform.position) < GameSettings.itemMouseReach;
                bool clicked = false;
                bool clickedDown = false;
                bool clickedUp = false;
                for (int j = 0; j < GameSettings.confirmKeys.Count; j++)
                {
                    clicked = (Input.GetKey(GameSettings.confirmKeys[j]) && mouseInRange) || clicked;
                    clickedDown = (Input.GetKeyDown(GameSettings.confirmKeys[j]) && mouseInRange) || clickedDown;
                    clickedUp = (Input.GetKeyUp(GameSettings.confirmKeys[j])) || clickedUp;
                }
                bool isEquipped = false;
                Item comparedSword = playerInventory.GetEquippedItem(Item.itemTypes.Sword);
                if (comparedSword)
                {
                    isEquipped = itemSprites[i].sprite == comparedSword.itemSprite;
                }
                Item comparedHelmet = playerInventory.GetEquippedItem(Item.itemTypes.Helmet);
                if (comparedHelmet)
                {
                    isEquipped = itemSprites[i].sprite == comparedHelmet.itemSprite || isEquipped;
                }
                switch (type)
                {
                    case MenuTypes.GameInventory:
                        if (clickedUp && mouseInRange)
                        {
                            ReplaceItem(i);
                        }
                        break;
                    case MenuTypes.MapInventory:
                        if (clickedDown)
                        {
                            heldItemIndex = i;
                            selectedItemIndex = i;
                        }
                        if (clickedUp)
                        {
                            if (mouseInRange)
                            {
                                if (itemInRange)
                                {
                                    MergeItems(i);
                                }
                                else if (heldItemIndex > -1)
                                {
                                    selectedItemIndex = heldItemIndex;
                                }
                            }
                            if (i == playerInventory.items.Count - 1)
                            {
                                heldItemIndex = -1;
                            }
                        }
                        itemAnimators[i].SetBool("Equipped", isEquipped);
                        itemAnimators[i].SetBool("Mergable", itemInRange);
                        break;
                    default:
                        break;
                }
                itemAnimators[i].SetBool("Highlighted", mouseInRange && heldItemIndex < 0);
                itemAnimators[i].SetBool("Clicked", clicked || selectedItemIndex == i);
                itemAnimators[i].SetBool("Gone", itemSprites[i].sprite == null);
            }
        }
    }
    void ReplaceItem(int index)
    {
        animator.SetTrigger("Close");
        itemInstantiator.itemToReplace = index;
        itemInstantiator.AllowPickup();
    }
    void MergeItems(int otherItemIndex)
    {
        if (playerInventory.items[heldItemIndex].upgradeItem == null) return;
        FindObjectOfType<MapSystem1>().PlayImpactEffect(2, 2);
        playerInventory.items[otherItemIndex] = playerInventory.items[otherItemIndex].upgradeItem;
        playerInventory.items.RemoveAt(heldItemIndex);
        heldItemIndex = -1;
        selectedItemIndex = -1;
        CreateSlots();
    }
    void UpdateHeldItem()
    {
        if (isInventory)
        {
            if (heldItemIndex > -1)
            {
                itemSprites[heldItemIndex].sortingOrder = 200 + playerInventory.items.Count + 1;
                timeSinceHold = Mathf.Pow(Mathf.Clamp(timeSinceHold + itemMoveSpeed.y, 0, itemMoveSpeed.y), itemMoveSpeed.z);
                Vector3 desiredPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + GameSettings.GetSpriteOffset(itemSprites[heldItemIndex].sprite);
                desiredPosition.z = -1;
                itemSprites[heldItemIndex].transform.position = Vector3.Lerp(itemSprites[heldItemIndex].transform.position, desiredPosition, timeSinceHold * Time.deltaTime * 100);
            }
            else
            {
                timeSinceHold = 0;
            }
        }
    }
    void UpdateButtons()
    {
        if (type == MenuTypes.MapInventory)
        {
            bool buttonsVisible = selectedItemIndex > -1;
            RemoveButton.SetActive(buttonsVisible);
            bool equipped = false;
            if (buttonsVisible)
            {
                switch (playerInventory.items[selectedItemIndex].itemType)
                {
                    case Item.itemTypes.Sword:
                        Item comparedSword = playerInventory.GetEquippedItem(Item.itemTypes.Sword);
                        if (comparedSword)
                        {
                            equipped = itemSprites[selectedItemIndex].sprite == comparedSword.itemSprite;
                        }
                        break;
                    case Item.itemTypes.Helmet:
                        Item comparedHelmet = playerInventory.GetEquippedItem(Item.itemTypes.Helmet);
                        if (comparedHelmet)
                        {
                            equipped = itemSprites[selectedItemIndex].sprite == comparedHelmet.itemSprite;
                        }
                        break;
                    default: break;
                }
            }
            UnequipButton.SetActive(buttonsVisible && equipped);
            EquipButton.SetActive(buttonsVisible && !equipped);
        }
    }
    public void EquipItem()
    {
        if (selectedItemIndex < 0) return;
        FindObjectOfType<MapSystem1>().PlayImpactEffect(1, 1);
        switch (playerInventory.items[selectedItemIndex].itemType)
        {
            case Item.itemTypes.Helmet:
                playerInventory.EquipItem(playerInventory.items[selectedItemIndex]);
                break;
            case Item.itemTypes.Sword:
                playerInventory.EquipItem(playerInventory.items[selectedItemIndex]);
                break;
            default:
                break;
        }
        selectedItemIndex = -1;
    }
    public void UnquipItem()
    {
        if (selectedItemIndex < 0) return;
        FindObjectOfType<MapSystem1>().PlayImpactEffect(0, 0.5f);
        switch (playerInventory.items[selectedItemIndex].itemType)
        {
            case Item.itemTypes.Helmet:
                playerInventory.UnquipItem(Item.itemTypes.Helmet);
                break;
            case Item.itemTypes.Sword:
                playerInventory.UnquipItem(Item.itemTypes.Sword);
                break;
            default:
                break;
        }
        selectedItemIndex = -1;
    }
    public void RemoveItem()
    {
        if (selectedItemIndex < 0) return;
        FindObjectOfType<MapSystem1>().PlayImpactEffect(0, 0.5f);
        playerInventory.items.RemoveAt(selectedItemIndex);
        selectedItemIndex = -1;
        playerInventory.lastItemCount = playerInventory.items.Count;
        CreateSlots();
    }
    public void LoadBattle()
    {
        if (GameSettings.isFading)
            return;
        levelRestarter = Instantiate(levelRestarter.gameObject, transform).GetComponent<LevelSetup>();
        levelRestarter.level = instantiator.levelStats;

    }
    public void LoadMap()
    {
        if (GameSettings.isFading)
            return;
        Time.timeScale = GameSettings.defaultTimeScale;
        FindObjectOfType<Fade>().FadeIn();
        StartCoroutine(Load((int)GameSettings.Scenes.Map));
    }
    public void ItemReplaceCancel()
    {
        itemInstantiator.CancelPickup();
        Close();
    }
    public void ResumeGame()
    {
        instantiator.TogglePause();
        Close();
    }
    public void Close()
    {
        animator.SetTrigger("Close");
    }
    public void ResetGame()
    {
        mapSystem = GameObject.FindWithTag("GameController").GetComponent<MapSystem1>();
        if (mapSystem)
        {
            mapSystem.ResetGame();
        }
        Close();
    }
    IEnumerator Load(int index)
    {
        yield return new WaitForSeconds(GameSettings.fadeInTime);
        SceneManager.LoadScene(index);
    }
}
