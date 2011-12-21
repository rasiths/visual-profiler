namespace VisualProfilerUI.Model.Criteria.SamplingCriteria
{
    public class DurationCriteria : Criterion
    {
        public DurationCriteria()
            : base("Duration")
        {
        }

        public override int Divider { get { return 10000; } }

        public override string Unit { get { return "ms"; } }
    }
}