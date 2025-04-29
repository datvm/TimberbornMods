namespace TimberQuests;

public interface ITimberQuestUpdater
{
    void UpdateQuest(int ticksPassed);
}

public interface ITimberQuestUrgentUpdater : ITimberQuestUpdater
{
    void UpdateQuestUrgent(int ticksPassed);
}
