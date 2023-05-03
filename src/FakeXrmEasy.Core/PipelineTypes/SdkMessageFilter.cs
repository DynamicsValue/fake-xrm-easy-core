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
    /// Filter that defines which SDK messages are valid for each type of entity.
    /// </summary>
    [DataContractAttribute()]
    [EntityLogicalNameAttribute("sdkmessagefilter")]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal class SdkMessageFilter : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SdkMessageFilter() :
                base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "sdkmessagefilter";

        public const string EntityLogicalCollectionName = "sdkmessagefilters";

        public const string EntitySetName = "sdkmessagefilters";

        public const int EntityTypeCode = 4607;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Identifies where a method will be exposed. 0 - Server, 1 - Client, 2 - both.
        /// </summary>
        [AttributeLogicalNameAttribute("availability")]
        public int? Availability
        {
            get
            {
                return this.GetAttributeValue<int?>("availability");
            }
            set
            {
                this.OnPropertyChanging("Availability");
                this.SetAttributeValue("availability", value);
                this.OnPropertyChanged("Availability");
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the SDK message filter.
        /// </summary>
        [AttributeLogicalNameAttribute("createdby")]
        public EntityReference CreatedBy
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("createdby");
            }
        }

        /// <summary>
        /// Date and time when the SDK message filter was created.
        /// </summary>
        [AttributeLogicalNameAttribute("createdon")]
        public DateTime? CreatedOn
        {
            get
            {
                return this.GetAttributeValue<DateTime?>("createdon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who created the sdkmessagefilter.
        /// </summary>
        [AttributeLogicalNameAttribute("createdonbehalfby")]
        public EntityReference CreatedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Customization level of the SDK message filter.
        /// </summary>
        [AttributeLogicalNameAttribute("customizationlevel")]
        public int? CustomizationLevel
        {
            get
            {
                return this.GetAttributeValue<int?>("customizationlevel");
            }
        }

        /// <summary>
        /// Version in which the component is introduced.
        /// </summary>
        [AttributeLogicalNameAttribute("introducedversion")]
        public string IntroducedVersion
        {
            get
            {
                return this.GetAttributeValue<string>("introducedversion");
            }
            set
            {
                this.OnPropertyChanging("IntroducedVersion");
                this.SetAttributeValue("introducedversion", value);
                this.OnPropertyChanged("IntroducedVersion");
            }
        }

        /// <summary>
        /// Indicates whether a custom SDK message processing step is allowed.
        /// </summary>
        [AttributeLogicalNameAttribute("iscustomprocessingstepallowed")]
        public bool? IsCustomProcessingStepAllowed
        {
            get
            {
                return this.GetAttributeValue<bool?>("iscustomprocessingstepallowed");
            }
            set
            {
                this.OnPropertyChanging("IsCustomProcessingStepAllowed");
                this.SetAttributeValue("iscustomprocessingstepallowed", value);
                this.OnPropertyChanged("IsCustomProcessingStepAllowed");
            }
        }

        /// <summary>
        /// Information that specifies whether this component is managed.
        /// </summary>
        [AttributeLogicalNameAttribute("ismanaged")]
        public bool? IsManaged
        {
            get
            {
                return this.GetAttributeValue<bool?>("ismanaged");
            }
        }

        /// <summary>
        /// Indicates whether the filter should be visible.
        /// </summary>
        [AttributeLogicalNameAttribute("isvisible")]
        public bool? IsVisible
        {
            get
            {
                return this.GetAttributeValue<bool?>("isvisible");
            }
        }

        /// <summary>
        /// Unique identifier of the user who last modified the SDK message filter.
        /// </summary>
        [AttributeLogicalNameAttribute("modifiedby")]
        public EntityReference ModifiedBy
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("modifiedby");
            }
        }

        /// <summary>
        /// Date and time when the SDK message filter was last modified.
        /// </summary>
        [AttributeLogicalNameAttribute("modifiedon")]
        public DateTime? ModifiedOn
        {
            get
            {
                return this.GetAttributeValue<DateTime?>("modifiedon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who last modified the sdkmessagefilter.
        /// </summary>
        [AttributeLogicalNameAttribute("modifiedonbehalfby")]
        public EntityReference ModifiedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Name of the SDK message filter.
        /// </summary>
        [AttributeLogicalNameAttribute("name")]
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
        /// Unique identifier of the organization with which the SDK message filter is associated.
        /// </summary>
        [AttributeLogicalNameAttribute("organizationid")]
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
        [AttributeLogicalNameAttribute("overwritetime")]
        public DateTime? OverwriteTime
        {
            get
            {
                return this.GetAttributeValue<DateTime?>("overwritetime");
            }
        }

        /// <summary>
        /// Type of entity with which the SDK message filter is primarily associated.
        /// </summary>
        [AttributeLogicalNameAttribute("primaryobjecttypecode")]
        public string PrimaryObjectTypeCode
        {
            get
            {
                return this.GetAttributeValue<string>("primaryobjecttypecode");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalNameAttribute("restrictionlevel")]
        public int? RestrictionLevel
        {
            get
            {
                return this.GetAttributeValue<int?>("restrictionlevel");
            }
            set
            {
                this.OnPropertyChanging("RestrictionLevel");
                this.SetAttributeValue("restrictionlevel", value);
                this.OnPropertyChanged("RestrictionLevel");
            }
        }

        /// <summary>
        /// Unique identifier of the SDK message filter entity.
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessagefilterid")]
        public Guid? SdkMessageFilterId
        {
            get
            {
                return this.GetAttributeValue<Guid?>("sdkmessagefilterid");
            }
            set
            {
                this.OnPropertyChanging("SdkMessageFilterId");
                this.SetAttributeValue("sdkmessagefilterid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }
                this.OnPropertyChanged("SdkMessageFilterId");
            }
        }

        [AttributeLogicalNameAttribute("sdkmessagefilterid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                this.SdkMessageFilterId = value;
            }
        }

        /// <summary>
        /// Unique identifier of the SDK message filter.
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessagefilteridunique")]
        public Guid? SdkMessageFilterIdUnique
        {
            get
            {
                return this.GetAttributeValue<Guid?>("sdkmessagefilteridunique");
            }
        }

        /// <summary>
        /// Unique identifier of the related SDK message.
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessageid")]
        public EntityReference SdkMessageId
        {
            get
            {
                return this.GetAttributeValue<EntityReference>("sdkmessageid");
            }
            set
            {
                this.OnPropertyChanging("SdkMessageId");
                this.SetAttributeValue("sdkmessageid", value);
                this.OnPropertyChanged("SdkMessageId");
            }
        }

        /// <summary>
        /// Type of entity with which the SDK message filter is secondarily associated.
        /// </summary>
        [AttributeLogicalNameAttribute("secondaryobjecttypecode")]
        public string SecondaryObjectTypeCode
        {
            get
            {
                return this.GetAttributeValue<string>("secondaryobjecttypecode");
            }
        }

        /// <summary>
        /// Unique identifier of the associated solution.
        /// </summary>
        [AttributeLogicalNameAttribute("solutionid")]
        public Guid? SolutionId
        {
            get
            {
                return this.GetAttributeValue<Guid?>("solutionid");
            }
        }

        [AttributeLogicalNameAttribute("versionnumber")]
        public System.Nullable<long> VersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
            }
        }

        /// <summary>
        /// Whether or not the SDK message can be called from a workflow.
        /// </summary>
        [AttributeLogicalNameAttribute("workflowsdkstepenabled")]
        public bool? WorkflowSdkStepEnabled
        {
            get
            {
                return this.GetAttributeValue<bool?>("workflowsdkstepenabled");
            }
        }

        /// <summary>
        /// 1:N sdkmessagefilterid_sdkmessageprocessingstep
        /// </summary>
        [RelationshipSchemaNameAttribute("sdkmessagefilterid_sdkmessageprocessingstep")]
        public IEnumerable<SdkMessageProcessingStep> sdkmessagefilterid_sdkmessageprocessingstep
        {
            get
            {
                return this.GetRelatedEntities<SdkMessageProcessingStep>("sdkmessagefilterid_sdkmessageprocessingstep", null);
            }
            set
            {
                this.OnPropertyChanging("sdkmessagefilterid_sdkmessageprocessingstep");
                this.SetRelatedEntities<SdkMessageProcessingStep>("sdkmessagefilterid_sdkmessageprocessingstep", null, value);
                this.OnPropertyChanged("sdkmessagefilterid_sdkmessageprocessingstep");
            }
        }

        /// <summary>
        /// N:1 sdkmessageid_sdkmessagefilter
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessageid")]
        [RelationshipSchemaNameAttribute("sdkmessageid_sdkmessagefilter")]
        public SdkMessage sdkmessageid_sdkmessagefilter
        {
            get
            {
                return this.GetRelatedEntity<SdkMessage>("sdkmessageid_sdkmessagefilter", null);
            }
            set
            {
                this.OnPropertyChanging("sdkmessageid_sdkmessagefilter");
                this.SetRelatedEntity<SdkMessage>("sdkmessageid_sdkmessagefilter", null, value);
                this.OnPropertyChanged("sdkmessageid_sdkmessagefilter");
            }
        }
    }

    /// <summary>
    /// Identifies if a plug-in should be executed from a parent pipeline, a child pipeline, or both.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_invocationsource
    {

        [EnumMemberAttribute()]
        Internal = -1,

        [EnumMemberAttribute()]
        Parent = 0,

        [EnumMemberAttribute()]
        Child = 1,
    }

    /// <summary>
    /// Run-time mode of execution, for example, synchronous or asynchronous.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_mode
    {

        [EnumMemberAttribute()]
        Synchronous = 0,

        [EnumMemberAttribute()]
        Asynchronous = 1,
    }

    /// <summary>
    /// Stage in the execution pipeline that the SDK message processing step is in.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_stage
    {

        [EnumMemberAttribute()]
        InitialPreoperation_Forinternaluseonly = 5,

        [EnumMemberAttribute()]
        Prevalidation = 10,

        [EnumMemberAttribute()]
        InternalPreoperationBeforeExternalPlugins_Forinternaluseonly = 15,

        [EnumMemberAttribute()]
        Preoperation = 20,

        [EnumMemberAttribute()]
        InternalPreoperationAfterExternalPlugins_Forinternaluseonly = 25,

        [EnumMemberAttribute()]
        MainOperation_Forinternaluseonly = 30,

        [EnumMemberAttribute()]
        InternalPostoperationBeforeExternalPlugins_Forinternaluseonly = 35,

        [EnumMemberAttribute()]
        Postoperation = 40,

        [EnumMemberAttribute()]
        InternalPostoperationAfterExternalPlugins_Forinternaluseonly = 45,

        [EnumMemberAttribute()]
        Postoperation_Deprecated = 50,

        [EnumMemberAttribute()]
        FinalPostoperation_Forinternaluseonly = 55,

        [EnumMemberAttribute()]
        PreCommitstagefiredbeforetransactioncommit_Forinternaluseonly = 80,

        [EnumMemberAttribute()]
        PostCommitstagefiredaftertransactioncommit_Forinternaluseonly = 90,
    }

    /// <summary>
    /// Status of the SDK message processing step.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_statecode
    {

        [EnumMemberAttribute()]
        Enabled = 0,

        [EnumMemberAttribute()]
        Disabled = 1,
    }

    /// <summary>
    /// Reason for the status of the SDK message processing step.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_statuscode
    {

        [EnumMemberAttribute()]
        Enabled = 1,

        [EnumMemberAttribute()]
        Disabled = 2,
    }

    /// <summary>
    /// Deployment that the SDK message processing step should be executed on; server, client, or both.
    /// </summary>
    [DataContractAttribute()]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal enum sdkmessageprocessingstep_supporteddeployment
    {

        [EnumMemberAttribute()]
        ServerOnly = 0,

        [EnumMemberAttribute()]
        MicrosoftDynamics365ClientforOutlookOnly = 1,

        [EnumMemberAttribute()]
        Both = 2,
    }
}
