#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.ConditionOperatorTests.HierarchyOperators
{
    public class HierarchyOperatorTestsBase: FakeXrmEasyTestsBase
    {
        protected readonly Account _grandParentAccount;
        protected readonly Account _parentAccount;
        protected readonly Account _childAccount;
        protected readonly Account _nestedChildAccount1;
        protected readonly Account _nestedChildAccount2;

        protected readonly dv_test _noHierarchyEntity;
        
        public HierarchyOperatorTestsBase()
        {
            _grandParentAccount = new Account()
            {
                Id = new Guid("4BA6769F-4F01-4F7C-B5E3-895B171323FE"),
                Name = "Grand Parent Account"
            };
            _parentAccount = new Account()
            {
                Id = new Guid("6DE0D3D8-4C5F-43BF-AFDB-93727EC40BC8"),
                Name = "Parent Account",
                ParentAccountId = _grandParentAccount.ToEntityReference()
            };
            _childAccount = new Account()
            {
                Id = new Guid("97B1EB88-E60F-43BC-8EAA-08575D6B723F"),
                Name = "Child Account",
                ParentAccountId = _parentAccount.ToEntityReference()
            };
            _nestedChildAccount1 = new Account()
            {
                Id = new Guid("25E18679-45ED-4D5A-A1EB-7DC03E56C2A1"),
                Name = "Nested Child 1",
                ParentAccountId = _childAccount.ToEntityReference()
            };
            _nestedChildAccount2 = new Account()
            {
                Id = new Guid("5B405A61-A942-46F8-ABB1-9EDA15E3F4DE"),
                Name = "Nested Child 2",
                ParentAccountId = _childAccount.ToEntityReference()
            };
            
            _noHierarchyEntity = new dv_test()
            {
                Id = new Guid("E06F83C0-B5D8-48E6-9048-DDCF26200407"),
                dv_string = "No hierarchy test"
            };
            
            _context.AddRelationship("account_parent_account", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.FakeRelationshipType.OneToMany,
                IsHierarchical = true,
                Entity1LogicalName = "account",
                Entity1Attribute = "accountid",
                Entity2LogicalName = "account",
                Entity2Attribute = "parentaccountid"
            });
            
            _context.Initialize(new List<Entity>()
            {
                _grandParentAccount,
                _parentAccount,
                _childAccount,
                _nestedChildAccount1,
                _nestedChildAccount2,
                _noHierarchyEntity
            });
        }
    }
}
#endif