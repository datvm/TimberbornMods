namespace BeavVsMachine.Services;

public partial class BeaverExpStatTracker
{
    record ExpComponents(
        BeaverExpComponent Exp,
        BeaverFitnessComponent Fitness,
        Enterer Enterer,
        NeedManager NeedManager,
        Worker? Worker
    ) : IDisposable
    {
        const string BookNeedId = "Books";
        public static readonly FrozenSet<string> FitnessBuildingPrefabNames = [
            "Lido.Folktails", "DanceHall.Folktails", "PowerWheel.Folktails",
            "SwimmingPool.IronTeeth", "ExercisePlaza.IronTeeth", "LargePowerWheel.IronTeeth",
        ];

        bool initialized;

        public ExpComponents(BeaverExpComponent exp) : this(
            exp,
            exp.GetComponentFast<BeaverFitnessComponent>(),
            exp.GetComponentFast<Enterer>(),
            exp.GetComponentFast<NeedManager>(),
            exp.GetComponentFast<Worker>()
        )
        {
            Initialize();
        }

        public bool HasBook => NeedManager.GetNeedPoints(BookNeedId) > 0;
        public bool IsInside { get; private set; }
        public bool IsInsideWorkplace { get; private set; }
        public bool IsInsideFitnessBuilding { get; private set; }

        public void Initialize()
        {
            if (initialized)
            {
                throw new InvalidOperationException("ExpComponents already initialized");
            }

            initialized = true;

            Enterer.EnteredEnterable += OnEntered;
            Enterer.ExitedEnterable += OnExited;

            SetInsideInfo(Enterer.CurrentBuilding);
        }

        void OnExited(object sender, EventArgs e)
        {
            IsInside = false;
            IsInsideWorkplace = false;
        }

        void OnEntered(object sender, EnteredEnterableEventArgs e)
        {
            SetInsideInfo(e.Enterable);

            IsInside = true;

            var workplace = Worker ? Worker.Workplace : null;
            if (workplace && e.Enterable == workplace.GetComponentFast<Enterable>())
            {
                IsInsideWorkplace = true;
            }

        }

        void SetInsideInfo(Enterable? enterable)
        {
            IsInside = enterable;

            if (IsInside)
            {
                var workplace = Worker ? Worker.Workplace : null;
                IsInsideWorkplace = workplace && enterable == workplace.GetComponentFast<Enterable>();

                var prefabName = enterable!.GetComponentFast<PrefabSpec>().PrefabName;
                IsInsideFitnessBuilding = FitnessBuildingPrefabNames.Contains(prefabName);
            }
            else
            {
                IsInsideWorkplace = false;
                IsInsideFitnessBuilding = false;
            }
        }

        public void Dispose()
        {
            Enterer.EnteredEnterable -= OnEntered;
            Enterer.ExitedEnterable -= OnExited;
        }
    }
}
