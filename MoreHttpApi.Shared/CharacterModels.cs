namespace MoreHttpApi.Shared;

public record HttpCharacter(
    HttpEntityModel Entity,
    CharacterType Type,
    HttpNamedEntity Name,
    float Progress,
    Guid? Dwelling,
    Guid? Workplace,
    Guid? District,
    ValueTuple<string, float>[] Bonuses
);

public record HttpPopulation(
    HttpCharacter[] Adult,
    HttpCharacter[] Child,
    HttpCharacter[] Bot
)
{
    public HttpCharacter[] this[CharacterType index] => index switch
    {
        CharacterType.Adult => Adult,
        CharacterType.Child => Child,
        CharacterType.Bot => Bot,
        _ => throw new IndexOutOfRangeException()
    };
}

public enum CharacterType
{
    Adult,
    Child,
    Bot
}