using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Extensions.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy
{
    /// <summary>
    /// 
    /// </summary>
    public partial class XrmFakedContext : IXrmFakedContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logicalName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Type FindReflectedType(string logicalName)
        {
            var types =
                ProxyTypesAssemblies.Select(a => FindReflectedType(logicalName, a))
                                    .Where(t => t != null);

            if (types.Count() > 1)
            {
                var errorMsg = $"Type { logicalName } is defined in multiple assemblies: ";
                foreach (var type in types)
                {
                    errorMsg += type.Assembly
                                    .GetName()
                                    .Name + "; ";
                }
                var lastIndex = errorMsg.LastIndexOf("; ");
                errorMsg = errorMsg.Substring(0, lastIndex) + ".";
                throw new InvalidOperationException(errorMsg);
            }

            return types.SingleOrDefault();
        }

        /// <summary>
        /// Finds reflected type for given entity from given assembly.
        /// </summary>
        /// <param name="logicalName">
        /// Entity logical name which type is searched from given
        /// <paramref name="assembly"/>.
        /// </param>
        /// <param name="assembly">
        /// Assembly where early-bound type is searched for given
        /// <paramref name="logicalName"/>.
        /// </param>
        /// <returns>
        /// Early-bound type of <paramref name="logicalName"/> if it's found
        /// from <paramref name="assembly"/>. Otherwise null is returned.
        /// </returns>
        private static Type FindReflectedType(string logicalName,
                                              Assembly assembly)
        {
            try
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }

                var subClassType = assembly.GetTypes()
                        .Where(t => typeof(Entity).IsAssignableFrom(t))
                        .Where(t => t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).Length > 0)
                        .Where(t => ((EntityLogicalNameAttribute)t.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)[0]).LogicalName.Equals(logicalName.ToLower()))
                        .FirstOrDefault();

                return subClassType;
            }
            catch (ReflectionTypeLoadException exception)
            {
                // now look at ex.LoaderExceptions - this is an Exception[], so:
                var s = "";
                foreach (var innerException in exception.LoaderExceptions)
                {
                    // write details of "inner", in particular inner.Message
                    s += innerException.Message + " ";
                }

                throw new Exception("XrmFakedContext.FindReflectedType: " + s);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="earlyBoundType"></param>
        /// <param name="sEntityName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Type FindReflectedAttributeType(Type earlyBoundType, string sEntityName, string attributeName)
        {
            //Get that type properties
            var attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(attributeName);
            if (attributeInfo == null && attributeName.EndsWith("name"))
            {
                // Special case for referencing the name of a EntityReference
                attributeName = attributeName.Substring(0, attributeName.Length - 4);
                attributeInfo = earlyBoundType.GetEarlyBoundTypeAttribute(attributeName);

                if (attributeInfo.PropertyType != typeof(EntityReference))
                {
                    // Don't mess up if other attributes follow this naming pattern
                    attributeInfo = null;
                }
            }

            if (attributeInfo == null || attributeInfo.PropertyType.FullName == null)
            {
                //Try with metadata
                var injectedType = this.FindAttributeTypeInInjectedMetadata(sEntityName, attributeName);

                if (injectedType == null)
                {
                    throw new Exception($"XrmFakedContext.FindReflectedAttributeType: Attribute {attributeName} not found for type {earlyBoundType}");
                }

                return injectedType;
            }

            if (attributeInfo.PropertyType.FullName.EndsWith("Enum") || attributeInfo.PropertyType.BaseType.FullName.EndsWith("Enum"))
            {
                return typeof(int);
            }

            if (!attributeInfo.PropertyType.FullName.StartsWith("System."))
            {
                try
                {
                    var instance = Activator.CreateInstance(attributeInfo.PropertyType);
                    if (instance is Entity)
                    {
                        return typeof(EntityReference);
                    }
                }
                catch
                {
                    // ignored
                }
            }
#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            else if (attributeInfo.PropertyType.FullName.StartsWith("System.Nullable"))
            {
                return attributeInfo.PropertyType.GenericTypeArguments[0];
            }
#endif

            return attributeInfo.PropertyType;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public IQueryable<Entity> CreateQuery(string entityLogicalName)
        {
            return this.CreateQuery<Entity>(entityLogicalName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> CreateQuery<T>()
            where T : Entity
        {
            var typeParameter = typeof(T);

            if (ProxyTypesAssemblies.Count() == 0)
            {
                //Try to guess proxy types assembly
                var assembly = Assembly.GetAssembly(typeof(T));
                if (assembly != null)
                {
                    EnableProxyTypes(assembly);
                }
            }

            var logicalName = "";

            if (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true).Length > 0)
            {
                logicalName = (typeParameter.GetCustomAttributes(typeof(EntityLogicalNameAttribute), true)[0] as EntityLogicalNameAttribute).LogicalName;
            }

            return this.CreateQuery<T>(logicalName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IQueryable<T> CreateQuery<T>(string entityLogicalName)
            where T : Entity
        {
            var subClassType = FindReflectedType(entityLogicalName);
            if (subClassType == null && !(typeof(T) == typeof(Entity)) || (typeof(T) == typeof(Entity) && string.IsNullOrWhiteSpace(entityLogicalName)))
            {
                throw new Exception($"The type {entityLogicalName} was not found");
            }

            var lst = new List<T>();
            if (!Data.ContainsKey(entityLogicalName))
            {
                return lst.AsQueryable(); //Empty list
            }

            foreach (var e in Data[entityLogicalName].Values)
            {
                if (subClassType != null)
                {
                    var cloned = e.Clone(subClassType);
                    lst.Add((T)cloned);
                }
                else
                    lst.Add((T)e.Clone());
            }

            return lst.AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public IQueryable<Entity> CreateQueryFromEntityName(string entityName)
        {
            return Data[entityName].Values.AsQueryable();
        }      
    }
}