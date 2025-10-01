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

        bool initialized;

        public ExpComponents(BeaverExpComponent exp): this(
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

        public void Initialize()
        {
            if (initialized)
            {
                throw new InvalidOperationException("ExpComponents already initialized");
            }

            initialized = true;

            Enterer.EnteredEnterable += OnEntered;
            Enterer.ExitedEnterable += OnExited;

            var insideBuilding = Enterer.CurrentBuilding;
            IsInside = insideBuilding;
            if (insideBuilding && Worker)
            {
                var workplace = Worker.Workplace;
                IsInsideWorkplace = workplace && insideBuilding == workplace.GetComponentFast<Enterable>();
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            IsInside = false;
            IsInsideWorkplace = false;
        }

        private void OnEntered(object sender, EnteredEnterableEventArgs e)
        {
            IsInside = true;

            var workplace = Worker ? Worker.Workplace : null;
            if (workplace && e.Enterable == workplace.GetComponentFast<Enterable>())
            {
                IsInsideWorkplace = true;
            }
        }

        public void Dispose()
        {
            Enterer.EnteredEnterable -= OnEntered;
            Enterer.ExitedEnterable -= OnExited;
        }
    }
}
