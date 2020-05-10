
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions.Metadata;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Metadata
{
    public class OptionSetMetadataRepository : IOptionSetMetadataRepository
    {
        private readonly Dictionary<string, OptionSetMetadata> _repository;

        public OptionSetMetadataRepository()
        {
            _repository = new Dictionary<string, OptionSetMetadata>();
        }
        public IQueryable<OptionSetMetadata> CreateQuery()
        {
            return _repository.Values.AsQueryable();
        }

        public OptionSetMetadata GetByName(string sGlobalOptionSetName)
        {
            if(_repository.ContainsKey(sGlobalOptionSetName))
            {
                return _repository[sGlobalOptionSetName];
            }

            return null;
        }

        public void Set(string sGlobalOptionSetName, OptionSetMetadata metadata)
        {
            if(!_repository.ContainsKey(sGlobalOptionSetName)) 
            {
                _repository.Add(sGlobalOptionSetName, metadata);
            }
            else 
            {
                _repository[sGlobalOptionSetName] = metadata;
            }
        }
    }
}