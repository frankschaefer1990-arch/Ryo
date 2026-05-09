using UnityEngine;
using UnityEngine.SceneManagement;

public class MyUIManager : MonoBehaviour
{
    public static MyUIManager Instance;

    [Header("Panels")]
    public GameObject backpackPanel;
    public GameObject inventoryPanel;
    public GameObject attributePanel;
    public GameObject lockedDoorPopup;
    public GameObject shopPanel;

    [Header("Keys")]
    public KeyCode backpackKey = KeyCode.B;
    public KeyCode inventoryKey = KeyCode.I;
    public KeyCode attributeKey = KeyCode.T;

    [Header("Cursor Settings")]
    public float cursorSize = 40f;
    public Sprite cursorSprite;
    public RectTransform softwareCursor;

    public bool isLocked = false;

    private void Awake()
{
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        if (transform.parent == null || transform.parent.name != "PersistentSystems")
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectUIFromGameManager;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectUIFromGameManager;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLocked = false;
        ReconnectUIFromGameManager();
        CloseAllPanels();
    }

    private void Start()
    {
        ReconnectUIFromGameManager();
        CloseAllPanels();
    }

    private void Update()
    {
        UpdateCursorState();
        UpdateSoftwareCursor();

        // Check for active dialogues, manual lock, or if BattleScene is loaded
        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        bool inBattle = false;
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            if (SceneManager.GetSceneAt(i).name == "BattleScene") {
                inBattle = true;
                break;
            }
        }
        
        if (isLocked || dialogueActive || inBattle) return;

        if (Input.GetKeyDown(backpackKey)) ToggleBackpack();
if (Input.GetKeyDown(inventoryKey)) ToggleInventory();
        if (Input.GetKeyDown(attributeKey)) ToggleAttributes();
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAllPanels();
    }

    private void UpdateSoftwareCursor()
    {
        if (softwareCursor == null) CreateSoftwareCursor();
        if (softwareCursor == null) return;
        
        bool shouldShow = Cursor.visible;
        if (softwareCursor.gameObject.activeSelf != shouldShow)
            softwareCursor.gameObject.SetActive(shouldShow);
        
        if (shouldShow)
        {
            softwareCursor.position = Input.mousePosition;
            // Force size 80 pixels every frame. 
            // Scaler is ConstantPixelSize, so this is literal screen pixels.
            softwareCursor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursorSize);
            softwareCursor.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cursorSize);
            softwareCursor.localScale = Vector3.one;
            softwareCursor.SetAsLastSibling();
        }
    }

    private Sprite GetCursorSprite()
    {
        if (cursorSprite != null) return cursorSprite;
        cursorSprite = Resources.Load<Sprite>("ActualCursor");
        return cursorSprite;
    }

    private void CreateSoftwareCursor()
    {
        GameObject canvasObj = GameObject.Find("SoftwareCursorCanvas");
        if (canvasObj == null) {
            canvasObj = new GameObject("SoftwareCursorCanvas");
            DontDestroyOnLoad(canvasObj);
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9999; // Above EVERYTHING

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        }

        GameObject cursorObj;
        Transform existing = canvasObj.transform.Find("SoftwareCursor");
        if (existing == null) {
            cursorObj = new GameObject("SoftwareCursor");
            cursorObj.transform.SetParent(canvasObj.transform, false);
        } else {
            cursorObj = existing.gameObject;
        }
        
        softwareCursor = cursorObj.GetComponent<RectTransform>();
        if (softwareCursor == null) softwareCursor = cursorObj.AddComponent<RectTransform>();
        
        softwareCursor.anchorMin = Vector2.zero;
        softwareCursor.anchorMax = Vector2.zero;
        softwareCursor.pivot = new Vector2(0.15f, 0.85f);
        softwareCursor.sizeDelta = new Vector2(cursorSize, cursorSize);

        UnityEngine.UI.Image img = cursorObj.GetComponent<UnityEngine.UI.Image>();
        if (img == null) img = cursorObj.AddComponent<UnityEngine.UI.Image>();
        img.raycastTarget = false; 
        img.sprite = GetCursorSprite();
        img.color = Color.white;
    }

    private void UpdateCursorState()
    {
        bool bpOpen = backpackPanel != null && backpackPanel.activeInHierarchy;
        bool invOpen = inventoryPanel != null && inventoryPanel.activeInHierarchy;
        bool attrOpen = attributePanel != null && attributePanel.activeInHierarchy;
        bool shopOpen = shopPanel != null && shopPanel.activeInHierarchy;
        bool lockOpen = lockedDoorPopup != null && lockedDoorPopup.activeInHierarchy;

        bool anyPanelOpen = bpOpen || invOpen || attrOpen || shopOpen || lockOpen;

        if (anyPanelOpen) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ReconnectUIFromGameManager()
    {
        GameObject targetCanvas = null;
        if (GameManager.Instance != null && GameManager.Instance.canvas != null) {
            targetCanvas = GameManager.Instance.canvas;
        } else {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in canvases) {
                if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") {
                    targetCanvas = c.gameObject;
                    break;
                }
            }
        }

        if (targetCanvas == null) return;

        if (inventoryPanel == null || inventoryPanel.scene.name == null)
            inventoryPanel = FindChildRecursive(targetCanvas.transform, "InventoryPanel");

        if (inventoryPanel != null) {
            var btn = inventoryPanel.transform.Find("BackpackButton")?.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) {
                // Suche rekursiv falls nicht direktes Kind
                foreach(var b in inventoryPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true)) {
                    if (b.name == "BackpackButton") { btn = b; break; }
                }
            }
            if (btn != null) {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ToggleBackpack);
            }
        }
        
        if (backpackPanel == null || backpackPanel.scene.name == null)
            backpackPanel = FindChildRecursive(targetCanvas.transform, "BackpackPanel");

        if (attributePanel == null || attributePanel.scene.name == null)
            attributePanel = FindChildRecursive(targetCanvas.transform, "AttributePanel");

        if (shopPanel == null || shopPanel.scene.name == null)
            shopPanel = FindChildRecursive(targetCanvas.transform, "ShopPanel");
            
        if (lockedDoorPopup == null || lockedDoorPopup.scene.name == null)
            lockedDoorPopup = FindChildRecursive(targetCanvas.transform, "LockedDoorPopup");
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }

    public void ToggleBackpack()
    {
        if (isLocked) return;
        if (inventoryPanel == null || backpackPanel == null) { ReconnectUIFromGameManager(); }
if (inventoryPanel == null || backpackPanel == null) return;

        bool opening = !backpackPanel.activeSelf || !inventoryPanel.activeSelf;

        if (opening)
        {
            // Erst das allgemeine Layout resetten (Inventar nach Rechts)
            ResetLayout();

            // Backpack zum Child vom Inventar machen
            backpackPanel.transform.SetParent(inventoryPanel.transform, false);
            
            // Layout: Backpack links neben das Inventar kleben
            // Anker Oben-Links vom Inventar (0,1)
            // Pivot Oben-Rechts vom Rucksack (1,1) -> Rucksack ragt nach links raus
            RectTransform bpRT = backpackPanel.GetComponent<RectTransform>();
            bpRT.anchorMin = new Vector2(0, 1);
            bpRT.anchorMax = new Vector2(0, 1);
            bpRT.pivot = new Vector2(1, 1);
            bpRT.anchoredPosition = Vector2.zero;

            inventoryPanel.SetActive(true);
            backpackPanel.SetActive(true);
        }
        else
        {
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
        }
    }

    public void ToggleInventory()
    {
        if (isLocked) return;
        if (inventoryPanel == null || backpackPanel == null) { ReconnectUIFromGameManager(); }
        if (inventoryPanel == null) return;

        bool opening = !inventoryPanel.activeSelf;
        
        if (opening)
        {
            ResetLayout();
            inventoryPanel.SetActive(true);
            backpackPanel.SetActive(false); // Bei 'I' bleibt Backpack unsichtbar
        }
        else
        {
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
        }
    }

    public void SetBackpackState(bool state)
    {
        if (state) ToggleBackpack();
        else {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (backpackPanel != null) backpackPanel.SetActive(false);
        }
    }

    public void SetShopLayout(bool active)
    {
        if (shopPanel == null || inventoryPanel == null || backpackPanel == null) { ReconnectUIFromGameManager(); }
        if (shopPanel == null || inventoryPanel == null || backpackPanel == null) return;

        RectTransform shopRT = shopPanel.GetComponent<RectTransform>();
        RectTransform bpRT = backpackPanel.GetComponent<RectTransform>();

        if (active) {
            // 1. Shop Panel: Anchor oben links
            shopRT.anchorMin = new Vector2(0, 1);
            shopRT.anchorMax = new Vector2(0, 1);
            shopRT.pivot = new Vector2(0, 1);
            shopRT.anchoredPosition = new Vector2(50, -50);

            // 2. Backpack zum Child von Shop machen und rechts andocken
            backpackPanel.transform.SetParent(shopPanel.transform, false);
            
            // Anker oben rechts vom Shop, Pivot oben links vom Backpack
            bpRT.anchorMin = new Vector2(1, 1);
            bpRT.anchorMax = new Vector2(1, 1);
            bpRT.pivot = new Vector2(0, 1);
            bpRT.anchoredPosition = Vector2.zero;

            shopPanel.SetActive(true);
            backpackPanel.SetActive(true);
        } else {
            // 3. Zurück zum Inventar-Parent
            backpackPanel.transform.SetParent(inventoryPanel.transform, false);
            
            shopPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
            ResetLayout();
        }
    }

    private void ResetLayout() 
    {
        if (inventoryPanel == null || backpackPanel == null) return;
        RectTransform invRT = inventoryPanel.GetComponent<RectTransform>();
        
        // Restore InventoryPanel Standard (Rechts)
        invRT.anchorMin = new Vector2(1, 0.5f);
        invRT.anchorMax = new Vector2(1, 0.5f);
        invRT.pivot = new Vector2(0.5f, 0.5f);
        invRT.anchoredPosition = new Vector2(-257, 0);

        // Alle Kinder des Inventars wieder aktivieren (außer Backpack)
        foreach (Transform child in inventoryPanel.transform) {
            if (child.gameObject != backpackPanel) child.gameObject.SetActive(true);
        }
    }

    public void ToggleAttributes() { if (isLocked) return; if (attributePanel != null) attributePanel.SetActive(!attributePanel.activeSelf); }

    public void CloseAllPanels()
    {
        if (backpackPanel != null) {
            // Sicherstellen, dass Backpack wieder beim Inventar ist
            if (inventoryPanel != null) backpackPanel.transform.SetParent(inventoryPanel.transform, false);
            backpackPanel.SetActive(false);
        }
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (attributePanel != null) attributePanel.SetActive(false);
        
        if (shopPanel != null) {
            // Falls der Shop offen ist, über den ShopManager schließen (wegen Movement Lock)
            ShopManager shop = FindAnyObjectByType<ShopManager>();
            if (shop != null) {
                shop.CloseShop();
            } else {
                shopPanel.SetActive(false);
            }
        }
        
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();

        Cursor.visible = false;
Cursor.lockState = CursorLockMode.Locked;
    }
}