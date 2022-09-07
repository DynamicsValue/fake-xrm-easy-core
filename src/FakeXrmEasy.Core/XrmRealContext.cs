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
        /// Reference to an actual IOrganizationService
        /// </summary>
        protected IOrganizationService _service;

        /// <summary>
        /// Reference to an IOrganizationService instance without cancellation tokens
        /// </summary>
        protected IOrganizationServiceAsync _serviceAsync;

        /// <summary>
        /// Reference to an IOrganizationService instance with cancellation tokens
        /// </summary>
        protected IOrganizationServiceAsync2 _serviceAsync2;

        private readonly Dictionary<string, object> _properties;

        private readonly string _connectionString;

        /// <summary>
        /// The actual connection string to connect to a Dataverse environment
        /// </summary>
        /// <param name="connectionString"></param>
        public XrmRealContext(string connectionString)
        {
            _properties = new Dictionary<string, object>();
            _connectionString = connectionString;
        }

        /// <summary>
        /// Constructor of an XrmRealContext from previously created instances
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="serviceAsync"></param>
        /// <param name="serviceAsync2"></param>
        public XrmRealContext(IOrganizationService organizationService, IOrganizationServiceAsync serviceAsync = null, IOrganizationServiceAsync2 serviceAsync2 = null)
        {
            _properties = new Dictionary<string, object>();
            _service = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            _serviceAsync = serviceAsync ?? throw new ArgumentNullException(nameof(serviceAsync));
            _serviceAsync2 = serviceAsync2 ?? throw new ArgumentNullException(nameof(serviceAsync2));
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
        /// Returns an IOrganizationService instance that uses the underlying connectionString 
        /// </summary>
        /// <returns></returns>
        public IOrganizationService GetOrganizationService()
        {
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
        /// Execute the code of a plugin locally simulating the execution in a target environment with a serialised, compressed plugin profile execution from that environment
        /// </summary>
        /// <param name="sCompressedProfile">The compressed, serialised, plugin profile execution</param>
        /// <returns></returns>
        public XrmFakedPluginExecutionContext GetContextFromSerialisedCompressedProfile(string sCompressedProfile)
        {
            byte[] data = Convert.FromBase64String(sCompressedProfile);

            using (var memStream = new MemoryStream(data))
            {
                using (var decompressedStream = new DeflateStream(memStream, CompressionMode.Decompress, false))
                {
                    byte[] buffer = new byte[0x1000];

                    using (var tempStream = new MemoryStream())
                    {
                        int numBytesRead = decompressedStream.Read(buffer, 0, buffer.Length);
                        while (numBytesRead > 0)
                        {
                            tempStream.Write(buffer, 0, numBytesRead);
                            numBytesRead = decompressedStream.Read(buffer, 0, buffer.Length);
                        }

                        //tempStream has the decompressed plugin context now
                        var decompressedString = Encoding.UTF8.GetString(tempStream.ToArray());
                        var xlDoc = XDocument.Parse(decompressedString);

                        var contextElement = xlDoc.Descendants().Elements()
                            .Where(x => x.Name.LocalName.Equals("Context"))
                            .FirstOrDefault();

                        var pluginContextString = contextElement.Value;

                        XrmFakedPluginExecutionContext context = null;
                        using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(pluginContextString)))
                        {
                            var dcSerializer = new DataContractSerializer(typeof(XrmFakedPluginExecutionContext));
                            context = (XrmFakedPluginExecutionContext)dcSerializer.ReadObject(reader);
                        }

                        return context;
                    }
                }
            }
        }
    }
}
