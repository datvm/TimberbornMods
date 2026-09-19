namespace GlobalInventory.Services;

public partial class GlobalGoodHandle
{
    public class Serializer(GlobalGoodSpecService specs, EventBus eb) : IValueSerializer<GlobalGoodHandle>
    {
        static readonly PropertyKey<string> IdKey = new("Id");
        static readonly PropertyKey<float> AmountKey = new("Amount");
        static readonly PropertyKey<float> MinCapacityKey = new("MinCapacity");
        static readonly PropertyKey<float> MaxCapacityKey = new("MaxCapacity");
        static readonly PropertyKey<bool> RevealedKey = new("Revealed");

        public Obsoletable<GlobalGoodHandle> Deserialize(IValueLoader valueLoader)
        {
            var obj = valueLoader.AsObject();
            var id = obj.Get(IdKey);
            if (!specs.TryGet(id, out var spec))
            {
                return default;
            }

            var handle = new GlobalGoodHandle(spec, eb);
            var min = obj.Has(MinCapacityKey) ? obj.Get(MinCapacityKey) : spec.MinCapacity;
            float? max = obj.Has(MaxCapacityKey) ? obj.Get(MaxCapacityKey) : null;
            var amount = obj.Has(AmountKey) ? obj.Get(AmountKey) : spec.InitialAmount;
            var revealed = obj.Has(RevealedKey) && obj.Get(RevealedKey);
            handle.LoadState(amount, min, max, revealed);
            return handle;
        }

        public void Serialize(GlobalGoodHandle value, IValueSaver valueSaver)
        {
            var obj = valueSaver.AsObject();
            obj.Set(IdKey, value.Spec.Id);
            obj.Set(AmountKey, value.Amount);
            obj.Set(MinCapacityKey, value.MinCapacity);
            if (value.MaxCapacity is float max)
            {
                obj.Set(MaxCapacityKey, max);
            }

            obj.Set(RevealedKey, value.revealed);
        }
    }
}
