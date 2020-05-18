#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013

using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Metadata;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests
{
    public class DateTimeBehaviourTests
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public DateTimeBehaviourTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_RetrieveMultiple_with_DateTime_Field_Behaviour_set_to_DateOnly_result_is_Time_Part_is_Zero()
        {
            var _context = new XrmFakedContext
            {
                DateBehaviour = new Dictionary<string, Dictionary<string, DateTimeAttributeBehavior>>
                {
                    {
                        "contact", new Dictionary<string, DateTimeAttributeBehavior>
                        {
                            { "birthdate", DateTimeAttributeBehavior.DateOnly }
                        }
                    }
                }
            };

            _context.Initialize(new List<Entity>
            {
                new Contact
                {
                    Id = Guid.NewGuid(),
                    ["createdon"] = new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                    BirthDate = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc)
                }
            });

            

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet("createdon", "birthdate")
            };

            var entity = _service.RetrieveMultiple(query).Entities.Cast<Contact>().First();

            Assert.Equal(new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc), entity.CreatedOn);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), entity.BirthDate);
        }

        [Fact]
        public void When_RetrieveMultiple_with_DateTime_Field_Behaviour_set_to_UserLocal_result_is_Time_Part_is_Kept()
        {
            _context.DateBehaviour = new Dictionary<string, Dictionary<string, DateTimeAttributeBehavior>>
                {
                    {
                        "contact", new Dictionary<string, DateTimeAttributeBehavior>
                        {
                            { "birthdate", DateTimeAttributeBehavior.UserLocal }
                        }
                    }
                };

            _context.Initialize(new List<Entity>
            {
                new Contact
                {
                    Id = Guid.NewGuid(),
                    ["createdon"] = new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                    BirthDate = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc)
                }
            });

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet("createdon", "birthdate")
            };

            var entity = _service.RetrieveMultiple(query).Entities.Cast<Contact>().First();

            Assert.Equal(new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc), entity.CreatedOn);
            Assert.Equal(new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc), entity.BirthDate);
        }

        [Fact]
        public void When_Retrieve_with_DateTime_Field_Behaviour_set_to_DateOnly_result_is_Time_Part_is_Zero()
        {
            var _context = new XrmFakedContext
            {
                DateBehaviour = new Dictionary<string, Dictionary<string, DateTimeAttributeBehavior>>
                {
                    {
                        "contact", new Dictionary<string, DateTimeAttributeBehavior>
                        {
                            { "birthdate", DateTimeAttributeBehavior.DateOnly }
                        }
                    }
                }
            };

            var id = Guid.NewGuid();

            _context.Initialize(new List<Entity>
            {
                new Contact
                {
                    Id = id,
                    ["createdon"] = new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                    BirthDate = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc)
                }
            });

            

            var entity = _service.Retrieve("contact", id, new ColumnSet("createdon", "birthdate")).ToEntity<Contact>();

            Assert.Equal(new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc), entity.CreatedOn);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), entity.BirthDate);
        }

        [Fact]
        public void When_Retrieve_with_DateTime_Field_Behaviour_set_to_UserLocal_result_is_Time_Part_is_Kept()
        {
            var _context = new XrmFakedContext
            {
                DateBehaviour = new Dictionary<string, Dictionary<string, DateTimeAttributeBehavior>>
                {
                    {
                        "contact", new Dictionary<string, DateTimeAttributeBehavior>
                        {
                            { "birthdate", DateTimeAttributeBehavior.UserLocal }
                        }
                    }
                }
            };

            var id = Guid.NewGuid();

            _context.Initialize(new List<Entity>
            {
                new Contact
                {
                    Id = id,
                    ["createdon"] = new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                    BirthDate = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc)
                }
            });

            

            var entity = _service.Retrieve("contact", id, new ColumnSet("createdon", "birthdate")).ToEntity<Contact>();

            Assert.Equal(new DateTime(2017, 1, 1, 1, 0, 0, DateTimeKind.Utc), entity.CreatedOn);
            Assert.Equal(new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc), entity.BirthDate);
        }
    }
}

#endif