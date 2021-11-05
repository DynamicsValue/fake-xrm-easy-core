using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Metadata;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Metadata;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Permissions;
using FakeXrmEasy.Services;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FakeXrmEasy
{
    /// <summary>
    /// A fake context that stores In-Memory entites indexed by logical name and then Entity records, simulating
    /// how entities are persisted in Tables (with the logical name) and then the records themselves
    /// where the Primary Key is the Guid
    /// </summary>
    public partial class XrmFakedContext : IXrmFakedContext
    {
        internal IMiddlewareBuilder _builder;
        protected internal IOrganizationService _service;

        public IXrmFakedPluginContextProperties PluginContextProperties { get; set; }

        /// <summary>
        /// All proxy type assemblies available on mocked database.
        /// </summary>
        private List<Assembly> _proxyTypesAssemblies { get; set; }
        public IEnumerable<Assembly> ProxyTypesAssemblies 
        {
            get => _proxyTypesAssemblies;
        }

        protected internal bool Initialised { get; set; }

        public Dictionary<string, Dictionary<Guid, Entity>> Data { get; set; }

        [Obsolete("Please use ProxyTypesAssemblies to retrieve assemblies and EnableProxyTypes to add new ones")]
        public Assembly ProxyTypesAssembly
        {
            get
            {
                // TODO What we should do when ProxyTypesAssemblies contains multiple assemblies? One shouldn't throw exceptions from properties.
                return _proxyTypesAssemblies.FirstOrDefault();
            }
            set
            {
                _proxyTypesAssemblies = new List<Assembly>();
                if (value != null)
                {
                    _proxyTypesAssemblies.Add(value);
                }
            }
        }

        /// <summary>
        /// Sets the user to assign the CreatedBy and ModifiedBy properties when entities are added to the context.
        /// All requests will be executed on behalf of this user
        /// </summary>
        [Obsolete("Please use CallerProperties instead")]
        public EntityReference CallerId { get; set; }

        [Obsolete("Please use CallerProperties instead")]
        public EntityReference BusinessUnitId { get; set; }

        public delegate OrganizationResponse ServiceRequestExecution(OrganizationRequest req);

        /// <summary>
        /// Probably should be replaced by FakeMessageExecutors, more generic, which can use custom interfaces rather than a single method / delegate
        /// </summary>
        private Dictionary<Type, ServiceRequestExecution> ExecutionMocks { get; set; }

        private Dictionary<string, XrmFakedRelationship> _relationships { get; set; }
        public IEnumerable<XrmFakedRelationship> Relationships 
        { 
            get => _relationships.Values;
        }
        public IEntityInitializerService EntityInitializerService { get; set; }

        public int MaxRetrieveCount { get; set; }

        public EntityInitializationLevel InitializationLevel { get; set; }

        public ICallerProperties CallerProperties { get; set; }

        private readonly Dictionary<string, object> _properties;
        private readonly IXrmFakedTracingService _fakeTracingService;

        [Obsolete("The default constructor is deprecated. Please use MiddlewareBuilder to build a custom XrmFakedContext")]
        public XrmFakedContext(FakeXrmEasyLicense? license = null)
        {
            LicenseContext = license;

            _fakeTracingService = new XrmFakedTracingService();
            _properties = new Dictionary<string, object>();
            _service = A.Fake<IOrganizationService>();
            _serviceAsync = A.Fake<IOrganizationServiceAsync>();
            _serviceAsync2 = A.Fake<IOrganizationServiceAsync2>();

            _builder = MiddlewareBuilder
                        .New(this)
       
                        // Add* -> Middleware configuration
                        //.AddCrud()   
                        .AddFakeMessageExecutors()

                        // Use* -> Defines pipeline sequence
                        //.UseCrud() 
                        .UseMessages();

            if(LicenseContext != null)
            {
                _builder = _builder.SetLicense(LicenseContext.Value);
            }
                        
            _builder.Build();
            Init();
        }

        internal XrmFakedContext(IMiddlewareBuilder middlewareBuilder) 
        {
            _builder = middlewareBuilder;

            _fakeTracingService = new XrmFakedTracingService();
            _properties = new Dictionary<string, object>();
            _service = A.Fake<IOrganizationService>();
            _serviceAsync = A.Fake<IOrganizationServiceAsync>();
            _serviceAsync2 = A.Fake<IOrganizationServiceAsync2>();
            
            Init();
        }

        private void Init()
        {
            
            CallerProperties = new CallerProperties();
            MaxRetrieveCount = 5000;

            AttributeMetadataNames = new Dictionary<string, Dictionary<string, string>>();
            Data = new Dictionary<string, Dictionary<Guid, Entity>>();
            ExecutionMocks = new Dictionary<Type, ServiceRequestExecution>();

            _relationships = new Dictionary<string, XrmFakedRelationship>();

            EntityInitializerService = new DefaultEntityInitializerService();

            SetProperty<IAccessRightsRepository>(new AccessRightsRepository());
            SetProperty<IOptionSetMetadataRepository>(new OptionSetMetadataRepository());
            SetProperty<IStatusAttributeMetadataRepository>(new StatusAttributeMetadataRepository());

            SystemTimeZone = TimeZoneInfo.Local;

            EntityMetadata = new Dictionary<string, EntityMetadata>();

            UsePipelineSimulation = false;

            InitializationLevel = EntityInitializationLevel.Default;

            _proxyTypesAssemblies = new List<Assembly>();

            GetOrganizationService();
        }

        
        /// <summary>
        /// Checks if this XrmFakedContext has a property of the given type
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <returns></returns>
        public bool HasProperty<T>()
        {
            return _properties.ContainsKey(typeof(T).FullName);
        }
        
        public T GetProperty<T>() 
        {
            if(!_properties.ContainsKey(typeof(T).FullName)) 
            {
                throw new TypeAccessException($"Property of type '{typeof(T).FullName}' doesn't exists");  
            }

            return (T) _properties[typeof(T).FullName];
        }

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
        /// Returns an interface to an organization service that will execute requests according to the middleware setup
        /// </summary>
        /// <returns></returns>
        public IOrganizationService GetOrganizationService()
        {
            return GetFakedOrganizationService(this);
        }

        public IOrganizationServiceFactory GetOrganizationServiceFactory() 
        {
            var fakedServiceFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => fakedServiceFactory.CreateOrganizationService(A<Guid?>._)).ReturnsLazily((Guid? g) => GetOrganizationService());
            return fakedServiceFactory;
        }

        public IXrmFakedTracingService GetTracingService()
        {
            return _fakeTracingService;
        }

        /// <summary>
        /// Initializes the context with the provided entities
        /// </summary>
        /// <param name="entities"></param>
        public virtual void Initialize(IEnumerable<Entity> entities)
        {
            if (Initialised)
            {
                throw new Exception("Initialize should be called only once per unit test execution and XrmFakedContext instance.");
            }

            if (entities == null)
            {
                throw new InvalidOperationException("The entities parameter must be not null");
            }

            foreach (var e in entities)
            {
                AddEntityWithDefaults(e, true);
            }

            Initialised = true;
        }

        public void Initialize(Entity e)
        {
            this.Initialize(new List<Entity>() { e });
        }

        /// <summary>
        /// Enables support for the early-cound types exposed in a specified assembly.
        /// </summary>
        /// <param name="assembly">
        /// An assembly containing early-bound entity types.
        /// </param>
        /// <remarks>
        /// See issue #334 on GitHub. This has quite similar idea as is on SDK method
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.client.organizationserviceproxy.enableproxytypes.
        /// </remarks>
        public void EnableProxyTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (_proxyTypesAssemblies.Contains(assembly))
            {
                throw new InvalidOperationException($"Proxy types assembly { assembly.GetName().Name } is already enabled.");
            }

            _proxyTypesAssemblies.Add(assembly);
        }


        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors")]
        public void AddFakeMessageExecutor<T>(IFakeMessageExecutor executor) where T : OrganizationRequest
        {
            _builder.AddFakeMessageExecutor<T>(executor);
        }

        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors. If you want to remove one, simply remove it from the middleware setup.")]
        public void RemoveFakeMessageExecutor<T>() where T : OrganizationRequest
        {
            _builder.RemoveFakeMessageExecutor<T>();
        }

        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors")]
        public void AddGenericFakeMessageExecutor(string message, IFakeMessageExecutor executor)
        {
            _builder.AddGenericFakeMessageExecutor(message, executor);
        }

        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors. If you want to remove one, simply remove it from the middleware setup.")]
        public void RemoveGenericFakeMessageExecutor(string message)
        {
            if(HasProperty<GenericMessageExecutors>()) 
            {
                var genericMessageExecutors = GetProperty<GenericMessageExecutors>();
                if(genericMessageExecutors.ContainsKey(message))
                {
                    genericMessageExecutors.Remove(message);
                }
            }
        }

        public void AddRelationship(string schemaname, XrmFakedRelationship relationship)
        {
            _relationships.Add(schemaname, relationship);
        }

        public void RemoveRelationship(string schemaname)
        {
            _relationships.Remove(schemaname);
        }

        public XrmFakedRelationship GetRelationship(string schemaName)
        {
            if (_relationships.ContainsKey(schemaName))
            {
                return _relationships[schemaName];
            }

            return null;
        }

        public void AddAttributeMapping(string sourceEntityName, string sourceAttributeName, string targetEntityName, string targetAttributeName)
        {
            if (string.IsNullOrWhiteSpace(sourceEntityName))
                throw new ArgumentNullException("sourceEntityName");
            if (string.IsNullOrWhiteSpace(sourceAttributeName))
                throw new ArgumentNullException("sourceAttributeName");
            if (string.IsNullOrWhiteSpace(targetEntityName))
                throw new ArgumentNullException("targetEntityName");
            if (string.IsNullOrWhiteSpace(targetAttributeName))
                throw new ArgumentNullException("targetAttributeName");

            var entityMap = new Entity
            {
                LogicalName = "entitymap",
                Id = Guid.NewGuid(),
                ["targetentityname"] = targetEntityName,
                ["sourceentityname"] = sourceEntityName
            };

            var attributeMap = new Entity
            {
                LogicalName = "attributemap",
                Id = Guid.NewGuid(),
                ["entitymapid"] = new EntityReference("entitymap", entityMap.Id),
                ["targetattributename"] = targetAttributeName,
                ["sourceattributename"] = sourceAttributeName
            };

            AddEntityWithDefaults(entityMap);
            AddEntityWithDefaults(attributeMap);
        }

        

        /// <summary>
        /// Deprecated. Use GetOrganizationService instead
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use GetOrganizationService instead")]
        public IOrganizationService GetFakedOrganizationService()
        {
            return GetFakedOrganizationService(this);
        }

        protected IOrganizationService GetFakedOrganizationService(XrmFakedContext context)
        {
            return context._service;
        }

    }
}