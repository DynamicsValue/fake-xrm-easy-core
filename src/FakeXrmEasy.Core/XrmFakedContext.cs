using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Metadata;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Permissions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Core.Db;
using FakeXrmEasy.Metadata;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Permissions;
using FakeXrmEasy.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System.Runtime.CompilerServices;
using FakeXrmEasy.Abstractions.CommercialLicense;

[assembly: InternalsVisibleTo("FakeXrmEasy.Core.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c124cb50761165a765adf6078bde555a7c5a2b692ed6e6ec9df0bd7d20da69170bae9bf95e874fa50995cc080af404ccad36515fa509c4ea6599a0502c1642db254a293e023c47c79ce69889c6ba921d124d896d87f0baaa9ea1d87b28589ffbe7b08492606bacef19dc4bc4cefb0d525be63ee722b02dc8c79688a7a8f623a2")]

namespace FakeXrmEasy
{
    /// <summary>
    /// A fake context that stores In-Memory entites indexed by logical name and then Entity records, simulating
    /// how entities are persisted in Tables (with the logical name) and then the records themselves
    /// where the Primary Key is the Guid
    /// </summary>
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// Internal middleware setup
        /// </summary>
        internal IMiddlewareBuilder _builder;

        /// <summary>
        /// 
        /// </summary>
        protected internal IOrganizationService _service;

        /// <summary>
        /// 
        /// </summary>
        public IXrmFakedPluginContextProperties PluginContextProperties { get; set; }

        /// <summary>
        /// All proxy type assemblies available on mocked database.
        /// </summary>
        private List<Assembly> _proxyTypesAssemblies { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Assembly> ProxyTypesAssemblies 
        {
            get => _proxyTypesAssemblies;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal bool Initialised { get; set; }

        /// <summary>
        /// Internal In-Memory Database
        /// </summary>
        internal InMemoryDb Db { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Please use ProxyTypesAssemblies to retrieve assemblies and EnableProxyTypes to add new ones. This method is solely maintained for making a smoother transition to the latest versions from v1")]
        public Assembly ProxyTypesAssembly
        {
            get
            {
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

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Please use CallerProperties instead")]
        public EntityReference BusinessUnitId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public delegate OrganizationResponse ServiceRequestExecution(OrganizationRequest req);

        private Dictionary<string, XrmFakedRelationship> _relationships { get; set; }

        /// <summary>
        /// Relationships
        /// </summary>
        public IEnumerable<XrmFakedRelationship> Relationships 
        { 
            get => _relationships.Values;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEntityInitializerService EntityInitializerService { get; set; }

        /// <summary>
        /// Default max count value when retrieving data, defaults to 5000
        /// </summary>
        public int MaxRetrieveCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public EntityInitializationLevel InitializationLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICallerProperties CallerProperties { get; set; }

        private readonly Dictionary<string, object> _properties;
        private readonly IXrmFakedTracingService _fakeTracingService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="license"></param>
        [Obsolete("The default constructor is deprecated. Please use MiddlewareBuilder to build a custom XrmFakedContext")]
        public XrmFakedContext(FakeXrmEasyLicense? license = null)
        {
            LicenseContext = license;

            _fakeTracingService = new XrmFakedTracingService();
            _properties = new Dictionary<string, object>();
            _service = A.Fake<IOrganizationService>();

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
            
            Init();
        }

        private void Init()
        {
            CallerProperties = new CallerProperties();
            MaxRetrieveCount = 5000;

            AttributeMetadataNames = new Dictionary<string, Dictionary<string, string>>();
            Db = new InMemoryDb();

            _relationships = new Dictionary<string, XrmFakedRelationship>();

            EntityInitializerService = new DefaultEntityInitializerService();

            SetProperty<IAccessRightsRepository>(new AccessRightsRepository());
            SetProperty<IOptionSetMetadataRepository>(new OptionSetMetadataRepository());
            SetProperty<IStatusAttributeMetadataRepository>(new StatusAttributeMetadataRepository());

            SystemTimeZone = TimeZoneInfo.Local;

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
        /// Returns an interface to an organization service that will execute requests according to the middleware setup
        /// </summary>
        /// <returns></returns>
        public IOrganizationService GetOrganizationService()
        {
            return GetFakedOrganizationService(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IOrganizationServiceFactory GetOrganizationServiceFactory() 
        {
            var fakedServiceFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => fakedServiceFactory.CreateOrganizationService(A<Guid?>._)).ReturnsLazily((Guid? g) => GetOrganizationService());
            return fakedServiceFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Initializes the context with a single entity record
        /// </summary>
        /// <param name="entity">Entity record that will be used to initialize the In-Memory context</param>
        public void Initialize(Entity entity)
        {
            this.Initialize(new List<Entity>() { entity });
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="executor"></param>
        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors")]
        public void AddFakeMessageExecutor<T>(IFakeMessageExecutor executor) where T : OrganizationRequest
        {
            _builder.AddFakeMessageExecutor<T>(executor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors. If you want to remove one, simply remove it from the middleware setup.")]
        public void RemoveFakeMessageExecutor<T>() where T : OrganizationRequest
        {
            _builder.RemoveFakeMessageExecutor<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="executor"></param>
        [Obsolete("Please use MiddlewareBuilder's functionality to set custom message executors")]
        public void AddGenericFakeMessageExecutor(string message, IFakeMessageExecutor executor)
        {
            _builder.AddGenericFakeMessageExecutor(message, executor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaname"></param>
        /// <param name="relationship"></param>
        public void AddRelationship(string schemaname, XrmFakedRelationship relationship)
        {
            _relationships.Add(schemaname, relationship);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaname"></param>
        public void RemoveRelationship(string schemaname)
        {
            _relationships.Remove(schemaname);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public XrmFakedRelationship GetRelationship(string schemaName)
        {
            if (_relationships.ContainsKey(schemaName))
            {
                return _relationships[schemaName];
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceEntityName"></param>
        /// <param name="sourceAttributeName"></param>
        /// <param name="targetEntityName"></param>
        /// <param name="targetAttributeName"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected IOrganizationService GetFakedOrganizationService(XrmFakedContext context)
        {
            return context._service;
        }

        /// <summary>
        /// Creates a new entity record that is consistent with the current use of early-bound or late-bound entities by the current context
        /// </summary>
        /// <param name="logicalName">The entity logical name of the entity</param>
        /// <returns>An early-bound record dif the context is already using early-bound entity records, a late bound entity otherwise</returns>
        public Entity NewEntityRecord(string logicalName)
        {
            if (_proxyTypesAssemblies.Any())
            {                
                var subClassType = FindReflectedType(logicalName);
                if (subClassType != null)
                {
                    var instance = Activator.CreateInstance(subClassType);
                    return (Entity) instance;                    
                }
            }
            
            return new Entity
            {
                LogicalName = logicalName,
                Id = Guid.Empty
            };
        }

    }
}