namespace ModdableTimberborn.Helpers;

public static class AutomationExtensions
{

    extension(AutomatorConnection conn)
    {

        public void TryConnecting(Guid? id, EntityRegistry entityRegistry)
            => conn.TryConnecting(entityRegistry.TryGetAutomator(id));

        public void TryConnecting(EntityComponent? value) 
            => conn.TryConnecting(value?.GetComponent<Automator>());

        public void TryConnecting(Automator? automator)
        {
            conn.Connect(automator);
        }
    }

}
