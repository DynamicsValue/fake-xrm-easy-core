
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

        /// <summary>
        /// 
        /// </summary>
        public StatusAttributeMetadataRepository()
        {
            _repository = new Dictionary<string, StatusAttributeMetadata>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<StatusAttributeMetadata> CreateQuery()
        {
            return _repository.Values.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public StatusAttributeMetadata GetByAttributeName(string entityName, string attributeName)
        {
            var key = GetAttributeKey(entityName, attributeName);
            return GetFromKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StatusAttributeMetadata GetByGlobalOptionSetName(string name)
        {
            var key = GetOptionSetKey(name);
            return GetFromKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalOptionSetName"></param>
        /// <param name="metadata"></param>
        public void Set(string globalOptionSetName, StatusAttributeMetadata metadata)
        {
            var key = GetOptionSetKey(globalOptionSetName);
            AddOrSet(key, metadata);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="metadata"></param>
        public void Set(string entityName, string attributeName, StatusAttributeMetadata metadata)
        {
            var key = GetAttributeKey(entityName, attributeName);
            AddOrSet(key, metadata);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="metadata"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected StatusAttributeMetadata GetFromKey(string key)
        {
            if(_repository.ContainsKey(key))
            {
               return _repository[key];
            }
            
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string GetOptionSetKey(string name) 
        {
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        protected string GetAttributeKey(string entityName, string attributeName)
        {
            return $"{entityName}#{attributeName}";
        }
    }
}