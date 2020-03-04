
Breaking changes
=======================================================================

 - Microsoft.Xrm.Client no longer exists, because of that, some of the tests targeting FAKE_XRM_EASY (2011) are not passing and probably support for 2011 will be dropped.

 - Some messages returning FaultException now return FaultException&lt;OrganizationServiceFault&gt;

 - Pacakges will target .net core only, or .net standard, if crmsdk packages are updated to use .net standard eventually.

