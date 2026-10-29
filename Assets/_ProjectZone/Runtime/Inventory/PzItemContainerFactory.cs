using System;
using PolymindGames.InventorySystem;

namespace ProjectZone
{
    /// <summary>
    /// Safe HQ container creation — generator arrays can be null after construction.
    /// </summary>
    internal static class PzItemContainerFactory
    {
        public static ItemContainer Create(IInventory inventory, ItemContainerGenerator generator, bool populateWithItems)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            Normalize(generator);
            return (ItemContainer)generator.GenerateContainer(inventory, populateWithItems);
        }

        public static void Normalize(ItemContainerGenerator generator)
        {
            if (generator == null)
                return;

            generator.Restrictions ??= Array.Empty<ContainerRestriction>();
            generator.PredefinedItems ??= Array.Empty<ItemGenerator>();
        }

        public static ContainerRestriction[] RestrictionsOrEmpty(ItemContainerGenerator generator)
            => generator?.Restrictions ?? Array.Empty<ContainerRestriction>();
    }
}
