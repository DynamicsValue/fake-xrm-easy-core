using FakeXrmEasy.Abstractions.Integrity;

namespace FakeXrmEasy.Integrity
{
    /// <summary>
    /// 
    /// </summary>
    public class IntegrityOptions : IIntegrityOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public bool ValidateEntityReferences { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IntegrityOptions()
        {
            ValidateEntityReferences = true;
        }
    }
}
