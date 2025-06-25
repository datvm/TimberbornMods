
namespace HydroFormaProjects.Services;

public class DamGateService(
    IBlockService blocks,
    ScientificProjectService projects
)
{

    bool canCloseDam;
    public bool CanCloseDam => canCloseDam || ReloadCloseDam();

    public void ToggleDamGate(DamGateComponent comp, bool closed)
    {
        var blockObj = comp.GetComponentFast<BlockObject>();
        HashSet<Vector3Int> visited = [blockObj.Coordinates];
        Stack<DamGateComponent> stack = new([comp]);

        var neighbors = Deltas.Neighbors4Vector3Int;

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!current.Finished 
                || (!current.Synchronize && current != comp)) { continue; }

            if (current.Closed != closed)
            {
                current.ToggleClosed(closed);
            }

            if (!current.Synchronize) { continue; }

            var block = current.GetComponentFast<BlockObject>();
            var coord = block.Coordinates;
            foreach (var n in neighbors)
            {
                var neighborCoord = coord + n;
                if (visited.Contains(neighborCoord)) { continue; }
                visited.Add(neighborCoord);

                var neighborBlocks = blocks.GetObjectsWithComponentAt<DamGateComponent>(neighborCoord);
                foreach (var b in neighborBlocks)
                {
                    stack.Push(b);
                }
            }
        }
    }

    bool ReloadCloseDam() => canCloseDam = projects.IsUnlocked(HydroFormaModUtils.DamUpgrade);

}
