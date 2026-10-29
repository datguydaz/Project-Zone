using PolymindGames.InventorySystem;
using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZone
{
    /// <summary>
    /// Lightweight greybox slot UI over HQ SlotReference (no custom inventory model).
    /// </summary>
    [RequireComponent(typeof(SelectableButton))]
    public sealed class PzSimpleItemSlotUI : ItemSlotUIBase
    {
        private Text _label;
        private Image _background;

        public void BindVisuals(Text label, Image background)
        {
            _label = label;
            _background = background;
        }

        protected override void UpdateUI(in ItemStack stack, SlotChangeType changeType)
        {
            if (_label == null)
                return;

            if (!stack.HasItem())
            {
                _label.text = "—";
                if (_background != null)
                    _background.color = new Color(0.15f, 0.16f, 0.18f, 0.9f);
                return;
            }

            string itemName = stack.Item.Name;
            _label.text = stack.Count > 1 ? $"{itemName} x{stack.Count}" : itemName;
            if (_background != null)
                _background.color = new Color(0.22f, 0.28f, 0.24f, 0.95f);
        }
    }
}
