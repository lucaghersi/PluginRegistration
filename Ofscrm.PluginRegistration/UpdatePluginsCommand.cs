using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ofscrm.PluginRegistration.Helpers;
using Ofscrm.PluginRegistration.Wrappers;

namespace Ofscrm.PluginRegistration
{
    public class UpdatePluginsCommand : CommandBase
    {
        private readonly List<CrmPluginAssembly> _assemblies;
        private readonly UpdatePluginsCommandOptions _options;

        public UpdatePluginsCommand(UpdatePluginsCommandOptions options)
        {
            _options = options;
            _assemblies = new List<CrmPluginAssembly>();

            foreach (CrmPluginAssembly assembly in Organization.Assemblies)
            {
                // If the same assembly name used for any other custom plugin assembly then that need to be added
                if ((CrmServiceEndpoint.ServiceBusPluginAssemblyName != assembly.Name || 0 != assembly.CustomizationLevel) &&
                    !assembly.IsProfilerAssembly)
                {
                    _assemblies.Add(assembly);
                }
            }
        }

        public void ExecuteCommand()
        {
            // for each plugin registered in the system
            // check the plugin load folder for available matching assemblies
            var newAssemblies = _assemblies
                .Select(GetPluginAssemblyFromPluginFolder)
                .Where(assemblyCouple => assemblyCouple.localAssembly != null).ToList();

            // for each matching assemblies we execute the assembly update
            // and the plugin update
            foreach ((CrmPluginAssembly crmAssembly, CrmPluginAssembly localAssembly) assemblyCouple in newAssemblies)
            {
                RegisterPlugin(assemblyCouple.crmAssembly, assemblyCouple.localAssembly);
            }
        }

        private (CrmPluginAssembly crmAssembly, CrmPluginAssembly localAssembly) GetPluginAssemblyFromPluginFolder(CrmPluginAssembly crmAssembly)
        {
            string assemblyFileName = Path.Combine(_options.PluginsFolder, crmAssembly.Name);
            if (File.Exists(assemblyFileName) == false) return (crmAssembly, null);            

            CrmPluginAssembly localAssembly;
            try
            {
                localAssembly = RegistrationHelper.RetrievePluginsFromAssembly(assemblyFileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to load the specified Plugin Assembly", ex);
            }

            return (crmAssembly, localAssembly);
        }

        private void RegisterPlugin(CrmPluginAssembly crmAssembly, CrmPluginAssembly localAssembly)
        {
            
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Create a list of currently selected plugins
            var assemblyCanBeIsolated = true;
            var checkedPluginList = new Dictionary<string, CrmPlugin>();

            foreach (ICrmTreeNode node in trvPlugins.CheckedNodes)
            {
                if (node.NodeType == CrmTreeNodeType.Plugin || node.NodeType == CrmTreeNodeType.WorkflowActivity)
                {
                    var plugin = (CrmPlugin)node;
                    if (CrmPluginIsolatable.No == plugin.Isolatable)
                    {
                        assemblyCanBeIsolated = false;
                    }

                    checkedPluginList.Add(plugin.TypeName, plugin);
                }
            }

            //Check if there are any plugins selected
            if (checkedPluginList.Count == 0)
            {
                MessageBox.Show(
                    "No plugins have been selected from the list. Please select at least one and try again.",
                    "No Plugins Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // Verify that a valid isolation mode has been selected
            if (radIsolationSandbox.Checked && !assemblyCanBeIsolated)
            {
                MessageBox.Show(
                    "Since some of the plug-ins cannot be isolated, the assembly cannot be marked as Isolated.",
                    "Isolation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            //Reload the assembly
            var assemblyPath = AssemblyPathControl.FileName;
            CrmPluginAssembly assembly;
            if (string.IsNullOrEmpty(assemblyPath))
            {
                // Clone the existing assembly
                assembly = m_currentAssembly.Clone(false);
            }
            else
            {
                assembly = RegistrationHelper.RetrievePluginsFromAssembly(assemblyPath);

                //Retrieve the source type and determine if the 
                assembly.SourceType = GetAssemblySourceType();

                if (CrmAssemblySourceType.Disk != assembly.SourceType)
                {
                    assembly.ServerFileName = null;
                }
                else
                {
                    assembly.ServerFileName = txtServerFileName.Text;
                }
            }

            if (m_currentAssembly != null)
            {
                var oldVersion = new Version(m_currentAssembly.Version);
                var newVersion = new Version(assembly.Version);

                if (oldVersion.Major != newVersion.Major)
                {
                    MessageBox.Show(
                        $"Assembly's major version has changed from {oldVersion} to {newVersion}.\n\nSuch an update is not supported, please register another instance of this assembly instead!",
                        "Assembly's version missmatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (oldVersion.Minor != newVersion.Minor)
                {
                    MessageBox.Show(
                        $"Assembly's minor version has changed from {oldVersion} to {newVersion}.\n\nSuch an update is not supported, please register another instance of this assembly instead!",
                        "Assembly's version missmatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            // Ensure the checked items were all found in the assembly
            var registerPluginList = new List<CrmPlugin>();
            var pluginList = new List<CrmPlugin>();
            var removedPluginList = new List<CrmPlugin>();
            var missingPluginList = new List<CrmPlugin>();

            try
            {
                Parallel.ForEach(assembly.Plugins.Values, (currentPlugin) => {
                    var foundPlugin = m_registeredPluginList?.Where(x => x.TypeName.ToLowerInvariant() == currentPlugin.TypeName.ToLowerInvariant()).FirstOrDefault();
                    var alreadyExisted = (m_registeredPluginList != null && foundPlugin != null);

                    if (alreadyExisted)
                    {
                        currentPlugin.AssemblyId = m_currentAssembly.AssemblyId;
                        currentPlugin.PluginId = foundPlugin.PluginId;
                    }

                    if (checkedPluginList.ContainsKey(currentPlugin.TypeName))
                    {
                        registerPluginList.Add(currentPlugin);

                        if (currentPlugin.PluginType == CrmPluginType.Plugin)
                        {
                            pluginList.Add(currentPlugin);
                        }
                    }
                    else if (alreadyExisted)
                    {
                        removedPluginList.Add(currentPlugin);
                    }
                });

                if (m_registeredPluginList != null)
                {
                    Parallel.ForEach(m_registeredPluginList, (currentRecord) => {
                        if (!assembly.Plugins.Values.ToList().Any(x => x.TypeName.ToLowerInvariant() == currentRecord.TypeName.ToLowerInvariant()))
                        {
                            missingPluginList.Add(currentRecord);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessageForm.ShowErrorMessageBox(this, "Unable to load the specified Plugin Assembly.", "Plugins", ex);
                return;
            }

            // Update the assembly with the information specified by the user
            assembly.IsolationMode = GetIsolationMode();

            if (missingPluginList.Count != 0)
            {
                var list = missingPluginList.Select(x => x.TypeName).Aggregate((name01, name02) => name01 + "\n" + name02);

                MessageBox.Show(
                    $"Following plugin are missing in the assembly:\n\n{list}\n\nRegistration cannot continue!",
                    "Plugins are missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // An assembly with plugins must be strongly signed
            if (pluginList.Count != 0)
            {
                if (string.IsNullOrEmpty(assembly.PublicKeyToken))
                {
                    MessageBox.Show(
                        "Assemblies containing Plugins must be strongly signed. Sign the Assembly using a KeyFile.",
                        "Strong Names Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            // Check if there are any plugins selected that were in the assembly.
            if (registerPluginList.Count == 0)
            {
                MessageBox.Show(
                    "No plugins have been selected from the list. Please select at least one and try again.",
                    "No Plugins Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }
            else
            {
                assembly.ClearPlugins();
            }

            // If we are doing an Update, do some special processing
            if (m_currentAssembly != null)
            {
                assembly.AssemblyId = m_currentAssembly.AssemblyId;
            }
            #endregion

            #region Register Plugin
            m_progRegistration.Initialize(registerPluginList.Count + removedPluginList.Count, "Preparing Registration");

            int registeredAssemblies = 0;
            int ignoredAssemblies = 0;
            int updatedAssemblies = 0;
            bool createAssembly;

            //Check whether the plugin exists. If it exists, should we use the existing one?
            var retrieveDateList = new List<ICrmEntity>();
            try
            {
                Guid pluginAssemblyId = Guid.Empty;
                if (m_currentAssembly != null)
                {
                    if (chkUpdateAssembly.Checked)
                    {
                        var originalGroupName = RegistrationHelper.GenerateDefaultGroupName(m_currentAssembly.Name, new Version(m_currentAssembly.Version));
                        var newGroupName = RegistrationHelper.GenerateDefaultGroupName(assembly.Name, new Version(assembly.Version));

                        var updateGroupNameList = new List<PluginType>();
                        foreach (var plugin in m_currentAssembly.Plugins)
                        {
                            if (plugin.PluginType == CrmPluginType.WorkflowActivity && string.Equals(plugin.WorkflowActivityGroupName, originalGroupName))
                            {
                                updateGroupNameList.Add(new PluginType()
                                {
                                    Id = plugin.PluginId,
                                    WorkflowActivityGroupName = newGroupName
                                });
                            }
                        }

                        //Do the actual update to the assembly
                        RegistrationHelper.UpdateAssembly(m_org, assemblyPath, assembly, updateGroupNameList.ToArray());

                        m_currentAssembly.Name = assembly.Name;
                        m_currentAssembly.Culture = assembly.Culture;
                        m_currentAssembly.CustomizationLevel = assembly.CustomizationLevel;
                        m_currentAssembly.PublicKeyToken = assembly.PublicKeyToken;
                        m_currentAssembly.ServerFileName = assembly.ServerFileName;
                        m_currentAssembly.SourceType = assembly.SourceType;
                        m_currentAssembly.Version = assembly.Version;
                        m_currentAssembly.IsolationMode = assembly.IsolationMode;

                        retrieveDateList.Add(m_currentAssembly);

                        foreach (var type in updateGroupNameList)
                        {
                            var plugin = m_currentAssembly.Plugins[type.Id];

                            plugin.WorkflowActivityGroupName = type.WorkflowActivityGroupName;
                            retrieveDateList.Add(plugin);
                        }

                        updatedAssemblies++;
                    }
                    else if (!chkUpdateAssembly.Visible && assembly.IsolationMode != m_currentAssembly.IsolationMode)
                    {
                        var updateAssembly = new PluginAssembly()
                        {
                            Id = assembly.AssemblyId,
                            IsolationMode = new OptionSetValue((int)assembly.IsolationMode)
                        };

                        m_org.OrganizationService.Update(updateAssembly);

                        m_currentAssembly.ServerFileName = assembly.ServerFileName;
                        m_currentAssembly.SourceType = assembly.SourceType;
                        m_currentAssembly.IsolationMode = assembly.IsolationMode;

                        retrieveDateList.Add(m_currentAssembly);

                        updatedAssemblies++;
                    }

                    assembly = m_currentAssembly;

                    createAssembly = false;
                    m_progRegistration.Increment();

                    m_orgControl.RefreshAssembly(m_currentAssembly, false);
                }
                else
                {
                    createAssembly = true;
                    m_progRegistration.Increment();
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    ERROR_MESSAGE = ex.Message;
                }

                m_progRegistration.Increment($"ERROR: {ERROR_MESSAGE}");

                ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                m_progRegistration.Complete(false);
                return;
            }

            //Register the assembly (if needed)
            if (createAssembly)
            {
                try
                {
                    assembly.AssemblyId = RegistrationHelper.RegisterAssembly(m_org, assemblyPath, assembly);
                    assembly.Organization = m_org;

                    retrieveDateList.Add(assembly);
                }
                catch (Exception ex)
                {
                    var chunks = ex.Message.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    if (chunks.Length > 1)
                    {
                        // Replacing generic error message extracted from exception to the title of the window
                        ERROR_MESSAGE = chunks[chunks.Length - 1];
                    }

                    m_progRegistration.Increment($"ERROR: {ERROR_MESSAGE}");

                    ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                    m_progRegistration.Complete(false);
                    return;
                }

                registeredAssemblies++;
                m_progRegistration.Increment("SUCCESS: Plugin Assembly was registered");
            }
            else if (m_currentAssembly == null)
            {
                ignoredAssemblies++;
                m_progRegistration.Increment("INFORMATION: Assembly was not registered");
            }
            else
            {
                if (chkUpdateAssembly.Checked)
                {
                    m_progRegistration.Increment("SUCCESS: Assembly was updated");
                }
                else
                {
                    m_progRegistration.Increment("INFORMATION: Assembly was not updated");
                }
            }

            //Check to see if the assembly needs to be added to the list
            if (!m_org.Assemblies.ContainsKey(assembly.AssemblyId))
            {
                m_org.AddAssembly(assembly);

                //Update the Main Form
                try
                {
                    m_orgControl.AddAssembly(assembly);
                    m_progRegistration.Increment();
                }
                catch (Exception ex)
                {
                    m_progRegistration.Increment("ERROR: Error occurred while updating the Main form for the assembly");

                    ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                    m_progRegistration.Complete(false);
                    return;
                }
            }
            else
            {
                m_progRegistration.Increment();
            }

            // Register the Plugin
            bool createPlugin;
            int registeredPlugins = 0;
            int ignoredPlugins = 0;
            int errorsPlugins = 0;

            foreach (var currentPlugin in registerPluginList)
            {
                currentPlugin.AssemblyId = assembly.AssemblyId;

                //Check if the plugin exists
                bool pluginUpdate = m_registeredPluginList != null && m_registeredPluginList.Any(x => x.TypeName.ToLowerInvariant() == currentPlugin.TypeName.ToLowerInvariant());
                try
                {
                    Guid pluginTypeId = Guid.Empty;

                    if (pluginUpdate || (!createAssembly && RegistrationHelper.PluginExists(m_org, currentPlugin.TypeName, assembly.AssemblyId, out pluginTypeId)))
                    {
                        if (pluginUpdate)
                        {
                            createPlugin = false;
                        }
                        else
                        {
                            m_progRegistration.AppendText(string.Format("INFORMATION: Plugin Type Name is already being used by PluginType {0}.", pluginTypeId));

                            switch (MessageBox.Show(string.Format("The specified name \"{0}\" is already registered. Skip the registration of this plugin?\n\nPlease note the plugins may not be the same.", currentPlugin.TypeName),
                                "Plugin Already Exists", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                            {
                                case DialogResult.Yes:
                                    createPlugin = false;

                                    currentPlugin.PluginId = pluginTypeId;
                                    currentPlugin.Organization = assembly.Organization;
                                    break;
                                case DialogResult.No:
                                    createPlugin = true;
                                    break;
                                case DialogResult.Cancel:
                                    m_progRegistration.AppendText("ABORTED: Plugin Registration has been aborted by the user.");
                                    m_progRegistration.Complete(false);
                                    return;
                                default:
                                    throw new NotImplementedException();
                            }
                        }

                        m_progRegistration.Increment();
                    }
                    else
                    {
                        createPlugin = true;
                        m_progRegistration.Increment();
                    }
                }
                catch (Exception ex)
                {
                    m_progRegistration.Increment(string.Format("ERROR: Occurred while checking if {0} is already registered.",
                        currentPlugin.TypeName));

                    ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                    m_progRegistration.Complete(false);
                    return;
                }

                //Create the plugin (if necessary)
                if (createPlugin)
                {
                    try
                    {
                        Guid pluginId = currentPlugin.PluginId;
                        currentPlugin.PluginId = RegistrationHelper.RegisterPlugin(m_org, currentPlugin);
                        currentPlugin.Organization = m_org;

                        if (pluginId != currentPlugin.PluginId && assembly.Plugins.ContainsKey(pluginId))
                        {
                            assembly.RemovePlugin(pluginId);
                        }

                        retrieveDateList.Add(currentPlugin);

                        m_progRegistration.Increment(string.Format("SUCCESS: Plugin {0} was registered.",
                            currentPlugin.TypeName));

                        registeredPlugins++;
                    }
                    catch (Exception ex)
                    {
                        m_progRegistration.Increment(2, string.Format("ERROR: Occurred while registering {0}.",
                            currentPlugin.TypeName));

                        ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                        errorsPlugins++;
                        continue;
                    }
                }
                else
                {
                    if (!pluginUpdate)
                    {
                        ignoredPlugins++;
                    }

                    m_progRegistration.Increment();
                }

                //Check if the plugin needs to be added to the list
                if (!assembly.Plugins.ContainsKey(currentPlugin.PluginId))
                {
                    assembly.AddPlugin(currentPlugin);

                    //Update the main form
                    try
                    {
                        m_orgControl.AddPlugin(currentPlugin);
                        m_progRegistration.Increment();
                    }
                    catch (Exception ex)
                    {
                        m_progRegistration.Increment(string.Format("ERROR: Occurred while updating the Main form for {0}.",
                            currentPlugin.TypeName));

                        ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                        m_progRegistration.Complete(false);
                        return;
                    }
                }
                else
                {
                    m_progRegistration.Increment();
                }
            }

            // Unregister plugins that were unchecked
            int updatedPlugins = 0;
            foreach (var currectPlugin in removedPluginList)
            {
                //Check if the plugin exists
                try
                {
                    RegistrationHelper.Unregister(m_org, currectPlugin);
                    m_progRegistration.Increment(3, string.Format("SUCCESS: Plugin {0} was unregistered.", currectPlugin.TypeName));
                    m_orgControl.RemovePlugin(currectPlugin.PluginId);

                    updatedPlugins++;
                }
                catch (Exception ex)
                {
                    m_progRegistration.Increment(3, string.Format("ERROR: Occurred while unregistering {0}.", currectPlugin.TypeName));

                    ErrorMessageForm.ShowErrorMessageBox(this, ERROR_MESSAGE, ERROR_CAPTION, ex);

                    errorsPlugins++;
                }
            }

            //Update the entities whose Created On / Modified On dates changed
            try
            {
                OrganizationHelper.UpdateDates(m_org, retrieveDateList);
                m_progRegistration.Increment("SUCCESS: Created On / Modified On dates updated");
            }
            catch (Exception ex)
            {
                m_progRegistration.Increment("ERROR: Unable to update Created On / Modified On dates");

                ErrorMessageForm.ShowErrorMessageBox(this, "Unable to update Created On / Modified On dates", "Update Error", ex);
            }
            #endregion

            m_progRegistration.AppendText("SUCCESS: Selected Plugins have been registered");
            m_progRegistration.Complete(false);

            MessageBox.Show(string.Format("The selected Plugins have been registered.\n{0} Assembly Registered\n{1} Assembly Ignored\n{2} Assembly Updated\n{3} Plugin(s) Registered\n{4} Plugin(s) Ignored\n{5} Plugin(s) Encountered Errors\n{6} Plugin(s) Removed",
                    registeredAssemblies, ignoredAssemblies, updatedAssemblies, registeredPlugins, ignoredPlugins, errorsPlugins, updatedPlugins),
                "Registered Plugins", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (errorsPlugins == 0)
            {
                Close();
            }
        }
    }
}