namespace MoreHttpApi.Shared;

public record HttpHomePageInfo(
    HttpGameVersion GameVersion,
    HttpMod[] Mods
);

public record HttpGameVersion(string Full);
public record HttpMod(string Id, string Name, HttpGameVersion Version, string Path, bool Active);