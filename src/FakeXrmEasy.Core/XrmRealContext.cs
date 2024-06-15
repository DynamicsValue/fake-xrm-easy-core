using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.Configuration;
using System.IO;

using System.Xml.Linq;
using System.Linq;

using System.IO.Compression;
using System.Runtime.Serialization;

using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Exceptions;

#if FAKE_XRM_EASY_NETCORE
using Microsoft.Powerplatform.Cds.Client;
#elif FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using Microsoft.Xrm.Tooling.Connector;
#else 
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
#endif

namespace FakeXrmEasy
{
    /// <summary>
    /// Reuse unit test syntax to test against a real CRM organisation
    /// It uses a real CRM organisation service instance
    /// </summary>
    public class XrmRealContext : IXrmRealContext
    {
        /// <summary>
        /// 
        /// </summary>
        public FakeXrmEasyLicense? LicenseContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionStringName { get; set; } = "fakexrmeasy-connection";

        /// <summary>
        /// Use these user to impersonate calls
        /// </summary>
        public ICallerProperties CallerProperties { get; set; }

        /// <summary>
        /// Plugin Context Properties
        /// </summary>
        public IXrmFakedPluginContextProperties PluginContextProperties { get; set; }

        /// <summary>
        /// Internal reference to an IOrganizationService.
        /// </summary>
        protected IOrganizationService _service;

        /// <summary>
        /// A fake tracing service if one is needed
        /// </summary>
        private IXrmFakedTracingService _fakeTracingService;

        private Dictionary<string, object> _properties;

        /// <summary>
        /// A default constructor that will use a connection string with name fakexrmeasy-connection to establish a real connection to an environment for integration testing purposes
        /// </summary>
        public XrmRealContext()
        {
            Init();
        }

        /// <summary>
        /// A constructor that will use a different connection string name
        /// </summary>
        /// <param name="connectionStringName"></param>
        public XrmRealContext(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
            Init();
        }

        /// <summary>
        /// Creates an XrmRealContext that uses the specified IOrganizationService interface
        /// </summary>
        /// <param name="organizationService"></param>
        public XrmRealContext(IOrganizationService organizationService)
        {
            _service = organizationService;
            Init();
        }

        /// <summary>
        /// Initializes common properties across different constructors
        /// </summary>
        private void Init()
        {
            _properties = new Dictionary<string, object>();
            _fakeTracingService = new XrmFakedTracingService();
            CallerProperties = new CallerProperties();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasProperty<T>()
        {
            return _properties.ContainsKey(typeof(T).FullName);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public T GetProperty<T>() 
        {
            if(!_properties.ContainsKey(typeof(T).FullName)) 
            {
                throw new TypeAccessException($"Property of type '{typeof(T).FullName}' doesn't exists");  
            }

            return (T) _properties[typeof(T).FullName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        public void SetProperty<T>(T property) 
        {
            if(!_properties.ContainsKey(typeof(T).FullName)) 
            {
                _properties.Add(typeof(T).FullName, property);
            }
            else 
            {
                _properties[typeof(T).FullName] = property;
            }
        }

        /// <summary>
        /// Returns the internal organization service reference
        /// </summary>
        /// <returns></returns>
        public IOrganizationService GetOrganizationService()
        {
            if (LicenseContext == null)
            {
                throw new LicenseException("Please, you need to choose a FakeXrmEasy license. More info at https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/licensing-exception/");
            }

            if (_service != null)
                return _service;

            _service = GetOrgService();
            return _service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IOrganizationService GetOrgService()
        {
            var connection = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            // In case of missing connection string in configuration,
            // use ConnectionStringName as an explicit connection string
            var connectionString = connection == null ? ConnectionStringName : connection.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("The ConnectionStringName property must be either a connection string or a connection string name");
            }

            // Connect to the CRM web service using a connection string.
#if FAKE_XRM_EASY_NETCORE
            var client = new CdsServiceClient(connectionString);
#elif FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            var client = new CrmServiceClient(connectionString);
#else
            CrmConnection crmConnection = CrmConnection.Parse(connectionString);
            var client = new OrganizationService(crmConnection);
#endif
            return client;
        }

        /// <summary>
        /// Returns a default ITracingService that will store all traces In-Memory
        /// </summary>
        /// <returns></returns>
        public IXrmFakedTracingService GetTracingService()
        {
            return _fakeTracingService;
        }
    }
}
