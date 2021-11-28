
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions.Metadata;
using Microsoft.Xrm.Sdk.Metadata;

namespace FakeXrmEasy.Metadata
{
    /// <summary>
    /// OptionSetMetadata Repository
    /// </summary>
    public class OptionSetMetadataRepository : IOptionSetMetadataRepository
    {
        private readonly Dictionary<string, OptionSetMetadata> _repository;

        /// <summary>
        /// 
        /// </summary>
        public OptionSetMetadataRepository()
        {
            _repository = new Dictionary<string, OptionSetMetadata>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<OptionSetMetadata> CreateQuery()
        {
            return _repository.Values.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sGlobalOptionSetName"></param>
        /// <returns></returns>
        public OptionSetMetadata GetByName(string sGlobalOptionSetName)
        {
            if(_repository.ContainsKey(sGlobalOptionSetName))
            {
                return _repository[sGlobalOptionSetName];
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sGlobalOptionSetName"></param>
        /// <param name="metadata"></param>
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