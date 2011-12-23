namespace VisualProfilerUI.Model.Criteria.SamplingCriteria
{
    public class TopStackOccurrenceCriteria : Criterion
    {
        public TopStackOccurrenceCriteria()
            : base("Top stack occurrence")
        {
        }

        public override int Divider
        {
            get { return 1; }
        }

        public override string Unit
        {
            get { return "call"; }
        }
    }
}