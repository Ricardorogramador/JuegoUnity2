using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaidUIManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button toggleButton;
    public RectTransform dropdownPanel;   // Panel con Image + VLG + CSF
    public RectTransform content;         // Donde se instancian los items (puede ser el mismo panel)
    public GameObject raidEntryPrefab;    // Botón + Text (TMP)
    public Canvas rootCanvas;

    [Header("Comportamiento")]
    public float yOffset = 6f;
    public Vector2 minSize = new Vector2(200f, 0f);
    public Vector2 maxSize = new Vector2(480f, 360f);   // sube X si quieres líneas más largas
    public bool closeOnOutsideClick = true;
    public bool closeOnEscape = true;

    [Header("Ajuste automático")]
    [Tooltip("Ensanche automático del panel para que quepa el texto (hasta Max Size X).")]
    public bool autoWidthToContent = true;

    [Tooltip("Si el texto no cabe en una línea, permite que salte a varias y ajusta la altura del ítem.")]
    public bool allowWrapIfNeeded = true;

    [Tooltip("Padding horizontal total dentro del ítem (suma de margen izquierda + derecha del label).")]
    public float itemHorizontalPadding = 24f; // 12 + 12

    [Tooltip("Padding vertical total dentro del ítem (arriba + abajo del label).")]
    public float itemVerticalPadding = 12f;   // 6 + 6

    [Tooltip("Altura mínima por ítem (una línea).")]
    public float itemMinHeight = 44f;

    [Header("Texto si no hay raids")]
    public string emptyStateText = "Sin raids disponibles";

    private GameManager gm;
    private bool isOpen;
    private Camera uiCamera;
    private RectTransform canvasRect;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        uiCamera = rootCanvas != null ? rootCanvas.worldCamera : null;
        canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleRaidPanel);
    }

    private void Start()
    {
        if (dropdownPanel != null && dropdownPanel.gameObject.activeSelf)
            dropdownPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleRaidPanel);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (closeOnOutsideClick && Input.GetMouseButtonDown(0))
        {
            Vector2 mouse = Input.mousePosition;
            bool overPanel = dropdownPanel != null &&
                             RectTransformUtility.RectangleContainsScreenPoint(dropdownPanel, mouse, uiCamera);
            bool overButton = toggleButton != null &&
                              RectTransformUtility.RectangleContainsScreenPoint(toggleButton.transform as RectTransform, mouse, uiCamera);

            if (!overPanel && !overButton)
                Close();
        }
    }

    public void ToggleRaidPanel()
    {
        if (isOpen) Close();
        else Open();
    }

    private void Open()
    {
        if (dropdownPanel == null || content == null || rootCanvas == null) return;

        dropdownPanel.gameObject.SetActive(true);
        isOpen = true;

        RefreshRaidList();
        EnsureMinWidth();
        if (autoWidthToContent) AutoSizeWidthToContent();
        AdjustItemHeightsToContent();  // después de fijar el ancho
        PositionPanelUnderButton();
        ClampToCanvasAndLimitHeight();
    }

    private void Close()
    {
        if (dropdownPanel == null) return;
        dropdownPanel.gameObject.SetActive(false);
        isOpen = false;
    }

    public void RefreshRaidList()
    {
        // Limpia hijos (sin borrar el prefab si vive dentro)
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var ch = content.GetChild(i).gameObject;
            if (raidEntryPrefab != null && ch == raidEntryPrefab) continue;
            Destroy(ch);
        }

        int count = (gm != null && gm.activeRaids != null) ? gm.activeRaids.Count : 0;
        Debug.Log($"[RaidUI] Poblando dropdown. Raids activas: {count}");

        if (count == 0)
        {
            CreateItem(null, emptyStateText, false);
        }
        else
        {
            foreach (var raid in gm.activeRaids)
            {
                string title = $"Raid nv {raid.nivel} ({raid.enemigos.Count} enemigos)";
                CreateItem(() =>
                {
                    Close();
                    gm.EnterRaid(raid);
                }, title, true);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownPanel);
    }

    private void CreateItem(UnityEngine.Events.UnityAction onClick, string text, bool interactable)
    {
        GameObject entry = (raidEntryPrefab != null)
            ? Instantiate(raidEntryPrefab, content)
            : new GameObject("RaidItem", typeof(RectTransform), typeof(Image), typeof(Button));

        entry.SetActive(true);

        if (raidEntryPrefab == null)
        {
            var img = entry.GetComponent<Image>();
            img.color = new Color(0.18f, 0.24f, 0.31f, 1f); // fondo oscuro por defecto
            var le = entry.AddComponent<LayoutElement>();
            le.minHeight = itemMinHeight; le.preferredHeight = Mathf.Max(itemMinHeight, 48f);
        }

        var btn = entry.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.interactable = interactable;
            if (onClick != null) btn.onClick.AddListener(onClick);
        }

        // Label
        var label = entry.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(entry.transform, false);
            label = go.GetComponent<TextMeshProUGUI>();
        }

        // Normaliza RT del label según paddings configurables
        var rt = label.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        float halfHPad = itemHorizontalPadding * 0.5f;
        float halfVPad = itemVerticalPadding * 0.5f;
        rt.offsetMin = new Vector2(halfHPad, halfVPad);       // L, B
        rt.offsetMax = new Vector2(-halfHPad, -halfVPad);     // -R, -T

        // Estilo texto legible
        label.enableWordWrapping = allowWrapIfNeeded;          // Permitimos wrap si hace falta
        label.overflowMode = TextOverflowModes.Overflow;       // No pongas ellipsis
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.fontSize = 22f;

        // Color según fondo del botón
        var bg = entry.GetComponent<Image>();
        bool lightBg = bg != null && (0.2126f * bg.color.r + 0.7152f * bg.color.g + 0.0722f * bg.color.b) > 0.7f;
        label.color = lightBg ? Color.black : Color.white;
        label.outlineWidth = 0.2f;
        label.outlineColor = new Color(0f, 0f, 0f, lightBg ? 0.2f : 0.7f);

        label.text = text;
        var c = label.color; c.a = 1f; label.color = c;
    }

    // Asegura que el panel no sea más estrecho que el botón
    private void EnsureMinWidth()
    {
        float targetWidth = minSize.x;
        if (toggleButton != null)
        {
            var rtBtn = toggleButton.transform as RectTransform;
            if (rtBtn != null) targetWidth = Mathf.Max(targetWidth, rtBtn.rect.width);
        }

        var le = dropdownPanel.GetComponent<LayoutElement>();
        if (le == null) le = dropdownPanel.gameObject.AddComponent<LayoutElement>();
        le.minWidth = targetWidth;
        le.preferredWidth = Mathf.Max(le.preferredWidth, targetWidth);
    }

    // Ensancha el panel hasta que el texto más largo quepa (limitado por maxSize.x)
    private void AutoSizeWidthToContent()
    {
        var vlg = dropdownPanel.GetComponent<VerticalLayoutGroup>();
        float panelPads = vlg != null ? vlg.padding.left + vlg.padding.right : 0f;

        float maxNeeded = minSize.x;
        for (int i = 0; i < content.childCount; i++)
        {
            var item = content.GetChild(i) as RectTransform;
            if (raidEntryPrefab != null && item.gameObject == raidEntryPrefab) continue;

            var label = item.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) continue;

            label.ForceMeshUpdate();
            float needed = label.preferredWidth + itemHorizontalPadding + panelPads;
            maxNeeded = Mathf.Max(maxNeeded, needed);
        }

        float target = Mathf.Clamp(maxNeeded, minSize.x, maxSize.x);
        var le = dropdownPanel.GetComponent<LayoutElement>();
        if (le == null) le = dropdownPanel.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = target;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownPanel);
    }

    // Ajusta la altura de cada ítem según su texto (para permitir 2+ líneas si hace falta)
    private void AdjustItemHeightsToContent()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            var item = content.GetChild(i) as RectTransform;
            if (raidEntryPrefab != null && item.gameObject == raidEntryPrefab) continue;

            var label = item.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) continue;

            // Forzar cálculo con el ancho final ya aplicado
            LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownPanel);
            label.ForceMeshUpdate();

            float preferredH = label.preferredHeight + itemVerticalPadding;
            var le = item.GetComponent<LayoutElement>();
            if (le == null) le = item.gameObject.AddComponent<LayoutElement>();

            le.minHeight = itemMinHeight;
            le.preferredHeight = Mathf.Max(itemMinHeight, Mathf.Ceil(preferredH));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void PositionPanelUnderButton()
    {
        if (toggleButton == null || dropdownPanel == null || canvasRect == null) return;

        var btnRT = toggleButton.transform as RectTransform;
        Vector3[] corners = new Vector3[4];
        btnRT.GetWorldCorners(corners); // 0=BL 1=TL 2=TR 3=BR
        Vector3 bottomCenterWorld = (corners[0] + corners[3]) * 0.5f;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(uiCamera, bottomCenterWorld),
            uiCamera,
            out localPoint
        );

        dropdownPanel.pivot = new Vector2(0.5f, 1f);
        dropdownPanel.anchorMin = dropdownPanel.anchorMax = new Vector2(0.5f, 0.5f);
        dropdownPanel.anchoredPosition = localPoint + new Vector2(0f, -yOffset);
    }

    private void ClampToCanvasAndLimitHeight()
    {
        if (dropdownPanel == null || canvasRect == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownPanel);

        var le = dropdownPanel.GetComponent<LayoutElement>();
        if (le == null) le = dropdownPanel.gameObject.AddComponent<LayoutElement>();

        float clampedH = Mathf.Min(dropdownPanel.rect.height, maxSize.y);
        float width = Mathf.Clamp(dropdownPanel.rect.width, minSize.x, maxSize.x);

        le.preferredHeight = clampedH;
        le.preferredWidth = width;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownPanel);

        Vector2 pos = dropdownPanel.anchoredPosition;
        Vector2 size = dropdownPanel.rect.size;

        float halfW = size.x * 0.5f;
        float canvasHalfW = canvasRect.rect.width * 0.5f;
        float canvasHalfH = canvasRect.rect.height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -canvasHalfW + halfW + 8f, canvasHalfW - halfW - 8f);

        float bottomY = pos.y - size.y;
        float minY = -canvasHalfH + 8f;
        if (bottomY < minY)
        {
            float delta = minY - bottomY;
            pos.y += delta;
        }

        dropdownPanel.anchoredPosition = pos;
    }
}