namespace MoreHttpApi.Shared;

public record HttpCharacter(
    HttpEntityModel Entity,
    CharacterType Type,
    string Name,
    string? ImagePath,
    int Age,
    float Progress,
    Guid? Dwelling,
    Guid? Workplace,
    Guid? District,
    int Wellbeing,
    ValueTuple<string, float>[] Bonuses
);

public record HttpCharacterDetailed(
    HttpCharacter Basic,
    Dictionary<Guid, HttpCharacterBuilding> Buildings,
    HttpCharacterNeed[] Needs,
    ParsedGoodAmountSpec? CarryingGood,
    int LiftingCapacity
);

public record HttpCharacterNeed(
    string Id,
    float Points,
    bool Enabled,
    int Wellbeing
);

public record HttpCharacterBuilding(
    Guid Id,
    string? ImagePath,
    string Name,
    string LabelName
);

public record HttpPopulation(
    HttpCharacter[] Adult,
    HttpCharacter[] Child,
    HttpCharacter[] Bot,
    Dictionary<Guid, HttpCharacterBuilding> RelevantBuildings
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
    Unknown,
    Adult,
    Child,
    Bot,    
}