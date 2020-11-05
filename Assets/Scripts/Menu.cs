using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public enum MenuTypes { None, GameInventory, MapInventory, Victory, Defeat }
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

    BattleSystem battleSystem = null;
    MapSystem mapSystem = null;

    private void Start()
    {
        if (firstSelected)
        {
            firstSelected.Select();
        }
        CreateSlots();
    }
    void CreateSlots()
    {
        if (type.ToString().Contains("Inventory"))
        {
            itemSprites.Clear();
            itemAnimators.Clear();
            for (int i = 0; i < playerInventory.maxItems; i++)
            {
                var slot = Instantiate(slotPrefab, transform);
                slot.transform.parent = panel;
                slot.transform.localPosition = GetSlotPosition(i);
                slot.name = "Slot " + (i + 1).ToString();
                itemAnimators.Add(slot.GetComponent<Animator>());
                itemSprites.Add(slot.GetComponentInChildren<SpriteRenderer>());

                if (i < playerInventory.items.Count)
                {
                    itemSprites[i].sprite = playerInventory.items[i].itemSprite;
                    float itemSize = itemSprites[i].sprite.pivot.x + itemSprites[i].sprite.pivot.y;
                    itemSprites[i].transform.localPosition = new Vector2(0, -(itemSprites[i].sprite.pivot.x - itemSprites[i].sprite.pivot.y) / itemSize);
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
    }
    void GetInputs()
    {
        if (type == MenuTypes.MapInventory)
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
        if (type.ToString().Contains("Inventory"))
        {
            for (int i = 0; i < playerInventory.items.Count; i++)
            {
                bool mouseInRange = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), itemAnimators[i].transform.position) < GameSettings.itemMouseReach;
                bool clicked = false;
                bool clickedDown = false;
                bool clickedUp = false;
                itemAnimators[i].SetBool("Highlighted", mouseInRange);
                for (int j = 0; j < GameSettings.confirmKeys.Count; j++)
                {
                    clicked = (Input.GetKey(GameSettings.confirmKeys[j]) && mouseInRange) || clicked;
                    clickedDown = (Input.GetKeyDown(GameSettings.confirmKeys[j]) && mouseInRange) || clickedDown;
                    clickedUp = (Input.GetKeyUp(GameSettings.confirmKeys[j]) && mouseInRange) || clickedUp;
                    itemAnimators[i].SetBool("Clicked", clicked);
                }
                if (clickedUp)
                {
                    switch (type)
                    {
                        case MenuTypes.GameInventory:
                            ReplaceItem(i);
                            break;
                        case MenuTypes.MapInventory:
                            BeginMerge();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    void ReplaceItem(int index)
    {
        animator.SetTrigger("Close");
        itemInstantiator.itemToReplace = index;
        itemInstantiator.AllowPickup();
    }
    void BeginMerge()
    {

    }

    public void RestartBattle()
    {
        if (GameSettings.isFading)
            return;
        // NOT WORKING; FIX IMMEDIATELY
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        levelRestarter = Instantiate(levelRestarter.gameObject, transform).GetComponent<LevelSetup>();
        levelRestarter.level = instantiator.levelStats;

    }
    public void LoadMap()
    {
        if (GameSettings.isFading)
            return;
        battleSystem = GameObject.FindWithTag("GameController").GetComponent<BattleSystem>();
        if (battleSystem)
        {
            battleSystem.fade.FadeIn();
        }
        mapSystem = GameObject.FindWithTag("GameController").GetComponent<MapSystem>();
        if (mapSystem)
        {
            mapSystem.fade.FadeIn();
        }
        StartCoroutine(Load((int)GameSettings.Scenes.Map));
    }
    public void ItemReplaceCancel()
    {
        itemInstantiator.CancelPickup();
        animator.SetTrigger("Close");
    }
    IEnumerator Load(int index)
    {
        yield return new WaitForSeconds(GameSettings.fadeInTime);
        SceneManager.LoadScene(index);
    }
}
