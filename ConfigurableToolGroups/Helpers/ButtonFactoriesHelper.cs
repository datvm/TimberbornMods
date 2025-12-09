namespace Timberborn.ToolButtonSystem;

public static class ButtonFactoriesHelper
{

    extension(ToolButtonFactory factory)
    {

        public Sprite LoadImage(string imageName)
            => factory._assetLoader.Load<Sprite>(Path.Combine(ToolButtonFactory.ImageDirectory, imageName));

        public ToolButton CreateGroupless(ITool tool, string name, RootToolButtonColor color) => color switch
        {
            RootToolButtonColor.Blue => factory.CreateGrouplessBlue(tool, name),
            RootToolButtonColor.Green => factory.CreateGrouplessGreen(tool, factory.LoadImage(name)),
            RootToolButtonColor.Red => factory.CreateGrouplessRed(tool, name),
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, $"Unknown color: {color}"),
        };

    }

    extension(ToolGroupButtonFactory factory)
    {
        public ToolGroupButton Create(ToolGroupSpec spec, ToolButtonColor color) => color switch
        {
            ToolButtonColor.Blue => factory.CreateBlue(spec),
            ToolButtonColor.Green => factory.CreateGreen(spec),
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, $"Unknown color: {color}"),
        };
    }

    extension(BlockObjectToolGroupSpec spec)
    {
        public ToolGroupSpec ToToolGroupSpec()
            => BlockObjectToolGroupButtonFactory.CreateBlueprint(spec).GetSpec<ToolGroupSpec>();
    }

}
