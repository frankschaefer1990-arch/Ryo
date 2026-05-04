using UnityEngine;

public class MyUIManager : MonoBehaviour
{
    public static MyUIManager Instance;

    [Header("Panels")]
    public GameObject backpackPanel;      // Backpack / Tasche
    public GameObject inventoryPanel;     // Großes Inventar / Equipment
    public GameObject attributePanel;     // Attribute
    public GameObject lockedDoorPopup;
    public GameObject shopPanel;

    [Header("Keys")]
    public KeyCode backpackKey = KeyCode.B;
    public KeyCode inventoryKey = KeyCode.I;
    public KeyCode attributeKey = KeyCode.T;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ReconnectUIFromGameManager();
        CloseAllPanels();
    }

    private void Update()
    {
        // Cursor-Zustand basierend auf offenen Fenstern steuern
        UpdateCursorState();

        // BACKPACK (B) - Öffnet/Schließt beides
        if (Input.GetKeyDown(backpackKey))
        {
            ToggleBackpack();
        }

        // INVENTORY (I) - Nur Equipment
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }

        // ATTRIBUTE (T)
        if (Input.GetKeyDown(attributeKey))
        {
            ToggleAttributes();
        }

        // ESC = ALLES SCHLIESSEN
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPanels();
        }
    }

    private void UpdateCursorState()
    {
        // Wenn irgendein Panel offen ist (aktiv in der Hierarchie) -> Maus zeigen
        bool anyPanelOpen = (backpackPanel != null && backpackPanel.activeInHierarchy) ||
                            (inventoryPanel != null && inventoryPanel.activeInHierarchy) ||
                            (attributePanel != null && attributePanel.activeInHierarchy) ||
                            (shopPanel != null && shopPanel.activeInHierarchy) ||
                            (lockedDoorPopup != null && lockedDoorPopup.activeInHierarchy);

        if (anyPanelOpen)
        {
            if (!Cursor.visible)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            if (Cursor.visible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void ReconnectUIFromGameManager()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas nicht gefunden!");
            return;
        }

        // Suche InventoryPanel
        if (inventoryPanel == null || inventoryPanel.scene.name == null)
        {
            Transform inv = canvas.transform.Find("InventoryPanel");
            if (inv == null) inv = canvas.transform.Find("Inventory");
            
            // Suche tiefer falls nötig
            if (inv == null)
            {
                foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "InventoryPanel" || t.name == "Inventory")
                    {
                        inv = t;
                        break;
                    }
                }
            }

            if (inv != null) inventoryPanel = inv.gameObject;
        }

        // Suche BackpackPanel
        if (backpackPanel == null || backpackPanel.scene.name == null)
        {
            // Erst unter InventoryPanel suchen
            if (inventoryPanel != null)
            {
                Transform backpack = inventoryPanel.transform.Find("BackpackPanel");
                if (backpack == null) backpack = inventoryPanel.transform.Find("Backpack");
                
                if (backpack != null) backpackPanel = backpack.gameObject;
            }
            
            // Wenn immer noch null, überall unter Canvas suchen
            if (backpackPanel == null)
            {
                foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "BackpackPanel" || t.name == "Backpack")
                    {
                        backpackPanel = t.gameObject;
                        break;
                    }
                }
            }
        }

        // Attribute Panel
        if (attributePanel == null || attributePanel.scene.name == null)
        {
            Transform attr = canvas.transform.Find("AttributePanel");
            if (attr == null)
            {
                foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "AttributePanel") { attr = t; break; }
                }
            }
            if (attr != null) attributePanel = attr.gameObject;
        }

        // Shop Panel
        if (shopPanel == null || shopPanel.scene.name == null)
        {
            Transform shop = canvas.transform.Find("ShopPanel");
            if (shop == null)
            {
                foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "ShopPanel") { shop = t; break; }
                }
            }
            if (shop != null) shopPanel = shop.gameObject;
        }

        if (backpackPanel == null)
        {
            Debug.LogError("BackpackPanel konnte ABSOLUT NICHT gefunden werden auf " + canvas.name);
        }
        else
        {
            Debug.Log("BackpackPanel erfolgreich verbunden: " + backpackPanel.name);
        }
    }

    public void ToggleBackpack()
    {
        if (inventoryPanel == null || backpackPanel == null) return;

        // Wenn eines von beiden zu ist -> mach beides auf
        if (!inventoryPanel.activeSelf || !backpackPanel.activeSelf)
        {
            SetBackpackState(true);
        }
        else
        {
            // Wenn beides offen war -> mach beides zu
            SetBackpackState(false);
        }
    }

    public void SetBackpackState(bool state)
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(state);
        if (backpackPanel != null) backpackPanel.SetActive(state);
        
        // Reset layout if we are just toggling normally
        if (state) ResetLayout();
    }

    public void SetShopLayout(bool active)
    {
        if (shopPanel == null || inventoryPanel == null)
        {
            ReconnectUIFromGameManager();
            if (shopPanel == null || inventoryPanel == null) return;
        }

        // --- CANVAS SCALER OPTIMIERUNG ---
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        RectTransform shopRect = shopPanel.GetComponent<RectTransform>();
        RectTransform invRect = inventoryPanel.GetComponent<RectTransform>();
        UnityEngine.UI.Image invImage = inventoryPanel.GetComponent<UnityEngine.UI.Image>();

        if (active)
        {
            // --- SHOP LAYOUT AKTIV ---
            float targetScale = 0.7f; 
            
            // Pivots so setzen, dass sie sich in der Mitte treffen
            shopRect.pivot = new Vector2(1f, 1f); // Oben-Rechts
            invRect.pivot = new Vector2(0f, 1f);  // Oben-Links

            // Anker Top-Center
            shopRect.anchorMin = shopRect.anchorMax = new Vector2(0.5f, 1f);
            invRect.anchorMin = invRect.anchorMax = new Vector2(0.5f, 1f);
            
            shopRect.localScale = new Vector3(targetScale, targetScale, 1f);
            invRect.localScale = new Vector3(targetScale, targetScale, 1f);

            // Positionen: Beide treffen sich exakt bei X=0 (Bildschirmmitte)
            shopRect.anchoredPosition = new Vector2(0f, -50f);
            invRect.anchoredPosition = new Vector2(0f, -50f); 
            
            if (invImage != null) invImage.enabled = false;
            
            foreach (Transform child in inventoryPanel.transform)
            {
                if (child.name != "BackpackPanel")
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);
                    RectTransform bpRect = child.GetComponent<RectTransform>();
                    // Im Shop-Modus soll der Backpack das Zentrum des invRect sein
                    bpRect.anchorMin = bpRect.anchorMax = new Vector2(0f, 1f);
                    bpRect.pivot = new Vector2(0f, 1f);
                    bpRect.anchoredPosition = Vector2.zero;
                    bpRect.localScale = Vector3.one; 
                }
            }

            shopPanel.SetActive(true);
            inventoryPanel.SetActive(true);
        }
        else
        {
            // --- ZURÜCK ZUM NORMALEN LAYOUT ---
            ResetLayout();
            if (shopPanel != null) shopPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (backpackPanel != null) backpackPanel.SetActive(false);
        }
    }

    private void ResetLayout()
    {
        float normalScale = 0.65f; // Etwas kleiner für bessere Übersicht

        if (shopPanel != null)
        {
            RectTransform shopRect = shopPanel.GetComponent<RectTransform>();
            shopRect.anchorMin = shopRect.anchorMax = new Vector2(0.5f, 0.5f);
            shopRect.pivot = new Vector2(0.5f, 0.5f);
            shopRect.anchoredPosition = Vector2.zero;
            shopRect.localScale = Vector3.one;
        }

        if (inventoryPanel != null)
        {
            RectTransform invRect = inventoryPanel.GetComponent<RectTransform>();
            // Fest am rechten Rand verankern
            invRect.anchorMin = invRect.anchorMax = new Vector2(1f, 0.5f);
            invRect.pivot = new Vector2(1f, 0.5f); 
            invRect.anchoredPosition = new Vector2(-20f, 0); 
            invRect.localScale = new Vector3(normalScale, normalScale, 1f);
            
            UnityEngine.UI.Image invImage = inventoryPanel.GetComponent<UnityEngine.UI.Image>();
            if (invImage != null) invImage.enabled = true;

            foreach (Transform child in inventoryPanel.transform)
            {
                child.gameObject.SetActive(true);
                
                if (child.name == "BackpackPanel")
                {
                    RectTransform bpRect = child.GetComponent<RectTransform>();
                    // Rucksack soll oben-links am Inventar hängen
                    bpRect.anchorMin = bpRect.anchorMax = new Vector2(0f, 1f);
                    bpRect.pivot = new Vector2(1f, 1f);
                    bpRect.anchoredPosition = new Vector2(-10f, 0); // Direkt links bündig oben
                    bpRect.localScale = Vector3.one;
                }
            }
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool isOpen = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isOpen);

        if (inventoryPanel.activeSelf)
        {
            ResetLayout(); // Sicherstellen, dass die Größe stimmt
        }

        if (!inventoryPanel.activeSelf && backpackPanel != null)
        {
            backpackPanel.SetActive(false);
        }
    }

    public void ToggleAttributes()
    {
        if (attributePanel == null) return;
        attributePanel.SetActive(!attributePanel.activeSelf);
    }

    public void CloseAllPanels()
    {
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (attributePanel != null) attributePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // DialogueUI Singleton nutzen um nur das Frame zu verstecken, 
        // damit das Script-Objekt (lockedDoorPopup) aktiv bleibt!
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideAll();
        }
        else if (lockedDoorPopup != null)
        {
            lockedDoorPopup.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}