namespace ScientificProjects.Management;

public record class OnScientificProjectUnlockedEvent(ScientificProjectSpec Project);
public record class OnScientificProjectLevelChangeEvent(ScientificProjectSpec Project, int Level);