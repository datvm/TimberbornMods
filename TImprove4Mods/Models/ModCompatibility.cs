namespace TImprove4Mods.Models;

public class ModCompatibility
{
    public ObsoleteMod? Obsolete { get; init; }
    public ImmutableDictionary<string, IncompatibleMod>? Incompatibles { get; init; }
}

public interface IModIssue
{
    string? Note { get; }
}
public readonly record struct ModIssue(Mod Mod, IModIssue Issue);

public readonly record struct IncompatibleModIssue(Mod IncompatibleMod, IncompatibleMod Data) : IModIssue
{
    public string? Note => Data.Note;
}

public class IncompatibleMod
{

    public IncompatibleReason Reason { get; init; }
    public string? Note { get; init; }

}

public enum IncompatibleReason
{
    Unknown,
    Replace,
    Conflict,
}

public class ObsoleteMod : IModIssue
{
    public ReplacementMod? Replacement { get; init; }
    public string? Note { get; init; }
}

public class ReplacementMod
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Url { get; init; }
}