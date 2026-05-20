using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MyUIManager : MonoBehaviour
{
    public static MyUIManager Instance;

    [Header("Panels")]
    public GameObject backpackPanel;
    public GameObject inventoryPanel;
    public GameObject attributePanel;
    public GameObject skillPanel;
    public GameObject mainMenuPanel;
    public GameObject bottomMenuPanel;
    public GameObject lockedDoorPopup;
    public GameObject shopPanel;
    public TextMeshProUGUI backpackGoldText;

    [Header("Keys")]
    public KeyCode backpackKey = KeyCode.B;
    public KeyCode inventoryKey = KeyCode.I;
    public KeyCode attributeKey = KeyCode.T;
    public KeyCode skillKey = KeyCode.K;

    [Header("Cursor Settings")]
    public float cursorSize = 40f;
    public Sprite cursorSprite;
    public RectTransform softwareCursor;

    public bool isLocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        Debug.Log("MyUIManager: Initialized and persistent.");
    }

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectUIFromGameManager;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerGold.OnGoldChanged += RefreshGoldDisplay;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectUIFromGameManager;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerGold.OnGoldChanged -= RefreshGoldDisplay;
    }

    private void RefreshGoldDisplay()
    {
        if (backpackGoldText != null && PlayerGold.Instance != null)
        {
            backpackGoldText.text = PlayerGold.Instance.currentGold.ToString();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLocked = false;
        
        if (inventoryPanel == null || (!inventoryPanel.activeInHierarchy && !inventoryPanel.transform.root.gameObject.activeInHierarchy))
        {
            ReconnectUIFromGameManager();
        }
        
        CloseAllPanels();
        RefreshGoldDisplay();
    }

    private void Start()
    {
        isLocked = false; // Reset lock
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll(); // Reset dialogue
        
        ReconnectUIFromGameManager();
        CloseAllPanels();
        RefreshGoldDisplay();
    }

    private void Update()
    {
        bool inBattle = IsInBattleScene();
        bool isSplash = SceneManager.GetActiveScene().name == "SplashScreen";
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

        UpdateCursorState(inBattle);
        UpdateSoftwareCursor();

        // Update BottomMenuPanel visibility
if (bottomMenuPanel != null)
        {
            // Deactivate in Battle, SplashScreen, MainMenu, or during Cutscenes (isLocked)
            bool shouldBeVisible = !inBattle && !isSplash && !isMainMenu && !isLocked;
            if (bottomMenuPanel.activeSelf != shouldBeVisible)
            {
                bottomMenuPanel.SetActive(shouldBeVisible);
            }
        }

        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        
        // Debug: press Alt+L to see lock status
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"MyUIManager Status: isLocked={isLocked}, dialogueActive={dialogueActive}, inBattle={inBattle}, scene={SceneManager.GetActiveScene().name}");
        }

        // ESC is special: it should always be able to close panels regardless of isLocked
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsAnyPanelOpen())
            {
                CloseAllPanels();
            }
            else if (!isMainMenu && !isSplash && !isLocked && !dialogueActive)
            {
                ToggleMainMenu();
            }
        }

        if (isLocked || dialogueActive || inBattle) return;

        // Shortcuts
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
            (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null ||
             UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null))
        {
            return;
        }

        if (Input.GetKeyDown(backpackKey)) ToggleBackpack();
        if (Input.GetKeyDown(inventoryKey)) ToggleInventory();
        if (Input.GetKeyDown(attributeKey)) ToggleAttributes();
        if (Input.GetKeyDown(skillKey)) ToggleSkills();
        
        // Cheat: Press '5' to add 5 skill points
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.AddPoints(5);
                var ui = FindAnyObjectByType<SkillUI>();
                if (ui != null) ui.RefreshUI();
                Debug.Log("MyUIManager: Cheat - Added 5 skill points.");
            }
        }
        }

    public bool IsAnyPanelOpen()
    {
        bool bpOpen = backpackPanel != null && backpackPanel.activeInHierarchy;
        bool invOpen = inventoryPanel != null && inventoryPanel.activeInHierarchy;
        bool attrOpen = attributePanel != null && attributePanel.activeInHierarchy;
        bool skillOpen = skillPanel != null && skillPanel.activeInHierarchy;
        bool shopOpen = shopPanel != null && shopPanel.activeInHierarchy;
        bool lockOpen = lockedDoorPopup != null && lockedDoorPopup.activeInHierarchy;
        bool saveOpen = SaveSlotManager.Instance != null && SaveSlotManager.Instance.saveSlotPanel != null && SaveSlotManager.Instance.saveSlotPanel.activeInHierarchy;
        bool menuOpen = mainMenuPanel != null && mainMenuPanel.activeInHierarchy;
        bool chestOpen = ChestUI.Instance != null && ChestUI.Instance.chestPanel != null && ChestUI.Instance.chestPanel.activeInHierarchy;

        return bpOpen || invOpen || attrOpen || skillOpen || shopOpen || lockOpen || saveOpen || menuOpen || chestOpen;
    }

    private bool IsInBattleScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName != null && (sceneName.Contains("Battle") || sceneName.Contains("Kampf"));
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
            c.sortingOrder = 9999;

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

    private void UpdateCursorState(bool inBattle)
    {
        bool anyPanelOpen = IsAnyPanelOpen();
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        
        // Der Mauszeiger soll immer sichtbar sein, außer in Dialogen (Immersions-Modus)
        if (dialogueActive)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ReconnectUIFromGameManager()
    {
        if (inventoryPanel != null && inventoryPanel.transform.root.gameObject.scene.name == "DontDestroyOnLoad") return;

        GameObject targetCanvas = GetTargetCanvas();
        if (targetCanvas == null) return;

        inventoryPanel = FindChildRecursive(targetCanvas.transform, "InventoryPanel");
        backpackPanel = FindChildRecursive(targetCanvas.transform, "BackpackPanel");
        if (backpackPanel == null && inventoryPanel != null) {
            backpackPanel = FindChildRecursive(inventoryPanel.transform, "BackpackPanel");
        }
        attributePanel = FindChildRecursive(targetCanvas.transform, "AttributePanel");
        skillPanel = FindChildRecursive(targetCanvas.transform, "SkillPanel");
        shopPanel = FindChildRecursive(targetCanvas.transform, "ShopPanel");
        lockedDoorPopup = FindChildRecursive(targetCanvas.transform, "LockedDoorPopup");
        
        // RECONNECT MainMenuPanel
        mainMenuPanel = FindChildRecursive(targetCanvas.transform, "MainMenuPanel");
        if (mainMenuPanel == null) mainMenuPanel = FindChildRecursive(targetCanvas.transform, "MenuPanel");
        if (mainMenuPanel == null) mainMenuPanel = FindChildRecursive(targetCanvas.transform, "MenuContainer");

        if (inventoryPanel != null) {
            var btn = inventoryPanel.transform.Find("BackpackButton")?.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) {
                foreach(var b in inventoryPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true)) {
                    if (b.name == "BackpackButton") { btn = b; break; }
                }
            }
            if (btn != null) {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ToggleBackpack);
            }
        }
        
        ResetLayout();
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }

    public void ToggleBackpack()
    {
        if (isLocked) { Debug.Log("MyUIManager: UI is locked."); return; }
        if (inventoryPanel == null || backpackPanel == null) { ReconnectUIFromGameManager(); }
        if (inventoryPanel == null || backpackPanel == null) {
            Debug.LogError($"MyUIManager: References missing! Inventory: {inventoryPanel != null}, Backpack: {backpackPanel != null}");
            return;
        }

        bool shouldOpen = !backpackPanel.activeSelf;
        
        // Reset selection when opening
        if (shouldOpen && InventoryManager.Instance != null) {
            InventoryManager.Instance.DeselectSlot();
        }

        if (shouldOpen)
{
            ResetLayout();
            inventoryPanel.SetActive(true);
            backpackPanel.SetActive(true);
            
            // UPDATE GOLD DISPLAY
            if (backpackGoldText != null && PlayerGold.Instance != null)
            {
                backpackGoldText.text = PlayerGold.Instance.currentGold.ToString();
            }

            Debug.Log("MyUIManager: Set backpack and inventory to active.");
        }
        else
        {
            backpackPanel.SetActive(false);
            Debug.Log("MyUIManager: Set backpack to inactive.");
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
            backpackPanel.SetActive(false);
        }
        else
        {
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
        }
    }

    public void SetBackpackState(bool state)
    {
        if (state) {
            if (inventoryPanel != null) inventoryPanel.SetActive(true);
            if (backpackPanel != null) backpackPanel.SetActive(true);
        }
        else {
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
            shopRT.anchorMin = new Vector2(0, 1);
            shopRT.anchorMax = new Vector2(0, 1);
            shopRT.pivot = new Vector2(0, 1);
            shopRT.anchoredPosition = new Vector2(50, -50);

            backpackPanel.transform.SetParent(shopPanel.transform, false);
            bpRT.anchorMin = new Vector2(1, 1);
            bpRT.anchorMax = new Vector2(1, 1);
            bpRT.pivot = new Vector2(0, 1);
            bpRT.anchoredPosition = Vector2.zero;
            bpRT.sizeDelta = new Vector2(874.15f, 693.14f);
            bpRT.localScale = Vector3.one;

            shopPanel.SetActive(true);
            backpackPanel.SetActive(true);
        } else {
            GameObject targetCanvas = GetTargetCanvas();
            if (targetCanvas != null) backpackPanel.transform.SetParent(inventoryPanel != null ? inventoryPanel.transform : targetCanvas.transform, false);
            shopPanel.SetActive(false);
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
            ResetLayout();
        }
    }

    private GameObject GetTargetCanvas()
    {
        if (GameManager.Instance != null && GameManager.Instance.canvas != null) return GameManager.Instance.canvas;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases) {
            if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") return c.gameObject;
        }
        return null;
    }

    private void ResetLayout() 
    {
        if (inventoryPanel == null || backpackPanel == null) return;
        RectTransform invRT = inventoryPanel.GetComponent<RectTransform>();
        RectTransform bpRT = backpackPanel.GetComponent<RectTransform>();
        RectTransform attRT = (attributePanel != null) ? attributePanel.GetComponent<RectTransform>() : null;
        
        invRT.sizeDelta = new Vector2(1031.4f, 1440.5f);
        bpRT.sizeDelta = new Vector2(874.15f, 693.14f);
        if (attRT != null) attRT.sizeDelta = new Vector2(704.01f, 1200.4f);

        invRT.anchorMin = new Vector2(1, 0.5f);
        invRT.anchorMax = new Vector2(1, 0.5f);
        invRT.pivot = new Vector2(1, 0.5f);
        invRT.anchoredPosition = new Vector2(-20f, 0f);
        invRT.localScale = Vector3.one * 0.45f; 

        // Ensure backpack is child of inventory or canvas correctly
        if (backpackPanel.transform.parent != inventoryPanel.transform) {
            backpackPanel.transform.SetParent(inventoryPanel.transform, false);
        }

        bpRT.anchorMin = new Vector2(0.5f, 0.5f);
        bpRT.anchorMax = new Vector2(0.5f, 0.5f);
        bpRT.pivot = new Vector2(0.5f, 0.5f);
        bpRT.anchoredPosition = new Vector2(-1028.09f, 304.5f); 
        bpRT.localScale = Vector3.one * 1.25f; 

        if (attRT != null) {
            attRT.anchorMin = new Vector2(0, 0.5f);
            attRT.anchorMax = new Vector2(0, 0.5f);
            attRT.pivot = new Vector2(0, 0.5f);
            attRT.anchoredPosition = new Vector2(20f, 0f);
            attRT.localScale = Vector3.one * 0.5f;
        }

        foreach (Transform child in inventoryPanel.transform) {
            if (child.gameObject != backpackPanel) child.gameObject.SetActive(true);
        }
    }

    public void ToggleAttributes() { if (isLocked) return; if (attributePanel != null) attributePanel.SetActive(!attributePanel.activeSelf); }

    public void ToggleSkills() { if (isLocked) return; if (skillPanel != null) skillPanel.SetActive(!skillPanel.activeSelf); }

    public void ToggleMainMenu()
{
        if (isLocked) return;
        if (mainMenuPanel != null)
        {
            bool state = !mainMenuPanel.activeSelf;
            mainMenuPanel.SetActive(state);
            
            if (state) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void SaveGame()
    {
        if (SaveSlotManager.Instance == null)
        {
            var ssm = Object.FindAnyObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
            if (ssm != null) ssm.Open(false);
            else if (SaveSystem.Instance != null) SaveSystem.Instance.Save();
        }
        else
        {
            SaveSlotManager.Instance.Open(false);
        }
    }

    public void OpenLoadPanelFromMenu()
    {
        if (SaveSlotManager.Instance != null)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            SaveSlotManager.Instance.Open(true);
            return;
        }
        else
        {
            var ssm = Object.FindAnyObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
            if (ssm != null)
            {
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                ssm.Open(true);
                return;
            }
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        GameObject lp = FindChildRecursive(transform, "LoadPanel");
        // ... rest of the old logic
    }

    public void CloseLoadPanel()
    {
        if (SaveSlotManager.Instance != null) SaveSlotManager.Instance.Close();
        else {
            var ssm = Object.FindAnyObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
            if (ssm != null) ssm.Close();
        }
        GameObject lp = FindChildRecursive(transform, "LoadPanel");
        if (lp != null) lp.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true); 
    }

    public void LoadGame()
    {
        if (SaveSlotManager.Instance == null)
        {
            var ssm = Object.FindAnyObjectByType<SaveSlotManager>(FindObjectsInactive.Include);
            if (ssm != null) ssm.Open(true);
            else if (SaveSystem.Instance != null) SaveSystem.Instance.Load();
        }
        else
        {
            SaveSlotManager.Instance.Open(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Spiel beenden.");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void CloseAllPanels()
    {
        if (SaveSlotManager.Instance != null) SaveSlotManager.Instance.Close();
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (attributePanel != null) attributePanel.SetActive(false);
        if (skillPanel != null) skillPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (lockedDoorPopup != null) lockedDoorPopup.SetActive(false);
        
        if (shopPanel != null) {
ShopManager shop = FindFirstObjectByType<ShopManager>();
            if (shop != null) shop.CloseShop();
            else shopPanel.SetActive(false);
        }
        
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        bool inBattle = IsInBattleScene();
    Cursor.visible = inBattle;
        Cursor.lockState = inBattle ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
