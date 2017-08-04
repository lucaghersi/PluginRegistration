﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Ofscrm.PluginRegistration.Entities
{
    /// <summary>
    /// Type that inherits from the IPlugin interface and is contained within a plug-in assembly.
    /// </summary>
    [DataContract()]
    [EntityLogicalName("plugintype")]
    [GeneratedCode("CrmSvcUtil", "5.0.9689.1985")]
    public partial class PluginType : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public PluginType() :
            base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "plugintype";

        public const int EntityTypeCode = 4602;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanged(string propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanging(string propertyName)
        {
            if ((PropertyChanging != null))
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Full path name of the plug-in assembly.
        /// </summary>
        [AttributeLogicalName("assemblyname")]
        public string AssemblyName
        {
            get
            {
                return GetAttributeValue<string>("assemblyname");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalName("componentstate")]
        public OptionSetValue ComponentState
        {
            get
            {
                return GetAttributeValue<OptionSetValue>("componentstate");
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the plug-in type.
        /// </summary>
        [AttributeLogicalName("createdby")]
        public EntityReference CreatedBy
        {
            get
            {
                return GetAttributeValue<EntityReference>("createdby");
            }
        }

        /// <summary>
        /// Date and time when the plug-in type was created.
        /// </summary>
        [AttributeLogicalName("createdon")]
        public DateTime? CreatedOn
        {
            get
            {
                return GetAttributeValue<DateTime?>("createdon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who created the plugintype.
        /// </summary>
        [AttributeLogicalName("createdonbehalfby")]
        public EntityReference CreatedOnBehalfBy
        {
            get
            {
                return GetAttributeValue<EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Culture code for the plug-in assembly.
        /// </summary>
        [AttributeLogicalName("culture")]
        public string Culture
        {
            get
            {
                return GetAttributeValue<string>("culture");
            }
        }

        /// <summary>
        /// Customization level of the plug-in type.
        /// </summary>
        [AttributeLogicalName("customizationlevel")]
        public int? CustomizationLevel
        {
            get
            {
                return GetAttributeValue<int?>("customizationlevel");
            }
        }

        /// <summary>
        /// Description of the plug-in type.
        /// </summary>
        [AttributeLogicalName("description")]
        public string Description
        {
            get
            {
                return GetAttributeValue<string>("description");
            }
            set
            {
                OnPropertyChanging("Description");
                SetAttributeValue("description", value);
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// User friendly name for the plug-in.
        /// </summary>
        [AttributeLogicalName("friendlyname")]
        public string FriendlyName
        {
            get
            {
                return GetAttributeValue<string>("friendlyname");
            }
            set
            {
                OnPropertyChanging("FriendlyName");
                SetAttributeValue("friendlyname", value);
                OnPropertyChanged("FriendlyName");
            }
        }

        /// <summary>
        ///
        /// </summary>
        [AttributeLogicalName("ismanaged")]
        public bool? IsManaged
        {
            get
            {
                return GetAttributeValue<bool?>("ismanaged");
            }
        }

        /// <summary>
        /// Indicates if the plug-in is a custom activity for workflows.
        /// </summary>
        [AttributeLogicalName("isworkflowactivity")]
        public bool? IsWorkflowActivity
        {
            get
            {
                return GetAttributeValue<bool?>("isworkflowactivity");
            }
        }

        /// <summary>
        /// Major of the version number of the assembly for the plug-in type.
        /// </summary>
        [AttributeLogicalName("major")]
        public int? Major
        {
            get
            {
                return GetAttributeValue<int?>("major");
            }
        }

        /// <summary>
        /// Minor of the version number of the assembly for the plug-in type.
        /// </summary>
        [AttributeLogicalName("minor")]
        public int? Minor
        {
            get
            {
                return GetAttributeValue<int?>("minor");
            }
        }

        /// <summary>
        /// Unique identifier of the user who last modified the plug-in type.
        /// </summary>
        [AttributeLogicalName("modifiedby")]
        public EntityReference ModifiedBy
        {
            get
            {
                return GetAttributeValue<EntityReference>("modifiedby");
            }
        }

        /// <summary>
        /// Date and time when the plug-in type was last modified.
        /// </summary>
        [AttributeLogicalName("modifiedon")]
        public DateTime? ModifiedOn
        {
            get
            {
                return GetAttributeValue<DateTime?>("modifiedon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who last modified the plugintype.
        /// </summary>
        [AttributeLogicalName("modifiedonbehalfby")]
        public EntityReference ModifiedOnBehalfBy
        {
            get
            {
                return GetAttributeValue<EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Name of the plug-in type.
        /// </summary>
        [AttributeLogicalName("name")]
        public string Name
        {
            get
            {
                return GetAttributeValue<string>("name");
            }
            set
            {
                OnPropertyChanging("Name");
                SetAttributeValue("name", value);
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Unique identifier of the organization with which the plug-in type is associated.
        /// </summary>
        [AttributeLogicalName("organizationid")]
        public EntityReference OrganizationId
        {
            get
            {
                return GetAttributeValue<EntityReference>("organizationid");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalName("overwritetime")]
        public DateTime? OverwriteTime
        {
            get
            {
                return GetAttributeValue<DateTime?>("overwritetime");
            }
        }

        /// <summary>
        /// Unique identifier of the plug-in assembly that contains this plug-in type.
        /// </summary>
        [AttributeLogicalName("pluginassemblyid")]
        public EntityReference PluginAssemblyId
        {
            get
            {
                return GetAttributeValue<EntityReference>("pluginassemblyid");
            }
            set
            {
                OnPropertyChanging("PluginAssemblyId");
                SetAttributeValue("pluginassemblyid", value);
                OnPropertyChanged("PluginAssemblyId");
            }
        }

        /// <summary>
        /// Unique identifier of the plug-in type.
        /// </summary>
        [AttributeLogicalName("plugintypeid")]
        public Guid? PluginTypeId
        {
            get
            {
                return GetAttributeValue<Guid?>("plugintypeid");
            }
            set
            {
                OnPropertyChanging("PluginTypeId");
                SetAttributeValue("plugintypeid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = Guid.Empty;
                }
                OnPropertyChanged("PluginTypeId");
            }
        }

        [AttributeLogicalName("plugintypeid")]
        public override Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                PluginTypeId = value;
            }
        }

        /// <summary>
        /// Unique identifier of the plug-in type.
        /// </summary>
        [AttributeLogicalName("plugintypeidunique")]
        public Guid? PluginTypeIdUnique
        {
            get
            {
                return GetAttributeValue<Guid?>("plugintypeidunique");
            }
        }

        /// <summary>
        /// Public key token of the assembly for the plug-in type.
        /// </summary>
        [AttributeLogicalName("publickeytoken")]
        public string PublicKeyToken
        {
            get
            {
                return GetAttributeValue<string>("publickeytoken");
            }
        }

        /// <summary>
        /// Unique identifier of the associated solution.
        /// </summary>
        [AttributeLogicalName("solutionid")]
        public Guid? SolutionId
        {
            get
            {
                return GetAttributeValue<Guid?>("solutionid");
            }
        }

        /// <summary>
        /// Fully qualified type name of the plug-in type.
        /// </summary>
        [AttributeLogicalName("typename")]
        public string TypeName
        {
            get
            {
                return GetAttributeValue<string>("typename");
            }
            set
            {
                OnPropertyChanging("TypeName");
                SetAttributeValue("typename", value);
                OnPropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Version number of the assembly for the plug-in type.
        /// </summary>
        [AttributeLogicalName("version")]
        public string Version
        {
            get
            {
                return GetAttributeValue<string>("version");
            }
        }

        /// <summary>
        ///
        /// </summary>
        [AttributeLogicalName("versionnumber")]
        public long? VersionNumber
        {
            get
            {
                return GetAttributeValue<long?>("versionnumber");
            }
        }

        /// <summary>
        /// Group name of workflow custom activity.
        /// </summary>
        [AttributeLogicalName("workflowactivitygroupname")]
        public string WorkflowActivityGroupName
        {
            get
            {
                return GetAttributeValue<string>("workflowactivitygroupname");
            }
            set
            {
                OnPropertyChanging("WorkflowActivityGroupName");
                SetAttributeValue("workflowactivitygroupname", value);
                OnPropertyChanged("WorkflowActivityGroupName");
            }
        }
    }
}