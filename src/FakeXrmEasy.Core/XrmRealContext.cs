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
using Microsoft.PowerPlatform.Dataverse.Client;
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
        /// The current license context
        /// </summary>
        public FakeXrmEasyLicense? LicenseContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Reference to an actual IOrganizationService
        /// </summary>
        protected IOrganizationService _service;

        /// <summary>
        /// Reference to an IOrganizationService instance without cancellation tokens
        /// </summary>
        protected IOrganizationServiceAsync _serviceAsync;

        /// <summary>
        /// Use these user to impersonate calls
        /// </summary>
        public ICallerProperties CallerProperties { get; set; }

        /// <summary>
        /// Returns the default plugin context properties
        /// </summary>
        public IXrmFakedPluginContextProperties PluginContextProperties { get; set; }

        /// <summary>
        /// Internal reference to an IOrganizationService.
        /// </summary>
        protected IOrganizationServiceAsync2 _serviceAsync2;

        /// <summary>
        /// A fake tracing service if one is needed
        /// </summary>
        private IXrmFakedTracingService _fakeTracingService;

        private Dictionary<string, object> _properties;

        /// <summary>
        /// A constructor that will use connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public XrmRealContext(string connectionString)
        {
            _connectionString = connectionString;
            Init();
        }

        /// <summary>
        /// Creates an XrmRealContext that uses the specified IOrganizationService interface
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="serviceAsync"></param>
        /// <param name="serviceAsync2"></param>
        public XrmRealContext(IOrganizationService organizationService, IOrganizationServiceAsync serviceAsync = null, IOrganizationServiceAsync2 serviceAsync2 = null)
        {
            _service = organizationService;
            _serviceAsync = serviceAsync;
            _serviceAsync2 = serviceAsync2;
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
        /// Returns true if the property exists in this XrmRealContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasProperty<T>()
        {
            return _properties.ContainsKey(typeof(T).FullName);
        }
        
        /// <summary>
        /// Returns a property from this XrmRealContext
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
        /// Sets a property to this XrmRealContext
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
        /// Returns an IOrganizationServiceAsync instance that uses the underlying connectionString, without cancellation tokens
        /// </summary>
        /// <returns></returns>
        public IOrganizationServiceAsync GetAsyncOrganizationService()
        {
            if (_serviceAsync != null)
                return _serviceAsync;

            _serviceAsync = GetOrgService();
            return _serviceAsync;
        }

        /// <summary>
        /// Returns an IOrganizationServiceAsync instance that uses the underlying connectionString, with cancellation tokens
        /// </summary>
        /// <returns></returns>
        public IOrganizationServiceAsync2 GetAsyncOrganizationService2()
        {
            if (_serviceAsync2 != null)
                return _serviceAsync2;

            _serviceAsync2 = GetOrgService();
            return _serviceAsync2;
        }

        /// <summary>
        /// Internal method to retrieve an instance of an organization service
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected ServiceClient GetOrgService()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new Exception("The ConnectionString property is null or empty");
            }

            // Connect to the Dataverse with a connection string.
            var client = new ServiceClient(_connectionString);
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
