namespace TimberQuests;

public readonly record struct TimberQuestStatusChanged(TimberQuestStatusInfo StatusInfo, TimberQuestStatus Previous, TimberQuestStatus New);
public readonly record struct TimberQuestStepStatusChanged(TimberQuestStatusInfo StatusInfo, int StepIndex, TimberQuestStatus Previous, TimberQuestStatus New);