using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Backpack Panel")]
    public Transform backpackPanel;
    public Transform inventoryPanel;

    [Header("Weapon Sprites")]
    public Sprite basicSword;
    public Sprite rareSword;
    public Sprite epicSword;
    public Sprite legendarySword;
    public Sprite basicWand;
    public Sprite rareWand;
    public Sprite epicWand;
    public Sprite legendaryWand;
    public Sprite undeadSword;

    [Header("Helm Sprites")]
    public Sprite basicHelm;
    public Sprite rareHelm;
    public Sprite epicHelm;
    public Sprite legendaryHelm;

    [Header("Armor Sprites")]
    public Sprite basicArmor;
    public Sprite rareArmor;
    public Sprite epicArmor;
    public Sprite legendaryArmor;

    [Header("Ring Sprites")]
    public Sprite basicRing;
    public Sprite rareRing;
    public Sprite epicRing;
    public Sprite legendaryRing;

    [Header("Boots Sprites")]
    public Sprite basicBoots;
    public Sprite rareBoots;
    public Sprite epicBoots;
    public Sprite legendaryBoots;

    [Header("Consumables")]
    public Sprite potionSprite;
    public Sprite manaPotionSprite;

    [Header("Equipment Slots")]
    public Image weaponSlot;
    public Image helmSlot;
    public Image armorSlot;
    public Image ring1Slot;
    public Image ring2Slot;
    public Image bootsSlot;

    private Image[] inventorySlots;
    private Image[] slotBackgrounds;
    private int[] slotItemType; // 0=empty, 1=HP, 2=MP, 3-8=Basic, 9-14=Rare...
    private int selectedSlotIndex = -1;

    public int[] GetSlotItemTypes() => slotItemType;
    public void SetSlotData(int[] data) { slotItemType = data; RefreshInventory(); }
    public int GetSelectedSlotIndex() => selectedSlotIndex;
    public bool[] GetSlotData() { if (slotItemType == null) return new bool[0]; bool[] data = new bool[slotItemType.Length]; for (int i = 0; i < data.Length; i++) data[i] = slotItemType[i] > 0; return data; }
    public bool AddPotion() => AddItem(1);
    public bool AddManaPotion() => AddItem(2);

    private void Awake() { if (Instance != null && Instance != this) { Destroy(this); return; } Instance = this; if (transform.parent != null) transform.SetParent(null); DontDestroyOnLoad(gameObject); if (slotItemType == null || slotItemType.Length == 0) slotItemType = new int[40]; }
    private void OnEnable() { GameManager.OnSystemsReady += RefreshInventory; }
    private void OnDisable() { GameManager.OnSystemsReady -= RefreshInventory; }
    private void Start() { RefreshInventory(); }

    public void RefreshInventory() { ReconnectBackpackPanel(); InitializeInventorySlots(); RestoreInventoryVisuals(); UpdateSlotHighlights(); UpdateEquipmentVisuals(); }

    private void ReconnectBackpackPanel() {
        if (backpackPanel != null && backpackPanel.gameObject.scene.name == "DontDestroyOnLoad") return;
        GameObject target = (GameManager.Instance != null && GameManager.Instance.canvas != null) ? GameManager.Instance.canvas : null;
        if (target == null) { Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None); foreach (var c in canvases) if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") { target = c.gameObject; break; } }
        if (target == null) return;
        backpackPanel = FindChildRecursive(target.transform, "BackpackPanel");
        if (backpackPanel == null) backpackPanel = FindChildRecursive(target.transform, "Backpack");
        if (MyUIManager.Instance != null) inventoryPanel = MyUIManager.Instance.inventoryPanel.transform;
    }

    private Transform FindChildRecursive(Transform parent, string name) { foreach (Transform t in parent.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t; return null; }

    private void InitializeInventorySlots() {
        if (backpackPanel == null) return;
        
        System.Collections.Generic.List<Transform> slots = new System.Collections.Generic.List<Transform>();
        foreach(Transform child in backpackPanel) {
            if (child.name.Contains("Slot")) slots.Add(child);
        }

        int count = slots.Count;
        if (count == 0) return;

        inventorySlots = new Image[count];
        slotBackgrounds = new Image[count];
        
        if (slotItemType == null || slotItemType.Length == 0) slotItemType = new int[Mathf.Max(40, count)];
        else if (slotItemType.Length < count) { int[] nt = new int[count]; System.Array.Copy(slotItemType, nt, slotItemType.Length); slotItemType = nt; }

        for (int i = 0; i < count; i++) {
            Transform slot = slots[i]; 
            slotBackgrounds[i] = slot.GetComponent<Image>();
            if (slotBackgrounds[i] != null) { 
                slotBackgrounds[i].enabled = true; 
                slotBackgrounds[i].color = Color.black; 
                slotBackgrounds[i].sprite = null; // ALWAYS CLEAR PARENT SPRITE
                slotBackgrounds[i].raycastTarget = true; 
            }
            InventoryClickHandler h = slot.GetComponent<InventoryClickHandler>() ?? slot.gameObject.AddComponent<InventoryClickHandler>(); h.slotIndex = i;
            
            Transform itm = slot.Find("Item") ?? (slot.childCount > 0 ? slot.GetChild(0) : null);
            
            // AGGRESSIVELY CLEAR ALL OTHER IMAGES IN SLOT
            foreach (Transform child in slot) {
                if (child != itm) {
                    UnityEngine.UI.Image childImg = child.GetComponent<UnityEngine.UI.Image>();
                    if (childImg != null) {
                        childImg.sprite = null;
                        childImg.color = new Color(0,0,0,0);
                        childImg.enabled = false;
                    }
                }
            }

            if (itm != null) {
                inventorySlots[i] = itm.GetComponent<Image>() ?? itm.gameObject.AddComponent<Image>(); 
                inventorySlots[i].preserveAspect = true; 
                inventorySlots[i].raycastTarget = true;
                InventoryClickHandler ih = itm.GetComponent<InventoryClickHandler>() ?? itm.gameObject.AddComponent<InventoryClickHandler>(); ih.slotIndex = i;
                if (itm.GetComponent<CanvasGroup>() == null) itm.gameObject.AddComponent<CanvasGroup>();
                if (itm.GetComponent<InventoryDragDrop>() == null) itm.gameObject.AddComponent<InventoryDragDrop>();
            }
        }
    }

    public void SelectSlot(int idx) { if (idx < 0 || idx >= slotItemType.Length || slotItemType[idx] == 0) selectedSlotIndex = -1; else { selectedSlotIndex = idx; ShopManager shop = FindFirstObjectByType<ShopManager>(); if (shop != null) shop.DeselectShopItem(); } UpdateSlotHighlights(); }
    public void DeselectSlot() { selectedSlotIndex = -1; UpdateSlotHighlights(); }
    private void UpdateSlotHighlights() { if (slotBackgrounds == null) return; for (int i = 0; i < slotBackgrounds.Length; i++) if (slotBackgrounds[i] != null) slotBackgrounds[i].color = (i == selectedSlotIndex) ? new Color(0, 0.5f, 1, 0.8f) : Color.black; }
    public bool AddItem(int type) { if (slotItemType == null || slotItemType.Length == 0) slotItemType = new int[40]; for (int i = 0; i < slotItemType.Length; i++) if (slotItemType[i] == 0) { slotItemType[i] = type; RefreshInventory(); return true; } return false; }
    public void UseSelectedItem() { if (selectedSlotIndex == -1) return; int type = slotItemType[selectedSlotIndex]; if (type == 1) { PlayerStats.Instance?.Heal(50); RemoveSelected(); } else if (type == 2) { PlayerStats.Instance?.RestoreMana(30); RemoveSelected(); } else if (type >= 3 && type <= 30) EquipItem(type); }
    private void EquipItem(int type) {
        if (PlayerStats.Instance == null) return; int oldItem = 0;
        if (IsRing(type)) { if (PlayerStats.Instance.equippedRing1 == 0) PlayerStats.Instance.equippedRing1 = type; else if (PlayerStats.Instance.equippedRing2 == 0) PlayerStats.Instance.equippedRing2 = type; else { oldItem = PlayerStats.Instance.equippedRing1; PlayerStats.Instance.equippedRing1 = type; } }
        else if (IsWeapon(type)) { oldItem = PlayerStats.Instance.equippedWeapon; PlayerStats.Instance.equippedWeapon = type; }
        else if (IsHelm(type)) { oldItem = PlayerStats.Instance.equippedHelm; PlayerStats.Instance.equippedHelm = type; }
        else if (IsArmor(type)) { oldItem = PlayerStats.Instance.equippedArmor; PlayerStats.Instance.equippedArmor = type; }
        else if (IsBoots(type)) { oldItem = PlayerStats.Instance.equippedBoots; PlayerStats.Instance.equippedBoots = type; }
        slotItemType[selectedSlotIndex] = oldItem; selectedSlotIndex = -1; PlayerStats.Instance.RecalculateStats(); PlayerStats.Instance.UpdateUI(); RefreshInventory();
    }

    public bool IsWeapon(int id) => id == 3 || id == 8 || id == 9 || id == 14 || id == 15 || id == 21 || id == 26 || id == 27 || id == 28;
    public bool IsHelm(int id) => id == 4 || id == 10 || id == 16 || id == 22;
    public bool IsArmor(int id) => id == 5 || id == 11 || id == 17 || id == 23;
    public bool IsBoots(int id) => id == 7 || id == 13 || id == 19 || id == 25;
    public bool IsRing(int id) => id == 6 || id == 12 || id == 18 || id == 24;

    public void EquipFromBackpack(int backpackIdx, EqType targetType) {
        if (backpackIdx < 0 || backpackIdx >= slotItemType.Length) return; int itemId = slotItemType[backpackIdx]; if (itemId == 0) return;
        bool canEquip = false;
        if (targetType == EqType.Weapon && IsWeapon(itemId)) canEquip = true; else if (targetType == EqType.Helm && IsHelm(itemId)) canEquip = true; else if (targetType == EqType.Armor && IsArmor(itemId)) canEquip = true; else if ((targetType == EqType.Ring1 || targetType == EqType.Ring2) && IsRing(itemId)) canEquip = true; else if (targetType == EqType.Boots && IsBoots(itemId)) canEquip = true;
        if (canEquip) { selectedSlotIndex = backpackIdx; EquipToSpecificSlot(itemId, targetType); }
    }

    private void EquipToSpecificSlot(int itemId, EqType target) {
        if (PlayerStats.Instance == null) return; int oldItem = 0;
        switch(target) {
            case EqType.Weapon: oldItem = PlayerStats.Instance.equippedWeapon; PlayerStats.Instance.equippedWeapon = itemId; break;
            case EqType.Helm: oldItem = PlayerStats.Instance.equippedHelm; PlayerStats.Instance.equippedHelm = itemId; break;
            case EqType.Armor: oldItem = PlayerStats.Instance.equippedArmor; PlayerStats.Instance.equippedArmor = itemId; break;
            case EqType.Ring1: oldItem = PlayerStats.Instance.equippedRing1; PlayerStats.Instance.equippedRing1 = itemId; break;
            case EqType.Ring2: oldItem = PlayerStats.Instance.equippedRing2; PlayerStats.Instance.equippedRing2 = itemId; break;
            case EqType.Boots: oldItem = PlayerStats.Instance.equippedBoots; PlayerStats.Instance.equippedBoots = itemId; break;
        }
        slotItemType[selectedSlotIndex] = oldItem; selectedSlotIndex = -1; PlayerStats.Instance.RecalculateStats(); PlayerStats.Instance.UpdateUI(); RefreshInventory();
    }

    public void UnequipToSlot(EqType type, int backpackIdx) {
        if (PlayerStats.Instance == null) return; int id = GetEquippedId(type); if (id == 0) return;
        if (backpackIdx >= 0 && backpackIdx < slotItemType.Length && slotItemType[backpackIdx] == 0) { slotItemType[backpackIdx] = id; SetEquippedId(type, 0); PlayerStats.Instance.RecalculateStats(); PlayerStats.Instance.UpdateUI(); RefreshInventory(); }
    }

    public bool UnequipToFirstFree(EqType type) {
        if (PlayerStats.Instance == null) return false;
        int id = GetEquippedId(type);
        if (id == 0) return false;
        if (AddItem(id)) {
            SetEquippedId(type, 0);
            PlayerStats.Instance.RecalculateStats();
            PlayerStats.Instance.UpdateUI();
            RefreshInventory();
            return true;
        }
        return false;
    }

    public int GetEquippedId(EqType type) { if (PlayerStats.Instance == null) return 0; switch (type) { case EqType.Weapon: return PlayerStats.Instance.equippedWeapon; case EqType.Helm: return PlayerStats.Instance.equippedHelm; case EqType.Armor: return PlayerStats.Instance.equippedArmor; case EqType.Ring1: return PlayerStats.Instance.equippedRing1; case EqType.Ring2: return PlayerStats.Instance.equippedRing2; case EqType.Boots: return PlayerStats.Instance.equippedBoots; default: return 0; } }
    private void SetEquippedId(EqType type, int id) { if (PlayerStats.Instance == null) return; switch (type) { case EqType.Weapon: PlayerStats.Instance.equippedWeapon = id; break; case EqType.Helm: PlayerStats.Instance.equippedHelm = id; break; case EqType.Armor: PlayerStats.Instance.equippedArmor = id; break; case EqType.Ring1: PlayerStats.Instance.equippedRing1 = id; break; case EqType.Ring2: PlayerStats.Instance.equippedRing2 = id; break; case EqType.Boots: PlayerStats.Instance.equippedBoots = id; break; } }

    public void UpdateEquipmentVisuals() {
        if (PlayerStats.Instance == null) return; ReconnectEquipmentSlots();
        SetEquipmentIcon(weaponSlot, PlayerStats.Instance.equippedWeapon, EqType.Weapon); SetEquipmentIcon(helmSlot, PlayerStats.Instance.equippedHelm, EqType.Helm); SetEquipmentIcon(armorSlot, PlayerStats.Instance.equippedArmor, EqType.Armor); SetEquipmentIcon(ring1Slot, PlayerStats.Instance.equippedRing1, EqType.Ring1); SetEquipmentIcon(ring2Slot, PlayerStats.Instance.equippedRing2, EqType.Ring2); SetEquipmentIcon(bootsSlot, PlayerStats.Instance.equippedBoots, EqType.Boots);
    }

    private void ReconnectEquipmentSlots() {
        if (inventoryPanel == null) return; Transform eq = inventoryPanel.Find("Equipment");
        if (eq != null) {
            weaponSlot = eq.Find("WeaponSlot")?.GetComponent<Image>(); helmSlot = eq.Find("HelmSlot")?.GetComponent<Image>(); armorSlot = eq.Find("ArmorSlot")?.GetComponent<Image>(); ring1Slot = eq.Find("Ring1Slot")?.GetComponent<Image>(); ring2Slot = eq.Find("Ring2Slot")?.GetComponent<Image>(); bootsSlot = eq.Find("BootsSlot")?.GetComponent<Image>();
            SetupEqDrag(weaponSlot, EqType.Weapon); SetupEqDrag(helmSlot, EqType.Helm); SetupEqDrag(armorSlot, EqType.Armor); SetupEqDrag(ring1Slot, EqType.Ring1); SetupEqDrag(ring2Slot, EqType.Ring2); SetupEqDrag(bootsSlot, EqType.Boots);
        }
    }

    private void SetupEqDrag(Image slot, EqType type) {
        if (slot == null) return; Transform icn = slot.transform.Find("Icon"); GameObject target = icn != null ? icn.gameObject : slot.gameObject;
        if (target.GetComponent<InventoryDragDrop>() == null) target.AddComponent<InventoryDragDrop>(); if (target.GetComponent<CanvasGroup>() == null) target.AddComponent<CanvasGroup>();
        EquipmentSlot es = slot.GetComponent<EquipmentSlot>() ?? slot.gameObject.AddComponent<EquipmentSlot>(); es.slotType = type;
        GeneralTooltipTrigger tt = slot.GetComponent<GeneralTooltipTrigger>() ?? slot.gameObject.AddComponent<GeneralTooltipTrigger>(); tt.content = GetTooltipForId(GetEquippedId(type));
    }

    private void SetEquipmentIcon(Image slot, int itemId, EqType type) {
        if (slot == null) return; 
        
        // Find or Create Icon child
        Transform icnT = slot.transform.Find("Icon"); 
        UnityEngine.UI.Image icnI = null;
        float padding = (type == EqType.Ring1 || type == EqType.Ring2) ? 14f : 0f;
        
        if (icnT == null) { 
            GameObject go = new GameObject("Icon"); 
            go.transform.SetParent(slot.transform, false); 
            RectTransform rt = go.AddComponent<RectTransform>(); 
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; 
            rt.offsetMin = new Vector2(padding, padding); 
            rt.offsetMax = new Vector2(-padding, -padding); 
            icnI = go.AddComponent<UnityEngine.UI.Image>(); 
            icnI.preserveAspect = true; 
            icnI.raycastTarget = true;
            icnT = go.transform;
        }
        else {
            icnI = icnT.GetComponent<UnityEngine.UI.Image>();
            RectTransform rt = icnT.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        // AGGRESSIVELY CLEAR ALL GRAPHICS IN SLOT HIERARCHY
        UnityEngine.UI.Graphic[] allGraphics = slot.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var g in allGraphics) {
            if (g != icnI) {
                if (g is UnityEngine.UI.Image img) {
                    img.sprite = null;
                    if (g.gameObject == slot.gameObject) g.color = (itemId == 0) ? new Color(0,0,0,0) : Color.black;
                    else { g.color = new Color(0,0,0,0); g.enabled = false; }
                } else if (g is UnityEngine.UI.RawImage ri) {
                    ri.texture = null;
                    ri.color = new Color(0,0,0,0);
                    ri.enabled = false;
                }
            }
        }
        
        // Also check for SpriteRenderers just in case
        SpriteRenderer[] srs = slot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs) { sr.sprite = null; sr.enabled = false; }
        
        if (itemId == 0) { 
            icnI.sprite = null; 
            icnI.color = new Color(0,0,0,0); 
            icnI.enabled = false; 
            icnI.gameObject.SetActive(false);
        }
        else { 
            Sprite s = GetSpriteForId(itemId); 
            if (s != null) { 
                icnI.sprite = s; 
                icnI.color = Color.white; 
                icnI.enabled = true; 
                icnI.gameObject.SetActive(true);
            } else { 
                icnI.sprite = null; 
                icnI.color = new Color(0,0,0,0); 
                icnI.enabled = false; 
                icnI.gameObject.SetActive(false);
            } 
        }
        GeneralTooltipTrigger tt = slot.GetComponent<GeneralTooltipTrigger>(); 
        if (tt != null) tt.content = GetTooltipForId(itemId);
    }

    public Sprite GetSpriteForId(int id) {
        switch(id) {
            case 1: return potionSprite;
            case 2: return manaPotionSprite;
            case 3: return basicSword;
            case 4: return basicHelm;
            case 5: return basicArmor;
            case 6: return basicRing;
            case 7: return basicBoots;
            case 8: return basicWand;
            case 9: return rareSword;
            case 10: return rareHelm;
            case 11: return rareArmor;
            case 12: return rareRing;
            case 13: return rareBoots;
            case 14: return rareWand;
            case 15: return undeadSword;
            case 16: return epicHelm;
            case 17: return epicArmor;
            case 18: return epicRing;
            case 19: return epicBoots;
            case 21: return epicSword;
            case 22: return legendaryHelm;
            case 23: return legendaryArmor;
            case 24: return legendaryRing;
            case 25: return legendaryBoots;
            case 26: return epicWand;
            case 27: return legendarySword;
            case 28: return legendaryWand;
            default: return null;
        }
    }

    public void MoveItem(int f, int t) { if (f < 0 || f >= slotItemType.Length || t < 0 || t >= slotItemType.Length) return; int tmp = slotItemType[t]; slotItemType[t] = slotItemType[f]; slotItemType[f] = tmp; if (selectedSlotIndex == f) selectedSlotIndex = t; else if (selectedSlotIndex == t) selectedSlotIndex = f; RefreshInventory(); }
    public bool RemoveSelected() { if (selectedSlotIndex == -1 || slotItemType[selectedSlotIndex] == 0) return false; slotItemType[selectedSlotIndex] = 0; selectedSlotIndex = -1; RefreshInventory(); return true; }
    public bool RemoveOnePotion() { for (int i = slotItemType.Length - 1; i >= 0; i--) if (slotItemType[i] == 1) { slotItemType[i] = 0; RefreshInventory(); return true; } return false; }
    public void RemovePotion(Image usedSlot) { if (usedSlot == null || inventorySlots == null) return; for (int i = 0; i < inventorySlots.Length; i++) if (inventorySlots[i] == usedSlot) { slotItemType[i] = 0; if (selectedSlotIndex == i) selectedSlotIndex = -1; RefreshInventory(); return; } }
    private void RestoreInventoryVisuals() { if (inventorySlots == null || inventorySlots.Length == 0) { ReconnectBackpackPanel(); InitializeInventorySlots(); } RestoreVisualsInternal(); }
    private void RestoreVisualsInternal() {
        if (inventorySlots == null || slotItemType == null) return;
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i] == null) continue; int type = (i < slotItemType.Length) ? slotItemType[i] : 0;
            GeneralTooltipTrigger tt = inventorySlots[i].GetComponent<GeneralTooltipTrigger>() ?? inventorySlots[i].gameObject.AddComponent<GeneralTooltipTrigger>();
            InventoryDragDrop d = inventorySlots[i].GetComponent<InventoryDragDrop>() ?? inventorySlots[i].gameObject.AddComponent<InventoryDragDrop>();
            if (type != 0) { 
                inventorySlots[i].sprite = GetSpriteForId(type); 
                inventorySlots[i].color = inventorySlots[i].sprite != null ? Color.white : Color.black; 
                inventorySlots[i].gameObject.SetActive(true); 
                tt.content = GetTooltipForId(type); 
            }
            else { inventorySlots[i].sprite = null; inventorySlots[i].color = Color.black; tt.content = ""; }
        }
    }

    public string GetTooltipForId(int id) {
        switch(id) {
            case 1: return "Heilt 60 HP"; 
            case 2: return "Heilt 30 Mana";
            case 3: return "Basic Schwert\n+5 Stärke\n+10 Dmg"; 
            case 4: return "Basic Helm\n+20 Vitalität\n+5 Rüstung"; 
            case 5: return "Basic Rüstung\n+40 Vitalität\n+10 Rüstung"; 
            case 6: return "Basic Ring\n+5 Int\n+20 Mana"; 
            case 7: return "Basic Stiefel\n+5 Fluch"; 
            case 8: return "Basic Zauberstab\n+10 Int\n+10 Zauberschaden";
            case 9: return "Rare Schwert\n+12 Stärke\n+25 Dmg"; 
            case 10: return "Rare Helm\n+45 Vitalität\n+12 Rüstung"; 
            case 11: return "Rare Rüstung\n+80 Vitalität\n+25 Rüstung"; 
            case 12: return "Rare Ring\n+12 Int\n+50 Mana"; 
            case 13: return "Rare Boots\n+12 Fluch"; 
            case 14: return "Rare Zauberstab\n+25 Int\n+25 Zauberschaden";
            case 15: return "Untotenschwert\n+20 Stärke\n+15 Zauberschaden";
            case 16: return "Epic Helm\n+100 Vitalität\n+25 Rüstung"; 
            case 22: return "Legendary Helm\n+250 Vitalität\n+60 Rüstung";
            case 17: return "Epic Rüstung\n+150 Vitalität\n+40 Rüstung"; 
            case 23: return "Legendary Rüstung\n+400 Vitalität\n+100 Rüstung";
            case 18: return "Epic Ring\n+25 Int\n+100 Mana";
            case 24: return "Legendary Ring\n+60 Int\n+250 Mana";
            case 19: return "Epic Boots\n+25 Fluch";
            case 25: return "Legendary Boots\n+60 Fluch";
            case 21: return "Epic Schwert\n+30 Stärke\n+60 Dmg"; 
            case 27: return "Legendary Schwert\n+75 Stärke\n+150 Dmg";
            case 26: return "Epic Zauberstab\n+50 Int\n+60 Zauberschaden";
            case 28: return "Legendary Zauberstab\n+120 Int\n+150 Zauberschaden";
            default: return "";
        }
    }

    public int GetPotionCount() => GetItemCount(1);
    public int GetItemCount(int type) { int c = 0; if (slotItemType != null) foreach (int t in slotItemType) if (t == type) c++; return c; }
}