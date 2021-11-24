using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Use Pipeline Simulation
        /// </summary>
        public bool UsePipelineSimulation { get; set; }   
    }
}