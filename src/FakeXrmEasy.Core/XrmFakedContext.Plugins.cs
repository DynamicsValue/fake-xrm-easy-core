using FakeItEasy;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins;
using System.Collections.Generic;

namespace FakeXrmEasy
{
    public partial class XrmFakedContext : IXrmFakedContext
    {
        

        

        /// <summary>
        /// Executes a plugin passing a custom context. This is useful whenever we need to mock more complex plugin contexts (ex: passing MessageName, plugin Depth, InitiatingUserId etc...)
        /// </summary>
        /// <typeparam name="T">Must be a plugin</typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public IPlugin ExecutePluginWith<T>(XrmFakedPluginExecutionContext ctx = null)
            where T : IPlugin, new()
        {
            if (ctx == null)
            {
                ctx = GetDefaultPluginContext();
            }

            return this.ExecutePluginWith(ctx, new T());
        }

        /// <summary>
        /// Executes a plugin passing a custom context. This is useful whenever we need to mock more complex plugin contexts (ex: passing MessageName, plugin Depth, InitiatingUserId etc...)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public IPlugin ExecutePluginWith(XrmFakedPluginExecutionContext ctx, IPlugin instance)
        {
            var fakedServiceProvider = PluginContextProperties.GetServiceProvider(ctx);

            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = instance;
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider); //Execute the plugin
            return fakedPlugin;
        }

        public IPlugin ExecutePluginWith<T>(ParameterCollection inputParameters, ParameterCollection outputParameters, EntityImageCollection preEntityImages, EntityImageCollection postEntityImages)
            where T : IPlugin, new()
        {
            var ctx = GetDefaultPluginContext();
            ctx.InputParameters.AddRange(inputParameters);
            ctx.OutputParameters.AddRange(outputParameters);
            ctx.PreEntityImages.AddRange(preEntityImages);
            ctx.PostEntityImages.AddRange(postEntityImages);

            var fakedServiceProvider = PluginContextProperties.GetServiceProvider(ctx);

            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = new T();
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider); //Execute the plugin
            return fakedPlugin;
        }

        public IPlugin ExecutePluginWithConfigurations<T>(XrmFakedPluginExecutionContext plugCtx, string unsecureConfiguration, string secureConfiguration)
            where T : class, IPlugin
        {
            var pluginType = typeof(T);
            var constructors = pluginType.GetConstructors().ToList();

            if (!constructors.Any(c => c.GetParameters().Length == 2 && c.GetParameters().All(param => param.ParameterType == typeof(string))))
            {
                throw new ArgumentException("The plugin you are trying to execute does not specify a constructor for passing in two configuration strings.");
            }

            var pluginInstance = (T)Activator.CreateInstance(typeof(T), unsecureConfiguration, secureConfiguration);

            return this.ExecutePluginWith(plugCtx, pluginInstance);
        }

        [Obsolete("Use ExecutePluginWith(XrmFakedPluginExecutionContext ctx, IPlugin instance).")]
        public IPlugin ExecutePluginWithConfigurations<T>(XrmFakedPluginExecutionContext plugCtx, T instance, string unsecureConfiguration="", string secureConfiguration="")
            where T : class, IPlugin
        {
            var fakedServiceProvider = PluginContextProperties.GetServiceProvider(plugCtx);

            var fakedPlugin = A.Fake<IPlugin>();

            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var pluginType = typeof(T);
                    var constructors = pluginType.GetConstructors();

                    if (!constructors.Any(c => c.GetParameters().Length == 2 && c.GetParameters().All(param => param.ParameterType == typeof(string))))
                    {
                        throw new ArgumentException("The plugin you are trying to execute does not specify a constructor for passing in two configuration strings.");
                    }

                    var plugin = instance;
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider); //Execute the plugin
            return fakedPlugin;
        }

        public IPlugin ExecutePluginWithTarget<T>(XrmFakedPluginExecutionContext ctx, Entity target, string messageName = "Create", int stage = 40)
          where T : IPlugin, new()
        {
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return this.ExecutePluginWith<T>(ctx);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity target
        /// and returns the faked plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public IPlugin ExecutePluginWithTarget<T>(Entity target, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            return this.ExecutePluginWithTarget(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public IPlugin ExecutePluginWithTarget(IPlugin instance, Entity target, string messageName = "Create", int stage = 40)
        {
            var ctx = GetDefaultPluginContext();

            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return this.ExecutePluginWith(ctx, instance);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity reference target
        /// and returns the faked plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public IPlugin ExecutePluginWithTargetReference<T>(EntityReference target, string messageName = "Delete", int stage = 40)
            where T : IPlugin, new()
        {
            return this.ExecutePluginWithTargetReference(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity reference target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public IPlugin ExecutePluginWithTargetReference(IPlugin instance, EntityReference target, string messageName = "Delete", int stage = 40)
        {
            var ctx = GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return this.ExecutePluginWith(ctx, instance);
        }

        /// <summary>
        /// Returns a faked plugin with a target and the specified pre entity images
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public IPlugin ExecutePluginWithTargetAndPreEntityImages<T>(object target, EntityImageCollection preEntityImages, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.PreEntityImages.AddRange(preEntityImages);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return this.ExecutePluginWith<T>(ctx);
        }

        /// <summary>
        /// Returns a faked plugin with a target and the specified post entity images
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public IPlugin ExecutePluginWithTargetAndPostEntityImages<T>(object target, EntityImageCollection postEntityImages, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.PostEntityImages.AddRange(postEntityImages);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return this.ExecutePluginWith<T>(ctx);
        }

        [Obsolete]
        public IPlugin ExecutePluginWithTargetAndInputParameters<T>(Entity target, ParameterCollection inputParameters, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = GetDefaultPluginContext();

            ctx.InputParameters.AddRange(inputParameters);

            return this.ExecutePluginWithTarget<T>(ctx, target, messageName, stage);
        }



    }
}
