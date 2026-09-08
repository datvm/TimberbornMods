global using BuffDebuffDemo.Buffs;
global using Timberborn.Beavers;

namespace BuffDebuffDemo;

// Register these under the Game context
[Context("Game")]
public class GameConfig : Configurator
{
    public override void Configure()
    {
        // Buffs should be singleton
        Bind<PositiveBuff>().AsSingleton();
        Bind<NegativeBuff>().AsSingleton();
        Bind<LuckyBuff>().AsSingleton();

        // Decorator Component for the Beavers as the recommended way to process the Buffs
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BeaverSpec, BeaverBuffComponent>();
            return b.Build();
        }).AsSingleton();
    }
}
