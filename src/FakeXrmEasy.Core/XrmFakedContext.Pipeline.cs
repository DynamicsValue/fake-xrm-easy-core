using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        public bool UsePipelineSimulation { get; set; }

        
    }
}