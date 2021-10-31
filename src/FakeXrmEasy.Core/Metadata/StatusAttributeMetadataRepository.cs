
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions.Metadata;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Metadata
{
    /// <summary>
    /// This repository contains metadata for the global option sets
    /// </summary>
    public class StatusAttributeMetadataRepository : IStatusAttributeMetadataRepository
    {
        private readonly Dictionary<string, StatusAttributeMetadata> _repository;

        public StatusAttributeMetadataRepository()
        {
            _repository = new Dictionary<string, StatusAttributeMetadata>();
        }

        public IQueryable<StatusAttributeMetadata> CreateQuery()
        {
            return _repository.Values.AsQueryable();
        }

        public StatusAttributeMetadata GetByAttributeName(string entityName, string attributeName)
        {
            var key = GetAttributeKey(entityName, attributeName);
            return GetFromKey(key);
        }

        public StatusAttributeMetadata GetByGlobalOptionSetName(string name)
        {
            var key = GetOptionSetKey(name);
            return GetFromKey(key);
        }

        public void Set(string globalOptionSetName, StatusAttributeMetadata metadata)
        {
            var key = GetOptionSetKey(globalOptionSetName);
            AddOrSet(key, metadata);
        }

        public void Set(string entityName, string attributeName, StatusAttributeMetadata metadata)
        {
            var key = GetAttributeKey(entityName, attributeName);
            AddOrSet(key, metadata);
        }

        protected void AddOrSet(string key, StatusAttributeMetadata metadata)
        {
            if(_repository.ContainsKey(key))
            {
                _repository.Add(key, metadata);
            }
            else 
            {
                _repository[key] = metadata;
            }
        }

        protected StatusAttributeMetadata GetFromKey(string key)
        {
            if(_repository.ContainsKey(key))
            {
               return _repository[key];
            }
            
            return null;
        }

        protected string GetOptionSetKey(string name) 
        {
            return name;
        }

        protected string GetAttributeKey(string entityName, string attributeName)
        {
            return $"{entityName}#{attributeName}";
        }
    }
}