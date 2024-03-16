## [2.5.0]

### Added

- Added FileAttributeMetadata support to MetadataGenerator 
- Added support for bulk operations: CreateMultipleRequest, UpdateMultipleRequest
- Added new exception to make the initialization of entity records with attributes with a null entity reference more obvious (thanks Betim) - https://github.com/DynamicsValue/fake-xrm-easy/issues/107

### Changed

- Resolves referencing EntityAlias or EntityName in conditions inside nested filters of a LinkedEntity (thanks Temmy) - https://github.com/DynamicsValue/fake-xrm-easy/issues/63
- Resolves Resolving entity references by Alternate Keys when EntityMetadata is used that doesn't have any Keys. - https://github.com/DynamicsValue/fake-xrm-easy/issues/138

## [2.4.2]

### Added

- Added link to docs in exceptions
- Added some logging in subscription usage for CI builds

## [2.4.1]

### Changed

- Should not read subscription usage while in a CI build

## [2.4.0]

## Added

- **Alpha**: Introduced subscription usage monitoring based on customer feedback

### Changed

- Set default build configuration in solution file to FAKE_XRM_EASY_9
- Remove ReleaseNotes from package description - https://github.com/DynamicsValue/fake-xrm-easy/issues/115
- build.ps1 improvements: do not build project twice (added --no-build) when running dotnet test, do not build again either when packing assemblies either: https://github.com/DynamicsValue/fake-xrm-easy/issues/119
- Update build scripts to use 'all' target frameworks by default - https://github.com/DynamicsValue/fake-xrm-easy/issues/126
- Update github actions to use new Sonar environment variables - https://github.com/DynamicsValue/fake-xrm-easy/issues/120

## [2.3.3]

### Added

- Introduced new user-defined exceptions to check whether an early-bound type is defined in multiple assemblies.
- New method to retrieve early bound types based on EntityTypeCode.

### Changed

-  Update namespaces in tests project for consistency
 - Upgraded GitHub Actions to update Java major version to run SonarCloud analysis - https://github.com/DynamicsValue/fake-xrm-easy/issues/110
 - Introduced new NewEntityRecord method to easily create instances of entity records based on the current use of early-bound or late-bound entities 
 - Resolves an issue with query evaluation and MultiOptionSets when using late bound entities or if type information is not present. - https://github.com/DynamicsValue/fake-xrm-easy/issues/66

## [2.3.2]

### Changed

- Update legacy CRM SDK 2011 dependency to use official MS package - https://github.com/DynamicsValue/fake-xrm-easy/issues/105

## [2.3.0]

### Changed

- Adding tests for MetadataGenerator - https://github.com/DynamicsValue/fake-xrm-easy/issues/77
- Moved GetContextFromSerialisedCompressedProfile method into the Plugins package and FakeXrmEasy.Plugins namespace

## [2.2.0]

### Changed

- Added extension methods to detect and execute generic CRUD requests (of type OrganizationRequest) - DynamicsValue/fake-xrm-easy#31
- Replaced references to PullRequestException by references to UnsupportedExceptionFactory to make it easier raising requests based on the license context
- **BREAKING**: In-Memory data dictionary that was defined as a public dictionary is now internal using a rewritten data structure to prepare for parallelization and concurrency. If you were accessing this property, please use the GetEntity or CreateQuery public methods in the IXrmFakedContext interface to query the state of the In-Memory database state instead. For any other use, always rely on the IOrganizationService* interfaces only. This breaking change will affect you only if you were accessing the 'Data' dictionary directly.
- Fix Sonar Quality Gate settings: DynamicsValue/fake-xrm-easy#28
 

## [2.1.1]

### Changed

- Made CRM SDK v8.2 dependencies less specific - DynamicsValue/fake-xrm-easy#21
- Limit FakeItEasy package dependency to v6.x versions - DynamicsValue/fake-xrm-easy#37
- Updated build script to also include the major version in the Title property of the generated .nuspec file - DynamicsValue/fake-xrm-easy#41
- Modified TopCount support in QueryByAttribute and QueryExpression, to not throw exception if PageInfo was set but empty: DynamicsValue/fake-xrm-easy#16
- Do not clear previous FakeMessageExecutors or GenericFakeMessageExecutors when adding new ones or when calling them multiple times: DynamicsValue/fake-xrm-easy#15
- Allow creating records with any statecode attribute, which will be overriden by the platform as Active - DynamicsValue/fake-xrm-easy#36
- Both GetEntityById and GetEntityById&lt;T&gt; now clone the entity record before returning it - DynamicsValue/fake-xrm-easy#27

## [2.1.0]

### Changed

Added TopCount support in QueryByAttribute, and throw exception if both TopCount and PageInfo are set: DynamicsValue/fake-xrm-easy#16
Removed .netcoreapp3.1 target framework in versions 2.x, it'll be supported from versions 3.x onwards.
Bump Microsoft.CrmSdk.CoreAssemblies to version 9.0.2.27 to support plugin telemetry - DynamicsValue/fake-xrm-easy#24

## [2.0.1-rc1] - Initial release