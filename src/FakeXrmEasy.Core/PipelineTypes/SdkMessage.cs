using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace FakeXrmEasy.Core.PipelineTypes
{
    /// <summary>
    /// Message that is supported by the SDK.
    /// </summary>
    [DataContractAttribute()]
    [EntityLogicalNameAttribute("sdkmessage")]
    [GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
    internal class SdkMessage : Entity, INotifyPropertyChanging, INotifyPropertyChanged
    {

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SdkMessage() :
                base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "sdkmessage";

        public const string EntityLogicalCollectionName = "sdkmessages";

        public const string EntitySetName = "sdkmessages";

        public const int EntityTypeCode = 4606;

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
        /// Information about whether the SDK message is automatically transacted.
        /// </summary>
        [AttributeLogicalNameAttribute("autotransact")]
        public bool? AutoTransact
        {
            get
            {
                return this.GetAttributeValue<bool?>("autotransact");
            }
            set
            {
                this.OnPropertyChanging("AutoTransact");
                this.SetAttributeValue("autotransact", value);
                this.OnPropertyChanged("AutoTransact");
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
        /// If this is a categorized method, this is the name, otherwise None.
        /// </summary>
        [AttributeLogicalNameAttribute("categoryname")]
        public string CategoryName
        {
            get
            {
                return this.GetAttributeValue<string>("categoryname");
            }
            set
            {
                this.OnPropertyChanging("CategoryName");
                this.SetAttributeValue("categoryname", value);
                this.OnPropertyChanged("CategoryName");
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the SDK message.
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
        /// Date and time when the SDK message was created.
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
        /// Unique identifier of the delegate user who created the sdkmessage.
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
        /// Customization level of the SDK message.
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
        /// Name of the privilege that allows execution of the SDK message
        /// </summary>
        [AttributeLogicalNameAttribute("executeprivilegename")]
        public string ExecutePrivilegeName
        {
            get
            {
                return this.GetAttributeValue<string>("executeprivilegename");
            }
            set
            {
                this.OnPropertyChanging("ExecutePrivilegeName");
                this.SetAttributeValue("executeprivilegename", value);
                this.OnPropertyChanged("ExecutePrivilegeName");
            }
        }

        /// <summary>
        /// Indicates whether the SDK message should have its requests expanded per primary entity defined in its filters.
        /// </summary>
        [AttributeLogicalNameAttribute("expand")]
        public bool? Expand
        {
            get
            {
                return this.GetAttributeValue<bool?>("expand");
            }
            set
            {
                this.OnPropertyChanging("Expand");
                this.SetAttributeValue("expand", value);
                this.OnPropertyChanged("Expand");
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
        /// Information about whether the SDK message is active.
        /// </summary>
        [AttributeLogicalNameAttribute("isactive")]
        public bool? IsActive
        {
            get
            {
                return this.GetAttributeValue<bool?>("isactive");
            }
            set
            {
                this.OnPropertyChanging("IsActive");
                this.SetAttributeValue("isactive", value);
                this.OnPropertyChanged("IsActive");
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
        /// Indicates whether the SDK message is private.
        /// </summary>
        [AttributeLogicalNameAttribute("isprivate")]
        public bool? IsPrivate
        {
            get
            {
                return this.GetAttributeValue<bool?>("isprivate");
            }
            set
            {
                this.OnPropertyChanging("IsPrivate");
                this.SetAttributeValue("isprivate", value);
                this.OnPropertyChanged("IsPrivate");
            }
        }

        /// <summary>
        /// Identifies whether an SDK message will be ReadOnly or Read Write. false - ReadWrite, true - ReadOnly .
        /// </summary>
        [AttributeLogicalNameAttribute("isreadonly")]
        public bool? IsReadOnly
        {
            get
            {
                return this.GetAttributeValue<bool?>("isreadonly");
            }
            set
            {
                this.OnPropertyChanging("IsReadOnly");
                this.SetAttributeValue("isreadonly", value);
                this.OnPropertyChanged("IsReadOnly");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalNameAttribute("isvalidforexecuteasync")]
        public bool? IsValidForExecuteAsync
        {
            get
            {
                return this.GetAttributeValue<bool?>("isvalidforexecuteasync");
            }
        }

        /// <summary>
        /// Unique identifier of the user who last modified the SDK message.
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
        /// Date and time when the SDK message was last modified.
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
        /// Unique identifier of the delegate user who last modified the sdkmessage.
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
        /// Name of the SDK message.
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
        /// Unique identifier of the organization with which the SDK message is associated.
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
        /// Unique identifier of the SDK message entity.
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessageid")]
        public Guid? SdkMessageId
        {
            get
            {
                return this.GetAttributeValue<Guid?>("sdkmessageid");
            }
            set
            {
                this.OnPropertyChanging("SdkMessageId");
                this.SetAttributeValue("sdkmessageid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = Guid.Empty;
                }
                this.OnPropertyChanged("SdkMessageId");
            }
        }

        [AttributeLogicalNameAttribute("sdkmessageid")]
        public override Guid Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                this.SdkMessageId = value;
            }
        }

        /// <summary>
        /// Unique identifier of the SDK message.
        /// </summary>
        [AttributeLogicalNameAttribute("sdkmessageidunique")]
        public Guid? SdkMessageIdUnique
        {
            get
            {
                return this.GetAttributeValue<Guid?>("sdkmessageidunique");
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

        /// <summary>
        /// Indicates whether the SDK message is a template.
        /// </summary>
        [AttributeLogicalNameAttribute("template")]
        public bool? Template
        {
            get
            {
                return this.GetAttributeValue<bool?>("template");
            }
            set
            {
                this.OnPropertyChanging("Template");
                this.SetAttributeValue("template", value);
                this.OnPropertyChanged("Template");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [AttributeLogicalNameAttribute("throttlesettings")]
        public string ThrottleSettings
        {
            get
            {
                return this.GetAttributeValue<string>("throttlesettings");
            }
        }

        /// <summary>
        /// Number that identifies a specific revision of the SDK message. 
        /// </summary>
        [AttributeLogicalNameAttribute("versionnumber")]
        public long? VersionNumber
        {
            get
            {
                return this.GetAttributeValue<long?>("versionnumber");
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
        /// 1:N sdkmessageid_sdkmessagefilter
        /// </summary>
        [RelationshipSchemaNameAttribute("sdkmessageid_sdkmessagefilter")]
        public IEnumerable<SdkMessageFilter> sdkmessageid_sdkmessagefilter
        {
            get
            {
                return this.GetRelatedEntities<SdkMessageFilter>("sdkmessageid_sdkmessagefilter", null);
            }
            set
            {
                this.OnPropertyChanging("sdkmessageid_sdkmessagefilter");
                this.SetRelatedEntities<SdkMessageFilter>("sdkmessageid_sdkmessagefilter", null, value);
                this.OnPropertyChanged("sdkmessageid_sdkmessagefilter");
            }
        }

        /// <summary>
        /// 1:N sdkmessageid_sdkmessageprocessingstep
        /// </summary>
        [RelationshipSchemaNameAttribute("sdkmessageid_sdkmessageprocessingstep")]
        public IEnumerable<SdkMessageProcessingStep> sdkmessageid_sdkmessageprocessingstep
        {
            get
            {
                return this.GetRelatedEntities<SdkMessageProcessingStep>("sdkmessageid_sdkmessageprocessingstep", null);
            }
            set
            {
                this.OnPropertyChanging("sdkmessageid_sdkmessageprocessingstep");
                this.SetRelatedEntities<SdkMessageProcessingStep>("sdkmessageid_sdkmessageprocessingstep", null, value);
                this.OnPropertyChanged("sdkmessageid_sdkmessageprocessingstep");
            }
        }
    }
}
