using FakeXrmEasy.Abstractions.Integrity;

namespace FakeXrmEasy.Integrity
{
    public class IntegrityOptions : IIntegrityOptions
    {
        public bool ValidateEntityReferences { get; set; }

        public IntegrityOptions()
        {
            ValidateEntityReferences = true;
        }
    }
}
