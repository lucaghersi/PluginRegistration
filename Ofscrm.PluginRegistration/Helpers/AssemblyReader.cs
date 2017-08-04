using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Ofscrm.PluginRegistration.Wrappers;

namespace Ofscrm.PluginRegistration.Helpers
{
    public sealed class AssemblyReader : MarshalByRefObject
    {
        #region Public Constructors

        public AssemblyReader()
        {
            //Attach the resolver so that assemblies can be resolved correctly
            AssemblyResolver.AttachResolver(AppDomain.CurrentDomain);
        }

        #endregion Public Constructors

        #region Public Methods

        public CrmPluginAssembly RetrieveAssemblyProperties(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            else if (!File.Exists(path))
            {
                throw new ArgumentException("Path does not point to an existing file");
            }

            return RetrieveAssemblyProperties(LoadAssembly(path), path);
        }

        public CrmPluginAssembly RetrievePluginsFromAssembly(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            else if (!File.Exists(path))
            {
                throw new ArgumentException("Path does not point to an existing file");
            }

            //Load the assembly
            var assembly = LoadAssembly(path);
            var assemblyName = assembly.GetName();
            var defaultGroupName = RegistrationHelper.GenerateDefaultGroupName(assemblyName.Name, assemblyName.Version);

            //Retrieve the assembly properties
            var pluginAssembly = RetrieveAssemblyProperties(assembly, path);

            //Loop through each type and process it
            var errorList = new List<string>();
            foreach (var t in assembly.GetExportedTypes())
            {
                //Plugins and Workflow Activities must be non-abstract classes
                if (t.IsAbstract || !t.IsClass)
                {
                    continue;
                }

                // Plugins must implement the IPlugin interface
                string errorMessage = null;
                CrmPluginType type;
                CrmPluginIsolatable isolatable;

                //Retrieve the two interface types
                var xrmPlugin = t.GetInterface(typeof(IPlugin).FullName);
                var v4Plugin = t.GetInterface("Microsoft.Crm.Sdk.IPlugin");

                var workflowGroupName = defaultGroupName;
                var pluginName = t.FullName;

                Version sdkVersion = null;
                if (null != xrmPlugin)
                {
                    type = CrmPluginType.Plugin;
                    isolatable = CrmPluginIsolatable.Yes;
                    sdkVersion = xrmPlugin.Assembly.GetName().Version;
                }
                else if (null != v4Plugin)
                {
                    type = CrmPluginType.Plugin;
                    isolatable = CrmPluginIsolatable.No;

                    //It may be that the version of Microsoft.Crm.Sdk.dll will be the v5 version, but it will still be a v4 plug-in.
                    sdkVersion = new Version(4, 0);
                }
                else if (t.IsSubclassOf(typeof(CodeActivity)) || t.IsSubclassOf(typeof(Activity)))
                {
                    type = CrmPluginType.WorkflowActivity;
                    isolatable = CrmPluginIsolatable.Yes;
                }
                else
                {
                    continue;
                }

                //If the SDK version has been set, extract the Major/Minor number
                if (null != sdkVersion)
                {
                    pluginAssembly.SdkVersion = new Version(sdkVersion.Major, sdkVersion.Minor);
                }

                if (errorMessage != null)
                {
                    errorList.Add(errorMessage);
                }
                else
                {
                    var plugin = new CrmPlugin(null);
                    plugin.TypeName = t.FullName;
                    plugin.Name = pluginName;
                    plugin.PluginType = type;
                    plugin.PluginId = Guid.NewGuid();
                    plugin.AssemblyId = pluginAssembly.AssemblyId;
                    plugin.AssemblyName = pluginAssembly.Name;
                    plugin.Isolatable = isolatable;

                    if (type == CrmPluginType.WorkflowActivity && !string.IsNullOrWhiteSpace(workflowGroupName))
                    {
                        plugin.WorkflowActivityGroupName = workflowGroupName;
                    }

                    pluginAssembly.AddPlugin(plugin);
                }
            }

            return pluginAssembly;
        }

        #endregion Public Methods

        #region Private Methods

        private Assembly LoadAssembly(string path)
        {
            return Assembly.LoadFrom(path);
        }

        private CrmPluginAssembly RetrieveAssemblyProperties(Assembly assembly, string path)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            var pluginAssembly = new CrmPluginAssembly(null);
            pluginAssembly.AssemblyId = Guid.NewGuid();
            pluginAssembly.SourceType = CrmAssemblySourceType.Disk;

            var fileInfo = new FileInfo(path);
            pluginAssembly.ServerFileName = fileInfo.Name;

            var name = assembly.GetName();
            string cultureLabel;
            if (name.CultureInfo.LCID == System.Globalization.CultureInfo.InvariantCulture.LCID)
            {
                cultureLabel = "neutral";
            }
            else
            {
                cultureLabel = name.CultureInfo.Name;
            }

            pluginAssembly.Name = name.Name;
            pluginAssembly.Version = name.Version.ToString();
            pluginAssembly.Culture = cultureLabel;

            byte[] tokenBytes = name.GetPublicKeyToken();
            if (null == tokenBytes || 0 == tokenBytes.Length)
            {
                pluginAssembly.PublicKeyToken = null;
            }
            else
            {
                pluginAssembly.PublicKeyToken = string.Join(string.Empty, tokenBytes.Select(b => b.ToString("X2")));
            }

            return pluginAssembly;
        }

        #endregion Private Methods
    }
}