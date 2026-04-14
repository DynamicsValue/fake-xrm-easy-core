namespace FakeXrmEasy.Core.Tests.Metadata.MetadataGeneratorTests
{
        public class UnknownAttributeType
        {
            
        }
        
        [System.Runtime.Serialization.DataContractAttribute()]
        [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("dummy")]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.118")]
        public class DummyEntity : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
        {
            public DummyEntity() : 
                base(EntityLogicalName)
            {
            }
		
            public const string EntityLogicalName = "dummy";
            
            [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("dummyid")]
            public override System.Guid Id
            {
                get
                {
                    return base.Id;
                }
                set
                {
                    this.DummyId = value;
                }
            }
            
            /// <summary>
            /// Unique identifier of the account.
            /// </summary>
            [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("dummyid")]
            public System.Nullable<System.Guid> DummyId
            {
                get
                {
                    return this.GetAttributeValue<System.Nullable<System.Guid>>("accountid");
                }
                set
                {
                    this.OnPropertyChanging("AccountId");
                    this.SetAttributeValue("accountid", value);
                    if (value.HasValue)
                    {
                        base.Id = value.Value;
                    }
                    else
                    {
                        base.Id = System.Guid.Empty;
                    }
                    this.OnPropertyChanged("AccountId");
                }
            }
            
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
            public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
            
            private void OnPropertyChanged(string propertyName)
            {
                if ((this.PropertyChanged != null))
                {
                    this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
		
            private void OnPropertyChanging(string propertyName)
            {
                if ((this.PropertyChanging != null))
                {
                    this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
                }
            }
        }
}