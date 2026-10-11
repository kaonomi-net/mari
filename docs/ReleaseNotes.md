Release Notes
===

[![Nuget M-A-R-I](https://img.shields.io/nuget/dt/mari.framework.core.svg?label=Nuget%20MARI%20Framework%20downloads)](https://www.nuget.org/packages?q=mari)

# Released

## 1.0.0

In this version we mainly:
- Integrate the toolbox inside the repository: **Elvex.Toolbox** becomes **MARI.Toolbox**
- Restore the **Artifacts** sources (excluded by the `.gitignore` rule `artifacts/`)
- Switch the samples on the framework sources

### Features
- **Toolbox**
    - The toolbox sources are now in [src/Toolbox](/src/Toolbox/) and part of `MARI.sln` (solution folders *Toolbox*, *Toolbox/Tests*, *Toolbox/Benchmarks*)
    - Projects: `MARI.Toolbox`, `MARI.Toolbox.Abstractions`, `MARI.Toolbox.Configuration`, `MARI.Toolbox.WPF`, `MARI.Toolbox.WPF.Abstractions`, `MARI.Toolbox.WPF.UI`, `MARI.Toolbox.WPF.UI.Avalon`, unit tests, test toolkit and benchmark
    - The framework uses them through project references, the nuget packages `Elvex.Toolbox*` are no more used

- **Samples**
    - The samples reference the framework sources (project references) instead of the published nuget packages
    - Samples updated to the current API (trigger output set before the targets, `Trigger.Stream` with a SNI, `UseMongo`, lower case SNI, ...)

### Fix
-   **Build**: Restore the missing `Artifacts` folders (Core.Abstractions, Node.Abstractions, Builders, Node), the `.gitignore` rule is now `/artifacts/` (root folder only)
-   **Blackboard**: The internal signal `BlackboardAnalyzeFeedbackSignal` had an invalid SNI, any blackboard usage crashed at startup. Its SNI is now `blackboard-analyze-feedback-signal` (same Uid)

### Breaking Changes
>[CAUTION]
> - **Toolbox namespace** : `Elvex.Toolbox.*` becomes `MARI.Toolbox.*` (namespaces, assemblies, packages)
> - **Toolbox types renamed** :
>     - `IElvexJsonObjectConverter` -> `IMARIJsonObjectConverter`
>     - `IElvexJsonObjectConverterConditions` -> `IMARIJsonObjectConverterConditions`
>     - `ElvexJsonObjectPropertyMatchConverterConditions` -> `MARIJsonObjectPropertyMatchConverterConditions`
>     - `TypeSolverElvexJsonConverter` -> `TypeSolverMARIJsonConverter`
>     - `ElvexResources` -> `MARIResources`, `VisualElvexTreeHelper` -> `VisualMARITreeHelper` (WPF)
> - **XAML namespace** : `http://elvex.toolbox.ui/` becomes `http://mari.toolbox.ui/`
> - **Global temporary folder** : `%TEMP%\Elvex` becomes `%TEMP%\MARI`
> - **Serialized data** : data that stores type names (ex: definitions in a database) with `Elvex.Toolbox...` must be migrated or regenerated

# Beta

## 0.5.0-prerelease

In this version we mainly:
- Optimize architecture around repository declaration and creation
- Fix bugs, improve testing
- New feature call [RefId](#050-prerelease/feature/refid)
- New feature call Yaml [Compilation](#050-prerelease/feature/yaml-compilation)

### Features
- **Artifact**
    - ***Arguments*** : 
        - You can now differenciate command line argument for the launcher than launched script
        - Choose the charactere that will be used in command line between the argument name and his value, by default is ':'. Example : --port:4242

- **RefId**
    - Using attribute and code genertion we tag each VGrain with a unique "SNI" (Simple Name Identifier) and a namespace through an URI used as user friend identifier
        - URI Example: ***ref://vgr@bag.text/text-toolbox*** is the unique reference to a vgrain (interface) in the namespace "bag.text" with a SNI "text-toolbox"
    - A ref id could target:
        - VGrain interface
        - VGrain Implementation
        - Type (class, struct)
        - Methods
        - Definitions (Sequence, Trigger, Signal, ...)
        - Blackboard controller

- **Yaml Compilation**
    - Compile Yaml into definitions
    - Schema definition used to help yaml creation [Schema](https://github.com/kaonomi-net/mari/docs/mari.schema.json)
    - Support definitions
        - Sequence
        - Trigger (Cron, Signal)
        - Signal
        - StreamQueue

- **Auto-Configuration**
    - Schema definition to help configuration [Schema](https://github.com/kaonomi-net/mari/docs/configuration-node.schema.json)
    - Add logs to better understands

### Fix
-   **Repository**
    - ***Mongo***: Discriminator filter prevent some correct match 
-   **Artifact**: We are currently integrating other langugage like java and minor internal bug was revealed
    - ***Docker***: Fix image creation
-   **Auto-Configuration** : Fix configuration through json file, extension loading ...
-   **Portability**: Replaces Named Semaphore with Mutex: Suggests using a Mutex as a more portable alternative for synchronization.

### Breaking Changes
>[CAUTION]
> - **Repository** : Due to artitecture change some custom implementation risk to be impacted

## 0.4.4-prerelease

This update focuses on stability and bug fixes. <br/>
We've enhanced our development process with new testing and automation tools to prevent regressions and improve overall quality.<br/>

Please note that **Amexio** group is now supporting this projet.

## 0.4.3-prerelease

In this version we mainly:
- Enhance Blackboard usage
- Managed result after processing
- Stabilisation

### Features
- **Blackboard** : 
    - **Query** : Able to send query to blackboard, A type Request to have information or a type command to give order without response to the blackboard.
    - **Life Status** : Able to define a lifecycle status to request initialization or sealed with specific result when job is done.
    - **Signal** : Able to subscribe, received and unsubscribe to signals.

- **Sequence** :
    - **Extend Call** : Able to use property access in the call part, example : (grain, input, ctx) => grain.MethodAsync(input.Property.SubProperty, ctx).

- **Signal Hierarchy** : Able to create a hierarchy or signal, simply when a child fire it will automatically fire the parent.
- **Execution Handler** :
    - **Signal** : Add a signal information that need to be fire at the end of a execution.
    - **Deferred** : Able let process continue in background and use the deferred system let you know when the job is done
- **Meta Data** : Able to add description, tags, category information on descriptor like (Sequence, Trigger, ...), those meta data are used to "Understand" the goal
- **Artifact** : Able to setup an execution environment. Support Docker

### Breaking Changes
>[CAUTION]
> - **Artifact** : External execution had an issue during the message serialization. The protocol have been improved and change a bit.<br />
>                  Python mari librairy MUST be updated

### Fix
- **node** :
    - **Deco/Reco** : With mongo db as cluster theire was a singleton issue
    - **Restore** : 

## 0.4.1-prerelease

In this version we mainly focus on dynamic feature :
- Definitions
- Redirections

### Features
- **Redirections** : Able to change the implementation used during a sequence resolution. In scope local and global.
- **Sequence Stages Call** : Extend sequence stages.
    - **Sequence** : Be able to call a sequence from another sequence.
    - **Select** : Be able to manipulate the input to select a sub element or provider one.
- **Dynamic Definitions** : Be able to inject definition as runtime

### Breaking Changes
>[CAUTION]
> - **ReturnNoMessage** : Renaming end stage method from "ReturnNoMessage" to "ReturnNoData".

### Fix
- **Trigger** : Fix trigger activation and desactivation based on definition.
- **Repository** : Auto-configuration default mari repository


# Alpha

## 0.3-prerelease

In this version we mainly:
- Add blackboard extension
- Add Stream triggers to send and consumed
- Sequence feature Foreach/FireSignal
- Storage through repository

### Features
- **Blackboard** : Extension in charged to create a controlled shared memory.
- **Stream** : Trigger used to push to an orlean stream & trigger fire by message present in a stream
- **Sequence** 
    - **Foreach** : Be able to process through a foreach loop a sub-collectionn the model current data processed
    - **Fire Signal** : Be able to fire signal directly from a sequence
- **Trigger Cron** : Support second as minimal timer
- **Trigger Output** : Allow each trigger output to have a custom output value
- **Repository** : Simple storage space used to store information like state way but simpler without container

### Breaking Changes
>[CAUTION]
> - **rename(Bag)** : Rename debug tools and web tools VGRAINS to Bag terminologie <br />
> - **rename(Mongo)** : Rename extensions about storage in mongo from MARI.Framework.Node.Mongo to   MARI.Framework.Extensions.Mongo<br />
> - **Definition** : Force display name to be set <br />
> - **rename(SetInput)** : Rename SetInputValue to SetOutput<br />
> - **Move(TypeArgument)** : Move TypeArgument and EnumerableHelper in the abstractions assembly to allow more usage
> - **rename(Configuration)** : Changing configuration name from 'AddInMemoryMongoDefinitionProvider' to 'AddInMemoryDefinitionProvider' to remove mongo missplaced name

### Fix
- **ConcretType** : CollectionType was not tolerate as concret type

## 0.2.2-prerelease

In this version we mainly:
- Add Python VGrain support

### Features

- **Artifact** : Extended resource needed to mari node to work (code, png, licences, pfx ...)
    - Create, build and add artifact
    - Define code artifact to be used as grain

- **Code Artifact** : Artifact use to configure the external code, for example python scripts
- **External Code Remote controller** : Controller using artifact code definition to launch, maintain and communicate to another program
- **Python Package** : Package deployed on [Pypi](https://pypi.org/project/mari/) and [TestPypi](https://test.pypi.org/project/mari/) (cf. [Python/README.md](/src/Extensions/Dist/Python/Lib/README.md))

## 0.2.1-prerelease

In this version we mainly:
- Fix serialization issues
- Add extension to use MongoDB as external storage
- Add new door mechanism
- Add samples

### Features
- **Configuration**: Allow MARINode to be setup directly on existing IHostBuilder
- **Door** Create a door with condition based on source signal
    - Create builder RelayFilterDoor based on conditional expression serializableCreate builder RelayFilterDoor based on conditional expression serializable

- **Data Model**: Add IDefinition that introduction model validation

- **External Storage**: Add mongo extension to be used as storage for
    - **Cluster**: Using mongodb as meeting point, configuration could be done by config file, manually, auto
    - **Reminders**: Use mongodb as storage for reminders
    - **VGrain State** : Configuration to use MongoDB as VGrain state storage. Configure using mongo for alll storage
    - **Defintions** : Allow mongo to be a definitions provider. Could be configured inline or through config file.

- **Toolbox** : 
    - Add Expression serialization of math operator
    - Create AbstractType and AbstractMethod object to be able serialize type and method info.

- **Debug** : Extension used to provide debug sequences, vgrain ... to display information more easily  
- **Sequence** : In a sequence you can add some configuration to customize the context of the grain called (Often use to provide variable for grain id generation).
You can now setup dynamically the configuration based on the input data.

### Breaking Changes
>[CAUTION]
- **Data Model**: In sequence and other model that use the .net "Type". We have Change "Type" to "AbstractType" to easy serializations
- **Namespace**: Some configuration namespace have been changed to simplify it with two root "MARI.Framework" and "MARI.Framework.Configuration"
- **StartUntilEndAsync**: Now have a CancellationToken in third parameter

### Fix
- **Signal**: Change Signal input storage from only one not consumed to ordered linked list to support many
- **SequenceExecutor**: Sequence executor state change through surrogate

### Tests
- **Serialization** : Add lot of test to ensure surrogate or definition are serializable at least in json.
- **Door** : Add door unit testes to ensure signal handling is well performed.
- **Toolbox** : Many tool used in the framework core are now more unit tested.

### Others
- **Signal**:  Add serialization test on signal grain state to ensure save and restore
- **Door**: Test door behavior subscription and signal management
- **Configuration**:
    - Simplify "InMemory" definition configuration, register multiple definitions, register directly at root level
    - Add Debug tool injection though one line of configuration

## 0.2.0-prerelease

- **Definition Provider** : Architectured to allow multiple provider and in-memeory one build-in
- **Sequences** : Definition and execution.
- **Signal** : Definition, Subscribe and Fire
- **Door** : Definition, Subscribe and Fire
    - **Boolean logical Door** : Definition, Subscribe and Fire
- **Triggers** : Definition and fire
    - **Cron** : Definition and fire
    - **Signal** : Definition and fire
