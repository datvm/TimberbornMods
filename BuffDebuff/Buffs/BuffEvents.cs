namespace BuffDebuff;

public readonly record struct BuffAddedToEntityEvent(BuffInstance BuffInstance, BuffableComponent Buffable);
public readonly record struct BuffRemovedFromEntityEvent(BuffInstance BuffInstance, BuffableComponent Buffable);
public readonly record struct BuffInstanceAppliedEvent(BuffInstance BuffInstance);
public readonly record struct BuffInstanceRemovedEvent(BuffInstance BuffInstance);
public readonly record struct BuffInstanceActivatedEvent(BuffInstance BuffInstance);
public readonly record struct BuffInstanceDeactivatedEvent(BuffInstance BuffInstance);