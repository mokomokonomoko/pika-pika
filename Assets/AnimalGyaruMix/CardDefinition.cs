using UnityEngine;

namespace AnimalGyaruMix
{
    public enum CardSlot { Hair, Face, Ears, Tail, Top, Bottom, Accessory, Shoes }
    public enum CardRarity { Common, Rare, SuperRare }

    // One asset per physical card. Print the card ID in its QR code.
    [CreateAssetMenu(menuName = "Animal Gyaru Mix/Card")]
    public sealed class CardDefinition : ScriptableObject
    {
        [SerializeField] private string cardId;
        [SerializeField] private string displayName;
        [SerializeField] private CardSlot slot;
        [SerializeField] private CardRarity rarity;
        [SerializeField] private string animal;
        [SerializeField] private Sprite sprite;

        public string Id => cardId;
        public string DisplayName => displayName;
        public CardSlot Slot => slot;
        public CardRarity Rarity => rarity;
        public string Animal => animal;
        public Sprite Sprite => sprite;
        public string RarityName => Rarity == CardRarity.SuperRare ? "Super Rare" : Rarity.ToString();
    }
}
