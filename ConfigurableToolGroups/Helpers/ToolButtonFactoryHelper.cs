namespace Timberborn.ToolButtonSystem;

public static class ToolButtonFactoryHelper
{

    extension(ToolButtonFactory factory)
    {
    
        public Sprite LoadImage(string imageName)        
            => factory._assetLoader.Load<Sprite>(Path.Combine(ToolButtonFactory.ImageDirectory, imageName));

    }

}
