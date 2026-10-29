using System.Collections.Generic;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ProjectZone
{
    /// <summary>
    /// Dual-panel UI for loot/stash ↔ backpack transfers via HQ ItemTransfering.
    /// Does not replace HQ Item Wheel (middle mouse) — only opens from world interactables.
    /// </summary>
    public sealed class PzInventoryPanelController : MonoBehaviour
    {
        private const string BackpackName = "Backpack";

        private static PzInventoryPanelController _instance;

        private Canvas _canvas;
        private RectTransform _root;
        private Text _titleLabel;
        private Text _hintLabel;
        private Transform _leftGrid;
        private Transform _rightGrid;
        private readonly List<PzSimpleItemSlotUI> _leftSlots = new();
        private readonly List<PzSimpleItemSlotUI> _rightSlots = new();
        private SlotReference _selectedSlot;
        private CursorLockMode _previousLockMode;
        private bool _previousCursorVisible;
        private bool _isOpen;

        public static bool IsOpen => _instance != null && _instance._isOpen;

        public static void Open(ICharacter character, IItemContainer externalContainer, string title)
        {
            EnsureInstance();
            _instance.Show(character, externalContainer, title);
        }

        public static void CloseActive()
        {
            if (_instance != null && _instance._isOpen)
                _instance.Hide();
        }

        private static void EnsureInstance()
        {
            if (_instance != null)
                return;

            var go = new GameObject("PZ_InventoryPanel");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PzInventoryPanelController>();
        }

        private void Update()
        {
            if (!_isOpen)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                Hide();
        }

        private void Show(ICharacter character, IItemContainer externalContainer, string title)
        {
            if (character?.Inventory == null || externalContainer == null)
                return;

            EnsureUi();
            IItemContainer backpack = character.Inventory.FindContainer(ItemContainerFilters.WithName(BackpackName));
            if (backpack == null)
            {
                Debug.LogWarning("[ProjectZone][Inventory] Player has no Backpack container.", this);
                return;
            }

            _titleLabel.text = title;
            _hintLabel.text =
                "HQ backpack (left) + container (right). Click to select, click again to transfer/swap. Esc closes. " +
                "Middle mouse still opens HQ Item Wheel.";

            RebuildGrid(_leftGrid, backpack, _leftSlots, "Backpack");
            RebuildGrid(_rightGrid, externalContainer, _rightSlots, title);
            _rightGrid.gameObject.SetActive(true);

            _selectedSlot = SlotReference.Null;
            _canvas.enabled = true;
            _root.gameObject.SetActive(true);

            _previousLockMode = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _isOpen = true;
            EnsureEventSystem();
        }

        private void Hide()
        {
            if (!_isOpen)
                return;

            DetachSlots(_leftSlots);
            DetachSlots(_rightSlots);
            _selectedSlot = SlotReference.Null;
            _canvas.enabled = false;
            _root.gameObject.SetActive(false);
            Cursor.lockState = _previousLockMode;
            Cursor.visible = _previousCursorVisible;
            _isOpen = false;
        }

        private void RebuildGrid(Transform grid, IItemContainer container, List<PzSimpleItemSlotUI> slots, string header)
        {
            DetachSlots(slots);
            for (int i = grid.childCount - 1; i >= 0; i--)
                Destroy(grid.GetChild(i).gameObject);
            slots.Clear();

            if (container == null)
                return;

            if (!string.IsNullOrEmpty(header))
            {
                var headerGo = new GameObject("Header", typeof(RectTransform));
                headerGo.transform.SetParent(grid, false);
                Text headerText = headerGo.AddComponent<Text>();
                headerText.text = $"{header} ({container.SlotsCount})";
                headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                headerText.fontSize = 16;
                headerText.alignment = TextAnchor.MiddleLeft;
                headerText.color = Color.white;
                LayoutElement le = headerGo.AddComponent<LayoutElement>();
                le.minHeight = 22f;
                le.preferredWidth = 280f;
            }

            for (int i = 0; i < container.SlotsCount; i++)
            {
                PzSimpleItemSlotUI slotUi = CreateSlot(grid);
                SlotReference slot = container.GetSlot(i);
                slotUi.AttachToSlot(slot);
                SelectableButton button = slotUi.Selectable;
                SlotReference captured = slot;
                button.Clicked += _ => OnSlotClicked(captured);
                slots.Add(slotUi);
            }
        }

        private static void DetachSlots(List<PzSimpleItemSlotUI> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                    slots[i].AttachToSlot(SlotReference.Null);
            }
        }

        private void OnSlotClicked(SlotReference slot)
        {
            if (!slot.IsValid())
                return;

            if (!_selectedSlot.IsValid())
            {
                if (!slot.HasItem())
                    return;

                _selectedSlot = slot;
                _hintLabel.text = "Selected. Click another slot to transfer/swap.";
                return;
            }

            if (_selectedSlot == slot)
            {
                _selectedSlot = SlotReference.Null;
                _hintLabel.text = "Selection cleared.";
                return;
            }

            bool moved = _selectedSlot.TransferOrSwapWithSlot(slot);
            if (!moved && _selectedSlot.Container != slot.Container)
                moved = _selectedSlot.TransferOrSwapWithContainer(slot.Container);

            _hintLabel.text = moved ? "Transferred." : "Transfer blocked by container rules.";
            _selectedSlot = SlotReference.Null;
        }

        private void EnsureUi()
        {
            if (_canvas != null)
                return;

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Panel", typeof(RectTransform)).GetComponent<RectTransform>();
            _root.SetParent(canvasGo.transform, false);
            _root.anchorMin = new Vector2(0.5f, 0.5f);
            _root.anchorMax = new Vector2(0.5f, 0.5f);
            _root.sizeDelta = new Vector2(760f, 520f);
            Image panelBg = _root.gameObject.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.09f, 0.1f, 0.94f);

            _titleLabel = CreateLabel(_root, "Title", new Vector2(0f, 230f), 22);
            _hintLabel = CreateLabel(_root, "Hint", new Vector2(0f, 200f), 14);

            _leftGrid = CreateGrid(_root, "LeftGrid", new Vector2(-190f, -20f));
            _rightGrid = CreateGrid(_root, "RightGrid", new Vector2(190f, -20f));

            _canvas.enabled = false;
            _root.gameObject.SetActive(false);
        }

        private static Text CreateLabel(Transform parent, string name, Vector2 anchoredPos, int size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(700f, 28f);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return text;
        }

        private static Transform CreateGrid(Transform parent, string name, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(320f, 420f);
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 4f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        private static PzSimpleItemSlotUI CreateSlot(Transform parent)
        {
            var go = new GameObject("Slot", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 28f;
            le.preferredHeight = 28f;

            Image bg = go.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.16f, 0.18f, 0.9f);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.SetParent(rt, false);
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(8f, 2f);
            labelRt.offsetMax = new Vector2(-8f, -2f);
            var label = labelGo.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 13;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.raycastTarget = false;

            go.AddComponent<SelectableButton>();
            var slot = go.AddComponent<PzSimpleItemSlotUI>();
            slot.BindVisuals(label, bg);
            return slot;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }
    }
}
