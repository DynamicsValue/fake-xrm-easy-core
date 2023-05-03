using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FakeXrmEasy.Core.PipelineTypes
{
    /// <summary>
    /// Type that inherits from the IPlugin interface and is contained within a plug-in assembly.
    /// </summary>
    [DataContract()]
    [EntityLogicalName("plugintype")]
    [GeneratedCode("CrmSvcUtil", "9.1.0.93")]
    public partial class PluginType : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public PluginType() :
                base(EntityLogicalName)
        {
        }

        /// <summary>
        /// EntityLogicalName
        /// </summary>
        public const string EntityLogicalName = "plugintype";

        /// <summary>
        /// EntityLogicalCollectionName
        /// </summary>
        public const string EntityLogicalCollectionName = "plugintypes";

        /// <summary>
        /// EntitySetName
        /// </summary>
        public const string EntitySetName = "plugintypes";

        /// <summary>
        /// EntityTypeCode
        /// </summary>
        public const int EntityTypeCode = 4602;

        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanging
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// OnPropertyChanged
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// OnPropertyChanging
        /// </summary>
        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Full path name of the plug-in assembly.
        /// </summary>
        [AttributeLogicalNameAttribute("assemblyname")]
        public string AssemblyName
        {
            get
            {
                return this.GetAttributeValue<string>("assemblyname");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalName("componentstate")]
        public OptionSets.ComponentState? ComponentState
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue attributeValue = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("componentstate");
                if ((attributeValue != null))
                {
                    return ((OptionSets.ComponentState)(Enum.ToObject(typeof(OptionSets.ComponentState), attributeValue.Value)));
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.OnPropertyChanging("ComponentState");
                if ((value == null))
                {
                    this.SetAttributeValue("componentstate", value);
                }
                else
                {
                    this.SetAttributeValue("componentstate", new OptionSetValue(((int)(value))));
                }
                this.OnPropertyChanged("ComponentState");
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the plug-in type.
        /// </summary>
        [AttributeLogicalName("createdby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("createdby");
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
                return this.GetAttributeValue<DateTime?>("createdon");
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
                return this.GetAttributeValue<EntityReference>("createdonbehalfby");
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
                return this.GetAttributeValue<string>("culture");
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
                return this.GetAttributeValue<int?>("customizationlevel");
            }
        }

        /// <summary>
        /// Serialized Custom Activity Type information, including required arguments. For more information, see SandboxCustomActivityInfo.
        /// </summary>
        [AttributeLogicalName("customworkflowactivityinfo")]
        public string CustomWorkflowActivityInfo
        {
            get
            {
                return this.GetAttributeValue<string>("customworkflowactivityinfo");
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
                return this.GetAttributeValue<string>("description");
            }
            set
            {
                this.OnPropertyChanging("Description");
                this.SetAttributeValue("description", value);
                this.OnPropertyChanged("Description");
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
                return this.GetAttributeValue<string>("friendlyname");
            }
            set
            {
                this.OnPropertyChanging("FriendlyName");
                this.SetAttributeValue("friendlyname", value);
                this.OnPropertyChanged("FriendlyName");
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
                return this.GetAttributeValue<bool?>("ismanaged");
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
                return this.GetAttributeValue<bool?>("isworkflowactivity");
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
                return this.GetAttributeValue<int?>("major");
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
                return this.GetAttributeValue<int?>("minor");
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
                return this.GetAttributeValue<EntityReference>("modifiedby");
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
                return this.GetAttributeValue<DateTime?>("modifiedon");
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
                return this.GetAttributeValue<EntityReference>("modifiedonbehalfby");
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
                return this.GetAttributeValue<string>("name");
            }
            set
            {
                this.OnPropertyChanging("Name");
                this.SetAttributeValue("name", value);
                this.OnPropertyChanged("Name");
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
                return this.GetAttributeValue<EntityReference>("organizationid");
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
                return this.GetAttributeValue<DateTime?>("overwritetime");
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
                return this.GetAttributeValue<EntityReference>("pluginassemblyid");
            }
            set
            {
                this.OnPropertyChanging("PluginAssemblyId");
                this.SetAttributeValue("pluginassemblyid", value);
                this.OnPropertyChanged("PluginAssemblyId");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeLogicalName("plugintypeexportkey")]
        public string PluginTypeExportKey
        {
            get
            {
                return this.GetAttributeValue<string>("plugintypeexportkey");
            }
            set
            {
                this.OnPropertyChanging("PluginTypeExportKey");
                this.SetAttributeValue("plugintypeexportkey", value);
                this.OnPropertyChanged("PluginTypeExportKey");
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
                return this.GetAttributeValue<Guid?>("plugintypeid");
            }
            set
            {
                this.OnPropertyChanging("PluginTypeId");
                this.SetAttributeValue("plugintypeid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged("PluginTypeId");
            }
        }

        /// <summary>
        /// Id
        /// </summary>
        [AttributeLogicalName("plugintypeid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                this.PluginTypeId = value;
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
                return this.GetAttributeValue<Guid?>("plugintypeidunique");
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
                return this.GetAttributeValue<string>("publickeytoken");
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
                return this.GetAttributeValue<Guid?>("solutionid");
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
                return this.GetAttributeValue<string>("typename");
            }
            set
            {
                this.OnPropertyChanging("TypeName");
                this.SetAttributeValue("typename", value);
                this.OnPropertyChanged("TypeName");
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
                return this.GetAttributeValue<string>("version");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [AttributeLogicalName("versionnumber")]
        public System.Nullable<long> VersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
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
                return this.GetAttributeValue<string>("workflowactivitygroupname");
            }
            set
            {
                this.OnPropertyChanging("WorkflowActivityGroupName");
                this.SetAttributeValue("workflowactivitygroupname", value);
                this.OnPropertyChanged("WorkflowActivityGroupName");
            }
        }
    }

    /// <summary>
    /// OptionSets
    /// </summary>
    public sealed class OptionSets
    {

        /// <summary>
        /// ComponentState
        /// </summary>
        [System.Runtime.Serialization.DataContractAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.93")]
        public enum ComponentState
        {

            /// <summary>
            /// Published
            /// </summary>
            [System.Runtime.Serialization.EnumMemberAttribute()]
            Published = 0,

            /// <summary>
            /// Unpublished
            /// </summary>
            [System.Runtime.Serialization.EnumMemberAttribute()]
            Unpublished = 1,

            /// <summary>
            /// Deleted
            /// </summary>
            [System.Runtime.Serialization.EnumMemberAttribute()]
            Deleted = 2,

            /// <summary>
            /// Deleted Unpublished
            /// </summary>
            [System.Runtime.Serialization.EnumMemberAttribute()]
            DeletedUnpublished = 3,
        }
    }

}
