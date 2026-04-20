namespace DistroStorage.Services;

public record DistroSenderSerializableModel(bool Enabled);
public record DistroReceiverSerializableModel(bool Enabled, Priority Priority);