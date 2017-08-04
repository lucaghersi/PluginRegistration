using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ofscrm.PluginRegistration.Helpers
{
    internal static class AssemblyResolver
    {
        #region Private Fields

        /// <summary>
        /// Subdirectories that may contain an assembly (if it can't be located).
        /// </summary>
        /// <remarks>
        /// This is useful for architecture specific assemblies that may be different depending on the process architecture
        /// of the current assembly.
        /// </remarks>
        private static readonly string[] AssemblyProbeSubdirectories = new string[] { string.Empty, "amd64", "i386", @"..\..\..\..\..\private\lib" };

        /// <summary>
        /// List of base directories that should be checked
        /// </summary>
        private static Collection<string> _baseDirectories = SetLocations();

        /// <summary>
        /// Contains a list of the assemblies that were resolved via the custom assembly resolve event
        /// </summary>
        private static Dictionary<string, Assembly> _resolvedAssemblies = new Dictionary<string, Assembly>();

        #endregion Private Fields

        #region Internal Methods

        /// <summary>
        /// Attaches the resolver to the current app domain
        /// </summary>
        internal static void AttachResolver()
        {
            AttachResolver(AppDomain.CurrentDomain);
        }

        /// <summary>
        /// Attaches the resolver to the current app domain
        /// </summary>
        internal static void AttachResolver(AppDomain domain)
        {
            domain.AssemblyResolve -= new ResolveEventHandler(ResolveAssembly);

            if (null == domain)
            {
                throw new ArgumentNullException("domain");
            }

            domain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
        }

        #endregion Internal Methods

        #region Private Methods

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            //Check if the assembly has already been resolved
            Assembly resolvedAssembly;
            if (_resolvedAssemblies.TryGetValue(args.Name, out resolvedAssembly))
            {
                return resolvedAssembly;
            }

            //Check if the assembly has been already loaded
            resolvedAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == args.Name).FirstOrDefault();

            if (resolvedAssembly != null)
            {
                return resolvedAssembly;
            }

            //Create an AssemblyName from the event arguments so that the name can be retrieved
            var name = new AssemblyName(args.Name);

            //Create a file name for the assembly to start probing for a location
            var fileName = name.Name + ".dll";

            //Loop through the probing subdirectories to see if the assembly exists
            foreach (var baseDirectory in _baseDirectories)
            {
                foreach (var subdirectory in AssemblyProbeSubdirectories)
                {
                    //Create the file path to the assembly
                    var assemblyPath = Path.Combine(Path.Combine(baseDirectory, subdirectory), fileName);

                    //Check if the file path exists
                    if (File.Exists(assemblyPath))
                    {
                        //Load the assembly and return it
                        try
                        {
                            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath));
                            _resolvedAssemblies[args.Name] = assembly;
                            return assembly;
                        }
                        catch (BadImageFormatException)
                        {
                            //Ignore this assembly, because it will not work for the current assembly
                        }
                    }
                }
            }

            _resolvedAssemblies[args.Name] = null;
            return null;
        }

        private static Collection<string> SetLocations()
        {
            var list = new Collection<string>();
            var location = string.Empty;

            // Adding current AppDomain location
            location = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory).ToUpperInvariant();
            list.Add(location);

            // Searching for plugins in subfolder of current AppDomain location
            location = Path.Combine(location, "Plugins").ToUpperInvariant();

            // Adding (if different) current working folder location
            location = Path.GetDirectoryName(Environment.CurrentDirectory).ToUpperInvariant();
            if (!list.Contains(location))
            {
                list.Add(location);
            }

            // Searching for plugins in subfolder of current working folder location
            location = Path.Combine(location, "Plugins").ToUpperInvariant();
            if (!list.Contains(location))
            {
                list.Add(location);
            }

            return list;
        }

        #endregion Private Methods
    }
}