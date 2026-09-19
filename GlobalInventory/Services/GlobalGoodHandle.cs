namespace GlobalInventory.Services;

public partial class GlobalGoodHandle
{
    readonly EventBus eb;

    bool revealed;

    public GlobalGoodHandle(GlobalGoodSpec spec, EventBus eb)
    {
        Spec = spec;
        this.eb = eb;
        MinCapacity = spec.MinCapacity;
        MaxCapacity = spec.MaxCapacity;
        Amount = Clamp(spec.InitialAmount);
        revealed = Amount != 0;
    }

    public GlobalGoodSpec Spec { get; }
    public float Amount { get; private set; }
    public float MinCapacity { get; private set; }
    public float? MaxCapacity { get; private set; }
    public bool IsVisible => revealed && !(Spec.HideOnZero && Amount == 0);
    internal bool Revealed => revealed;

    public float? FillRate
    {
        get
        {
            if (MaxCapacity is not float max || max == 0)
            {
                return null;
            }

            var span = max - MinCapacity;
            if (span == 0)
            {
                return null;
            }

            return (Amount - MinCapacity) / span;
        }
    }

    public void Add(float delta)
    {
        if (delta == 0)
        {
            return;
        }

        Set(Amount + delta);
    }

    public void Remove(float delta) => Add(-delta);

    public float AddSafe(float delta)
    {
        if (delta == 0)
        {
            return 0;
        }

        var previous = Amount;
        SetSafe(Amount + delta);
        return Amount - previous;
    }

    public float RemoveSafe(float delta) => -AddSafe(-delta);

    public void Set(float amount)
    {
        EnsureInRange(amount);
        ApplyAmount(amount);
    }

    public float SetSafe(float amount)
    {
        ApplyAmount(Clamp(amount));
        return Amount;
    }

    public void SetCapacity(float min, float? max)
    {
        EnsureCapacityRange(min, max);
        if (!IsInRange(Amount, min, max))
        {
            throw RangeError(Amount, min, max);
        }

        ApplyCapacity(min, max);
    }

    public void SetCapacitySafe(float min, float? max)
    {
        EnsureCapacityRange(min, max);
        ApplyCapacity(min, max);
        ApplyAmount(Clamp(Amount));
    }

    public void Hide()
    {
        if (Amount != 0)
        {
            return;
        }

        SetRevealed(false);
    }

    internal void LoadState(float amount, float min, float? max, bool wasRevealed)
    {
        EnsureCapacityRange(min, max);
        MinCapacity = min;
        MaxCapacity = max;
        Amount = Clamp(amount);
        revealed = wasRevealed || Amount != 0;
        if (Spec.HideOnZero && Amount == 0)
        {
            revealed = false;
        }
    }

    void ApplyAmount(float amount)
    {
        if (amount == Amount)
        {
            return;
        }

        var previous = Amount;
        var previousVisible = IsVisible;
        Amount = amount;
        if (amount != previous)
        {
            revealed = true;
        }

        if (Spec.HideOnZero && Amount == 0)
        {
            revealed = false;
        }

        eb.Post(new GlobalGoodAmountChangedEvent(this, previous, Amount, Amount - previous));
        PostVisibility(previousVisible);
    }

    void ApplyCapacity(float min, float? max)
    {
        if (min == MinCapacity && max == MaxCapacity)
        {
            return;
        }

        MinCapacity = min;
        MaxCapacity = max;
        eb.Post(new GlobalGoodCapacityChangedEvent(this, min, max));
    }

    void SetRevealed(bool value)
    {
        var previousVisible = IsVisible;
        revealed = value;
        PostVisibility(previousVisible);
    }

    void PostVisibility(bool previousVisible)
    {
        if (previousVisible == IsVisible)
        {
            return;
        }

        eb.Post(new GlobalGoodVisibilityChangedEvent(this, IsVisible));
    }

    float Clamp(float amount)
    {
        if (amount < MinCapacity)
        {
            return MinCapacity;
        }

        if (MaxCapacity is float max && amount > max)
        {
            return max;
        }

        return amount;
    }

    void EnsureInRange(float amount)
    {
        if (!IsInRange(amount, MinCapacity, MaxCapacity))
        {
            throw RangeError(amount, MinCapacity, MaxCapacity);
        }
    }

    static void EnsureCapacityRange(float min, float? max)
    {
        if (max is float m && m < min)
        {
            throw new InvalidOperationException($"MaxCapacity {m} is below MinCapacity {min}.");
        }
    }

    static bool IsInRange(float amount, float min, float? max)
    {
        if (amount < min)
        {
            return false;
        }

        if (max is float m && amount > m)
        {
            return false;
        }

        return true;
    }

    InvalidOperationException RangeError(float amount, float min, float? max)
    {
        var maxText = max is float m ? m.ToString() : "unlimited";
        return new InvalidOperationException(
            $"Global good '{Spec.Id}' amount {amount} is outside [{min}, {maxText}].");
    }
}
