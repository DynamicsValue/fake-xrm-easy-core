using Crm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.PermissionsTests
{
    public class PermissionsTests: FakeXrmEasyTests
    {
        [Fact]
        public void Entity_Granted_Access_Has_Access()
        {
            
            var contact = new Contact { Id = Guid.NewGuid() };
            var user = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact, user
            });


            GrantAccessRequest gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess,
                    Principal = user.ToEntityReference()
                },
                Target = contact.ToEntityReference()
            };
            _service.Execute(gar);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void Entity_Granted_Multiple_Access_Has_Access()
        {
            
            var contact = new Contact { Id = Guid.NewGuid() };
            var user = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact, user
            });

            

            GrantAccessRequest gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.DeleteAccess | AccessRights.CreateAccess,
                    Principal = user.ToEntityReference()
                },
                Target = contact.ToEntityReference()
            };
            _service.Execute(gar);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void Multiple_Entities_No_Confusion()
        {
            
            var contact1 = new Contact { Id = Guid.NewGuid() };
            var contact2 = new Contact { Id = Guid.NewGuid() };
            var user = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact1, contact2, user
            });

            

            GrantAccessRequest gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.DeleteAccess | AccessRights.CreateAccess,
                    Principal = user.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar);

            gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ShareAccess,
                    Principal = user.ToEntityReference()
                },
                Target = contact2.ToEntityReference()
            };
            _service.Execute(gar);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact1.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void Entity_Not_Granted_Access_Does_Not_Have_Access()
        {
            
            var contact = new Contact { Id = Guid.NewGuid() };
            var user = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact, user
            });

            

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.Equal(AccessRights.None, rpaResp.AccessRights);
        }

        [Fact]
        public void Entity_Revoked_Access_Does_Not_Have_Access()
        {
            
            var contact = new Contact { Id = Guid.NewGuid() };
            var user = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact, user
            });

            

            GrantAccessRequest gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess,
                    Principal = user.ToEntityReference()
                },
                Target = contact.ToEntityReference()
            };
            _service.Execute(gar);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            RevokeAccessRequest rar = new RevokeAccessRequest
            {
                Target = contact.ToEntityReference(),
                Revokee = user.ToEntityReference()
            };

            _service.Execute(rar);
            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.Equal(AccessRights.None, rpaResp.AccessRights);
        }

        [Fact]
        public void Entity_Revoked_Access_Does_Not_Have_Access_Multiple_Users()
        {
            
            var contact = new Contact { Id = Guid.NewGuid() };
            var user1 = new SystemUser { Id = Guid.NewGuid() };
            var user2 = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact, user1, user2
            });

            

            GrantAccessRequest gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact.ToEntityReference()
            };
            _service.Execute(gar);

            gar = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.DeleteAccess | AccessRights.CreateAccess,
                    Principal = user2.ToEntityReference()
                },
                Target = contact.ToEntityReference()
            };
            _service.Execute(gar);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user1.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user2.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            RevokeAccessRequest rar = new RevokeAccessRequest
            {
                Target = contact.ToEntityReference(),
                Revokee = user1.ToEntityReference()
            };

            _service.Execute(rar);
            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user1.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.Equal(AccessRights.None, rpaResp.AccessRights);

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact.ToEntityReference(),
                Principal = user2.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));
        }

        [Fact]
        public void Multiple_Entities_With_Multiple_Users()
        {
            
            var contact1 = new Contact { Id = Guid.NewGuid() };
            var contact2 = new Contact { Id = Guid.NewGuid() };
            var user1 = new SystemUser { Id = Guid.NewGuid() };
            var user2 = new SystemUser { Id = Guid.NewGuid() };
            var user3 = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact1, user1, contact2, user2, user3
            });

            

            GrantAccessRequest gar1 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar1);

            GrantAccessRequest gar2 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.CreateAccess | AccessRights.DeleteAccess | AccessRights.ShareAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact2.ToEntityReference()
            };
            _service.Execute(gar2);

            GrantAccessRequest gar3 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.CreateAccess | AccessRights.DeleteAccess | AccessRights.ShareAccess,
                    Principal = user2.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar3);

            GrantAccessRequest gar4 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user2.ToEntityReference()
                },
                Target = contact2.ToEntityReference()
            };
            _service.Execute(gar4);

            RetrievePrincipalAccessRequest rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact1.ToEntityReference(),
                Principal = user1.ToEntityReference()
            };

            RetrievePrincipalAccessResponse rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact2.ToEntityReference(),
                Principal = user1.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact1.ToEntityReference(),
                Principal = user2.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact2.ToEntityReference(),
                Principal = user2.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.NotEqual(AccessRights.None, rpaResp.AccessRights);
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.ReadAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AppendToAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.AssignAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.CreateAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.DeleteAccess));
            Assert.False(rpaResp.AccessRights.HasFlag(AccessRights.ShareAccess));
            Assert.True(rpaResp.AccessRights.HasFlag(AccessRights.WriteAccess));

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact1.ToEntityReference(),
                Principal = user3.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.Equal(AccessRights.None, rpaResp.AccessRights);

            rpar = new RetrievePrincipalAccessRequest
            {
                Target = contact2.ToEntityReference(),
                Principal = user3.ToEntityReference()
            };

            rpaResp = (RetrievePrincipalAccessResponse)_service.Execute(rpar);
            Assert.Equal(AccessRights.None, rpaResp.AccessRights);
        }

        [Fact]
        public void RetrieveSharedPrincipalsAndAccess_Test()
        {
            
            var contact1 = new Contact { Id = Guid.NewGuid() };
            var contact2 = new Contact { Id = Guid.NewGuid() };
            var user1 = new SystemUser { Id = Guid.NewGuid() };
            var user2 = new SystemUser { Id = Guid.NewGuid() };
            var user3 = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact1, user1, contact2, user2, user3
            });

            

            GrantAccessRequest gar1 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar1);

            GrantAccessRequest gar2 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.CreateAccess | AccessRights.DeleteAccess | AccessRights.ShareAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact2.ToEntityReference()
            };
            _service.Execute(gar2);

            GrantAccessRequest gar3 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.CreateAccess | AccessRights.DeleteAccess | AccessRights.ShareAccess,
                    Principal = user2.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar3);

            GrantAccessRequest gar4 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user2.ToEntityReference()
                },
                Target = contact2.ToEntityReference()
            };
            _service.Execute(gar4);

            RetrieveSharedPrincipalsAndAccessRequest req = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = contact1.ToEntityReference()
            };
            RetrieveSharedPrincipalsAndAccessResponse resp = (RetrieveSharedPrincipalsAndAccessResponse)_service.Execute(req);

            foreach (PrincipalAccess pa in resp.PrincipalAccesses)
            {
                if (pa.Principal.Id == user1.Id)
                {
                    Assert.NotEqual(AccessRights.None, pa.AccessMask);
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ReadAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendToAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AssignAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.CreateAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.DeleteAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.ShareAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.WriteAccess));
                }
                else if (pa.Principal.Id == user2.Id)
                {
                    Assert.NotEqual(AccessRights.None, pa.AccessMask);
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ReadAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendToAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AssignAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.CreateAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.DeleteAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ShareAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.WriteAccess));
                }
                else if (pa.Principal.Id == user3.Id)
                {
                    Assert.Equal(AccessRights.None, pa.AccessMask);
                }
            }

            req = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = contact2.ToEntityReference()
            };
            resp = (RetrieveSharedPrincipalsAndAccessResponse)_service.Execute(req);

            foreach (PrincipalAccess pa in resp.PrincipalAccesses)
            {
                if (pa.Principal.Id == user2.Id)
                {
                    Assert.NotEqual(AccessRights.None, pa.AccessMask);
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ReadAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendToAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AssignAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.CreateAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.DeleteAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.ShareAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.WriteAccess));
                }
                else if (pa.Principal.Id == user1.Id)
                {
                    Assert.NotEqual(AccessRights.None, pa.AccessMask);
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ReadAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AppendToAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.AssignAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.CreateAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.DeleteAccess));
                    Assert.True(pa.AccessMask.HasFlag(AccessRights.ShareAccess));
                    Assert.False(pa.AccessMask.HasFlag(AccessRights.WriteAccess));
                }
                else if (pa.Principal.Id == user3.Id)
                {
                    Assert.Equal(AccessRights.None, pa.AccessMask);
                }
            }
        }

        [Fact]
        public void Principal_Granted_Access_Multiple_Times_Only_Appears_Once()
        {
            
            var contact1 = new Contact { Id = Guid.NewGuid() };
            var user1 = new SystemUser { Id = Guid.NewGuid() };

            _context.Initialize(new List<Entity>
            {
                contact1, user1
            });

            

            GrantAccessRequest gar1 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar1);

            GrantAccessRequest gar2 = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess,
                    Principal = user1.ToEntityReference()
                },
                Target = contact1.ToEntityReference()
            };
            _service.Execute(gar2);

            RetrieveSharedPrincipalsAndAccessRequest req = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = contact1.ToEntityReference()
            };
            RetrieveSharedPrincipalsAndAccessResponse resp = (RetrieveSharedPrincipalsAndAccessResponse)_service.Execute(req);

            Assert.Equal(1, resp.PrincipalAccesses.Length);
        }
    }
}