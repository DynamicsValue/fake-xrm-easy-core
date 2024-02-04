## [3.4.0]

## Added

- **Alpha**: Introduced subscription usage monitoring based on customer feedback

### Changed

- Set default build configuration in solution file to FAKE_XRM_EASY_9
- Remove ReleaseNotes from package description - https://github.com/DynamicsValue/fake-xrm-easy/issues/115
- build.ps1 improvements: do not build project twice (added --no-build) when running dotnet test, do not build again either when packing assemblies either: https://github.com/DynamicsValue/fake-xrm-easy/issues/119
- Update build scripts to use 'all' target frameworks by default - https://github.com/DynamicsValue/fake-xrm-easy/issues/126
- Update github actions to use new Sonar environment variables - https://github.com/DynamicsValue/fake-xrm-easy/issues/120

## [3.3.3]

### Added

- Introduced new user-defined exceptions to check whether an early-bound type is defined in multiple assemblies.
- New method to retrieve early bound types based on EntityTypeCode.

### Changed

-  Update namespaces in tests project for consistency
 - Upgraded GitHub Actions to update Java major version to run SonarCloud analysis - https://github.com/DynamicsValue/fake-xrm-easy/issues/110
 - Introduced new NewEntityRecord method to easily create instances of entity records based on the current use of early-bound or late-bound entities 
 - Resolves an issue with query evaluation and MultiOptionSets when using late bound entities or if type information is not present. - https://github.com/DynamicsValue/fake-xrm-easy/issues/66
 - Update legacy CRM SDK 2011 dependency to use official MS package - https://github.com/DynamicsValue/fake-xrm-easy/issues/105

## [3.3.1]

### Changed

- Bump DataversClient nuget package to use net6.0 - https://github.com/DynamicsValue/fake-xrm-easy/issues/90

## [3.3.0]

### Changed

- Adding tests for MetadataGenerator - https://github.com/DynamicsValue/fake-xrm-easy/issues/77
- Moved GetContextFromSerialisedCompressedProfile method into the Plugins package and FakeXrmEasy.Plugins namespace

## [3.2.0]

### Changed

- Added extension methods to detect and execute generic CRUD requests (of type OrganizationRequest) - DynamicsValue/fake-xrm-easy#31
- Replaced references to PullRequestException by references to UnsupportedExceptionFactory to make it easier raising requests based on the license context
- **BREAKING**: In-Memory data dictionary that was defined as a public dictionary is now internal using a rewritten data structure to prepare for parallelization and concurrency. If you were accessing this property, please use the GetEntity or CreateQuery public methods in the IXrmFakedContext interface to query the state of the In-Memory database state instead. For any other use, always rely on the IOrganizationService* interfaces only. This breaking change will affect you only if you were accessing the 'Data' dictionary directly.
- Fix Sonar Quality Gate settings: DynamicsValue/fake-xrm-easy#28

## [3.1.2]

- Bump dataverse dependency to production ready version 1.0.1
## [3.1.1]

### Changed

- Limit FakeItEasy package dependency to v6.x versions - DynamicsValue/fake-xrm-easy#37
- Updated build script to also include the major version in the Title property of the generated .nuspec file - DynamicsValue/fake-xrm-easy#41
- Modified TopCount support in QueryByAttribute and QueryExpression, to not throw exception if PageInfo was set but empty: DynamicsValue/fake-xrm-easy#16
- Do not clear previous FakeMessageExecutors or GenericFakeMessageExecutors when adding new ones or when calling them multiple times: DynamicsValue/fake-xrm-easy#15
- Allow creating records with any statecode attribute, which will be overriden by the platform as Active - DynamicsValue/fake-xrm-easy#36
- Both GetEntityById and GetEntityById&lt;T&gt; now clone the entity record before returning it - DynamicsValue/fake-xrm-easy#27

## [3.1.0]

### Changed

- Added TopCount support in QueryByAttribute, and throw exception if both TopCount and PageInfo are set: DynamicsValue/fake-xrm-easy#16
- Removed .netcoreapp3.1 target framework in versions 2.x, it'll be supported from versions 3.x onwards. Bump

## [3.0.2]

### Changed 

- Bump Dataverse dependency to 0.6.1 from 0.5.10 to solve DynamicsValue/fake-xrm-easy#20
- Also replaced Microsoft.Dynamics.Sdk.Messages dependency, as it has also been deprecated by MSFT, to Microsoft.PowerPlatform.Dataverse.Client.Dynamics 0.6.1 DynamicsValue/fake-xrm-easy#20

## [3.0.1-rc1] - Initial release

