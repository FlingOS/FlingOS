---
layout: reference-article
title: Drivers Compiler
date: 2015-07-29 11:32:00
categories: docs reference
---

## IL to Machine Code

The Drivers Compiler's sole purpose is to take IL code and to convert it into machine code. It does so via the following steps (the sequence of conversions are shown in increasing levels of detail).

``` bash
IL                          ->                         Machine Code
IL              ->          ASM         ->             Machine Code
IL   ->   Information / Structure   ->   ASM    ->     Machine Code
IL  ->  Info  ->  IL Blocks  ->  ASM Blocks  ->  Object Files  ->  ISO/ELF
IL  ->  Info  ->  IL Blocks (/IL Ops)  ->  ASM Blocks (/ASM Ops)  ->  Object Files  -> ISO/ELF
```

---

## Description of key classes / structure

**Note**: The compiler's namespace/class-naming convention matches the folder structure. For example, `Drivers.Compiler.IL.ILOp` refers to a class called `ILOp` found in a file called `ILOp.cs`. This file will be inside the `Kernel\Drivers\Compiler\IL\` folder.

It will also be found (in Visual Studio) under the `Drivers` folder in the `Drivers.Compiler` project. The project names match their namespaces.

---

**Note**: It is helpful to think of "types" as "descriptions of objects" and "objects" as "allocations of heap memory which conform to a given description".

---

**Note**: There are two kinds of class in the Drivers Compiler. There are classes for *doing things* and classes for *storing data* (often these names end in `Info`).

Classes for doing things are usually `static` and contain `static` methods (often named like `ExecuteXYZ`). Classes for storing data usually contain no methods and have the data given to them.

---

### Drivers.Compiler.App
* **CompilerProcess** - The main entry-point for the compiler when it is being used as a command-line application.
   This class handles calling the compiler steps as described in the sequence of conversions above. The exact list is:
   
   1. Validate command line options
   2. Load the *IL Library* (includes the *Information/Structure* conversion step)
   3. Call the *IL Compiler* (handles *IL Block* to *ASM Block* conversion)
   4. Call the *ASM Compiler* (handles *ASM Block* to *ASM file* to Object file conversion)
   5. Call the *Link Manager* (handles *Object File* to *ISO/ELF* conversion)

### Drivers.Compiler.MSBuild
* **`BuildTask`** - The main entrypoint for the compiler when it is being used as a build task for MSBuild.
  The Build Task is started by MSBuild immediately after MSBuild finishes converting C# into IL and after it has created the `.dll` file (which is the *IL Library*).

  The steps taken by the BuildTask are identical to those of the CompilerProcess in Drivers.Compiler.App.

### Drivers.Compiler

* **`Types` Namespace** - (`Kernel\Drivers\Compiler\Types` folder)

  This contains all the classes (and most of the methods) that the compiler uses to convert a `.dll` file into *information objects*.

  *Information objects* include `FieldInfo`, `MethodInfo`, `TypeInfo` and `VariableInfo`. They are used by the *Drivers Compiler* to keep track of information about everything the compiler has to convert. They are discussed in more detail below. There is one additional class called `TypeScanner` in this namespace.

	* **`TypeScanner`** - This static class has all the methods for reading an *IL Library* (a `.dll` file) and loading all its information. It loads the information, creates a new information object and then populates that new object with the information.
		The `TypeScanner` is also capable of determining some information from what it loads. The three things it determines relate to information about types. It determines:

		* The size of an instance of a given type if it were referred to on the stack
		* The size of an instance of a type when it is allocated in memory (i.e. how much memory a call to `memalloc` would need to request to have enough to match the given type)
		* Whether the type is managed by the *Garbage Collector* or not.

		In general, value types have the same size on the stack and the heap. Value types are not managed by the garbage collector. In contrast, reference types are 4 bytes on the stack (size of a pointer/address in 32-bit architecture) and their normal size on the heap. All reference types are managed by the garbage collector. Pointer types are treated as integer value types and so follow all the same processing steps as value types.

	* **`FieldInfo`** - Instances of `FieldInfo` are created by the Drivers Compiler to keep track of information about the fields of a type. 
		Note that fields are something like:
	  
		```
		class ClassWithAField
		{
			int ThisIsAField;
		}
		```

		Fields are distinct from properties. Properties are not fields. Properties use `get`/`set` and look something like:

		```
		class ClassWithAProperty
		{
			int ThisIsAProperty { get; set; }
		}
		```

		Field Info keeps track of some very simple data:
		
		* **`FieldType`** - The type of the field. In `ClassWithAField` above, the type of the `ThisIsAField` field is `int`.
		* **`IsStatic`** - Whether the field is `static` or not. Static fields are treated differently to normal ('instance') fields. Static fields are pre-initialised and have separate memory allocated by the compiler as part of the final machine code. Static fields are only allocated once. Instance fields, on the other hand, are allocated memory each time a new instance of the type is created, so they contribute to a type's size on the heap. Each separate instance of a type (i.e. each separate memory allocation) has its own values for instance fields, but the same value for static fields.
		* **`OffsetInBytes`** - This only applies to instance fields (i.e. non-static fields). This is the offset, from the start of the object when it is in heap memory, to the start of the field. It is equivalent to the sum of all the sizes of the fields which precede it in memory.
		* **`ID`** - This is a string which uniquely identifies the field or static field. It can be used as a label in the output ASM code.
		* **`Name`** - This is a string which is a human readable name for the field. It is the name given in the C# code.

		* **`MethodInfo`** - Instances of `MethodInfo` are created by the Drivers Compiler to keep track of information about the methods of a type. This includes all `static`, non-`static`, `private`, `public` and `abstract` or `virtual` methods.

		`MethodInfo` keeps track of the following data:
		
		* **`PlugAttribute`** - If a `PlugMethodAttribute` was applied to the method, then this field contains the information about that attribute. Otherwise, this field is `null`.
		* **`ArgumentInfos`** - A list of `VariableInfo`s which describe all the arguments for the function (including the hidden `this` argument).
		* **`LocalInfos`** - A list of `VariableInfo`s which describe all the local variables used within the method. This list includes hidden/extra local variables that don't appear in the C# code but which are created by *MSBuild*.
		* **`ApplyGC`** - Whether to apply the garbage collector inside the method or not. Use of the garbage collector causes the compiler to insert extra IL operations. If `ApplyGC` is false, the compiler will not add the extra GC-related IL operations. This will be set to `false` if the `NoGC` attribute is applied to the method.
		* **`ApplyDebug`** - Whether to apply debug operations inside the method or not. In debug build mode, the compiler will inject extra IL and ASM operations used for debugging at runtime. If `ApplyDebug` is `false`, the compiler will not add the extra debug-related IL and ASM operations. This will be set to false if the `NoDebug` attribute is applied to the method.
		* **`IsPlugged`** - Whether the method is plugged or not.
		* **`IsConstructor`** - Whether the method is a constructor or not.
		* **`IsStatic`** - Whether the method is `static` or not.
		* **`MethodBody`** - The method body of a method contains the bytes which represent the method's IL code.
		* **`ID`** - A string that uniquely identifies the method. This is also used as a label for the method in the ASM code.
		* **`Signature`** - A unique but human-readable string for the method. Cannot be used as a label but is used to generate the ID.
		* **`IDValue`** - A unique integer identifier for the method. This is used in the method tables.
		* **`Priority`** - The priority of the method. Zero by default. Set using the priority attribute. Used for ordering the output assembly code/machine code during the *linker* stage.
		* **`Preprocessed`** - Whether the method has undergone preprocessing or not. This is used to prevent the method being preprocessed multiple times!

	* **`TypeInfo`** - Instances of `TypeInfo` are created by the Drivers Compiler to keep track of information about all the types that are in the library(/libraries) being compiled.
		`TypeInfo` keeps track of the following data:
		
		* **`FieldInfos`** - A list of `FieldInfo` objects representing all the fields declared in the type.
		* **`MethodInfos`** - A list of `MethodInfo` objects representing all the methods declared in the type.
		* **`Processed`** - Whether the type's data (excludes methods and fields) has been processed or not.
		* **`ProcessedFields`** - Whether the type's fields have been processed or not.
		* **`IsValueType`** - Whether the type is a value type or not.
		* **`IsPointer`** - Whether the type is a pointer type or not.
		* **`SizeOnStackInBytes`** - The size of the type when it is represented on the stack. See description in `TypeScanner`.
		* **`SizeOnHeapInBytes`** - The size of the type when it is represented on the heap. See description in `TypeScanner`.
		* **`IsGCManaged`** - Whether the type is managed by the garbage collector. `true` for reference types (heap), `false` for value types (stack). Occasionally this value is altered from that rule.
		* **`ID`** - A unique string identifier for the type. This is also used as a label for the type in the ASM code.

	* **`VariableInfo`** - Instances of `VariableInfo` are created by the Drivers Compiler to keep track of information about variables (locals or arguments) that exist within/for a method.
		`VariableInfo` keeps track of the following data:

		* **`TheTypeInfo`** - Information about the type of the variable.
		* **`Position`** - The position of the variable as an index. It is the index in the list which contains the variable info.
		* **`Offset`** - The offset in bytes from the Base Pointer (under x86, this is EBP).


* **`IL` Namespace** - (`Kernel\Drivers\Compiler\IL` folder)

  This contains all of the classes (and most of the methods) related to taking IL operations and converting them into ASM operations. However, it does not (or should not) include any code which produces architecture-specific assembly operations. Any code related specifically to converting a specific IL op into ASM ops for a particular architecture goes in a *target architecture library*. For example, the `Drivers.Compiler.Architectures.x86_32` library is used to convert to 32-bit x86 ASM.

    For information about the `IL.ILOps` namespace (`Kernel\Drivers\Compiler\IL\ILOps` folder), see the next section.

    The `IL` namespace contains the following classes:

    * **`ILOp`** - This class has dual purposes. These purposes are independent and so are described individually below.

		The first purpose of the `ILOp` class is as a data class. It stores data about IL operations. IL operations are like a high-level assembly code. They are stored as bytes in a byte array in a method. See `ILBlock` for details.

		The second purpose of the `ILOp` class is as a base class for IL op converter implementations. There are a significant number of different IL ops and each of them inherits from the base `ILOp` class. The `ILOp` class declares an `abstract` method called `Convert` which must be implemented by the second-level child classes of `ILOp`. The child classes of `ILOp` go two levels deep. The first level is contained within the Drivers Compiler (in the `IL.ILOps` namespace), the second level is contained in a *target architecture library*.

		The first level of inheritance from the `ILOp` class is in the `IL.ILOps` namespace. There are many similar IL ops - for example `Starg` and `Starg_S`. These groups of similar IL ops can be handled together by the *target architecture library*, so the first-level child classes (which are still in the *Drivers Compiler*) group the IL ops togther. This results in a smaller set of 'basic' IL ops that the *target architecture library* can implement in the same class.

		These first-level classes do not implement the `Convert` method since they are in the *Drivers Compiler*, and the *Drivers Compiler* does not directly handle architecture-specific conversion. Because the `Convert` method is not implemented, the first-level child classes remain `abstract` and so you cannot actually create instances of them.

		The second-level child classes classes are found in a *target architecture library* such as `Drivers.Compiler.Architectures.x86_32`. These classes inherit from the first-level classes. The second-level classes implement the `Convert` method and handle the actual conversion of the IL Op into ASM Ops. The ASM Ops produced are also found in the *target architecture library*.

		The `ILOp` class also declares an `abstract` method called `PerformStackOperations` which is used by the `ILPreprocessor` prior to the `ILScanner` calling the `Convert` method. The *Drivers Compiler* keeps track of descriptions of what is on the stack during processing of a method. The `PerformStackOperations` method performs all the same stack operations as the `Convert` method but without outputting any actual assembly code.

	* **`ILBlock`** - This class is used to represent a block of IL code. A "block of IL code" is exactly equivalent to a method in C#. The compiler uses `MethodInfo` objects to store information about the methods being compiled but does not store the list of IL ops for the method. This is where the `ILBlock` comes in. The IL Block stores a list of all the IL Ops for the method and a reference to the `MethodInfo` for which the IL Block was generated. An IL Block also keeps track of some additional data related to converting the block from IL to ASM.
		An `ILBlock` contains the following data:
	
		* **`PlugPath`** - Path to the ASM file to use as a plug for the IL block. `null` if the block is not plugged.
		* **`IsPlugged`** - Whether the block is plugged or not.
		* **`ILOps`** - The list of IL Ops in the block which make up the method. These are loaded by the *IL Reader* from the byte array in `MethodInfo.MethodBody`. The IL Ops list is subsequently modified by the ILPreprocessor to add (and occasionally remove) some IL operations. This happens prior to the ILScanner converting the IL Ops into ASM ops.
		* **`ExceptionHandledBlocks`** - The list of try-catch-finally blocks for the method.

    * **`ILLibrary`** - This class is used to represent a library being compiled. An *IL Library* originates from a .dll file. An *IL Library* primarily holds information about the types and all methods within the library. It also holds information about library dependencies i.e. libraries which are referred to be the library in question.
		An *IL Library* contains the following data:

		* **`TheAssembly`** - The Assembly object which represents the (.dll) file from which the *IL Library* is loaded.
		* **`TheASMLibrary`** - The ASM Library to which the *IL Library* will be or has been compiled.
		* **`Dependencies`** - All direct dependencies of the library.
		* **`TypeInfos`** - List of all the information about all the types in the library.
		* **`ILBlocks`** - A dictionary mapping all the information about methods (from all types) in the library to their respective IL Blocks.
		* **`SpecialClasses`** - This is a dictionary which maps compiler-specific attributes (which flag special classes such as the replacement Object and String classes) to the TypeInfo(s) of the classes to which the attributes were applied. Special classes are used by the compiler to replace .Net Framework classes.
		* **`SpecialMethods`** - This is a dictionary which maps compiler-specific attributes (which flag special methods such as the replacement Throw Exception method) to the MethodInfo(s) of the methods to which the attributes were applied. Special methods are used by the compiler to replace .Net Framework methods or to complete compiler-required operations.
		* **`ILRead`** - *(past tense)* Indicates whether the IL Reader has been executed for the library or not.
		* **`ILPreprocessed`** - *(past tense)* Indicates whether the IL Preprocessor has been executed for the library or not.
		* **`ILScanned`** - *(past tense)* Indicates whether the IL Scanner has been executed for the library or not.
		* **`TheStaticConstructorDependencyTree`** - Tree describing the dependencies between static constructors. This is described in more detail in the *"Detailed points of operation"* section.
		* **`StringLiterals`** - A dictionary mapping IDs to values for all the string literals declared within the library.

		The *IL Library* class also contains a number of methods for accessing the contained data which take into account data from dependencies.

    * **`ILPreprocessor`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Pre-process all methods (plugged or not) for basic set of information such as information about arguments
        * Pre-processing for special classes / methods
        * Pre-processing of static constructors
        * Pre-scan IL ops to:
            *  Type Scan any local variables which are of otherwise an unscanned types (i.e. value types that are from the .Net Framework)
            * Inject general ops (method start, method end, etc.)
            * Inject Garbage Collector IL ops
            * Inject wrapping try-finally for GC
            * Inject IL ops for try-catch-finally

		These steps are described in more detail in *"Key points of operation"* and *"Detailed points of operation"*.

    * **`ILScanner`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Loading the target architecture library
        * Generating ASM Blocks for the following:
            * Static fields
            * Type Tables
            * Method Tables
            * Field Tables
            * Plugged IL Blocks
            * Non-plugged IL Blocks

		These steps are described in more detail in *"Key points of operation"* and *"Detailed points of operation"*.

    * **`ILCompiler`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Invoking the *IL Reader*
        * Invoking the *IL Preprocessor*
        * Invoking the *IL Scanner*

		This class is little more than a wrapper to ensure the respective IL processing steps are called in the correct order.

    * **`ILConversionState`** - This class represents the state of a method (such as items on the stack) while it is being converted. It is passed by the *IL Scanner* to the `Convert` method of the second-level inheritance IL Ops of the *target architecture library*. The target architecture library's IL ops can then use it to keep track of necessary state information that must be passed between `Convert` calls.

    * **`ILPreprocessState`** - A stripped-down version of the `ILConversionState` state used for the *ILOp.PerformStackOperations* method which is called by the ILPreprocessor.

    * **`ExceptionBlock`** - Used for storing information about try-catch-finally blocks. The exact data stored in this is discussed in more detail in the *"Detailed points of operation"* section.

    * **`StaticConstructorDependency`** - Describe a dependency between static constructors. This is discussed in more detail in the *"Detailed points of operation"* section.

* **`IL.ILOps` Namespace** (`Kernel\Drivers\Compiler\IL\ILOps` folder) - The *IL.ILOps* namespace contains all the first-level inheritance `ILOp` classes. They define all the actual IL ops supported by the compiler but do not contain the actual `Convert` or `PerformStackOperations` method implementations. The classes are always for the basic form of the IL ops. Attributes applied to those classes allow the classes to specify which exact variants of the IL op they support.

* **`ASM` Namespace** (`Kernel\Drivers\Compiler\ASM` folder and `Kernel\Drivers\Compiler\ASMOps` folder) - This contains all the classes (and most of the methods) related to taking ASM operations and converting them into machine code. However, it does not (or should not) include any code which handles architecture specific ASM operations nor architecture specific external build tools (such as NASM).

    The *ASM* and *ASM.ASMOps* namespaces contain the following classes:

    * **`ASMOp`** - This class has dual purposes (in a similar way to the way `ILOp` had dual purposes). These purposes are independent so are described individually below.

		The first purpose of the `ASMOp` class is as a data class. It stores data about ASM operations. ASM operations represent individual lines of assembly code (including comments, so an `ASMOp` might not actually relate to an "operation" pre-se).

		The second purpose of the `ASMOp` class is as a base class for ASM op converter implementations. Like the `ILOp` class, there are two levels of inheritance. The first is contained within the *Drivers Compiler* and specifies some of the specific "ops" the compiler needs to be able to use (such as comments and labels). The second level of inheritance is contained in the *target architecture library*. These second-level classes implement the `Convert` method which converts the ASM op into ASM text. The ASM text is later written out to a .asm file which is then fed to a compile tool such as NASM to produce the object files / machine code.

		The `ASMOp` class works in much the same way as the `ILOp` class except that:

		1. The `Convert` method does not have any state tracking so all ASM ops must be converted independently.
		2. The `ASMOp` class does not define a preprocessor method (`ILOp` defines `PerformStackOperations` which is a preprocessor method).

		The `ASMOp` class keeps track of very little information (since very little is needed by this point in the compiler steps). The following data is kept track of:

		* **`ILLabelPosition`** - The `Position` of the `ILOp` which generated this ASM op. -1 by default indicating that either the ASM op did not originate from an `ILOp` or that the ASM op is not the first ASM op created by the `ILOp` which generated it. Most IL ops convert to more than one ASM op.
		* **`RequiresILLabel`** - Whether a local label should precede the ASM op. The label is generated using the `ILLabelPosition` value. The purpose of an "IL op label" is so that the ASM op can be the target of a branch instruction.

    * **`ASMBlock`** - This class is used to represent a block of assembly code. It may or may not originate from an `ILBlock` since the *IL Scanner* generates  some ASM Blocks "from thin air" (for example, the String Literals block).
		The `ASMBlock` class also contains a few methods for generating things like IL labels and for adding external and global labels to the block.
		An `ASMBlock` contains the following data:
		* **`PlugPath`** - Path to the ASM file to use as a plug for the ASM block. `null` if the block is not plugged.
		* **`Plugged`** - Whether the block is plugged or not.
		* **`OutputFilePath`** - The path to the '.asm' (and later '.o') file that was produced by saving the converted ASM text for the ASM block.
		* **`ASMOps`** - The list of `ASMOp`s in the block. These are added by the `ILOp.Convert` method or, occasionally, by the `ILScanner` or `ASMPreprocessor`.
		* **`ExternalLabels`** - A list of all the external labels required / used in the `ASMBlock`.
		* **`GlobalLabels`** - A list of all the global labels declared in the `ASMBlock`.

    * **`ASMLibrary`** - This class is used to represent a library being compiled. An `ASMLibrary` originates from an `ILLibrary`. An `ASMLibrary` holds very little data. Primarily it holds the list of `ASMBlock`s for the library. The data it holds is:
         * **`ASMPreprocessed`** - Whether the *ASM Preprocessor* has been executed for the library.
         * **`ASMProcessed`** - Whether the *ASM Processor* has been executed for the library.
         * **`ASMBlocks`** - The `ASMBlock`s that belong to the library.

    * **`ASMPreprocessor`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Discarding empty ASM blocks (or blocks with empty (but not null) plug paths)
        * Adding ASM ops for method labels
        * Adding ASM ops for global labels
        * Adding ASM ops for external labels
        * Adding ASM ops for header/footer of the blocks

		These steps are described in more detail in *"Key points of operation"* and *"Detailed points of operation"*.

    * **`ASMProcessor`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Loading and parsing ASM plug files
        * Converting `ASMBlocks` into ASM text (by calling `ASMOp.Convert`)
        * Cleaning up the output ASM text
        * Saving the output ASM text to '.asm' files
        * Converting the '.asm' files into Object Files ('.o' files) using an external assembly code compiler (such as NASM for x86).

		These steps are described in more detail in *"Key points of operation"* and *"Detailed points of operation"*.

    * **`ASMCompiler`** - This class is static and used to perform processing. Specifically it handles the following tasks:
        * Invoking the *ASM Preprocessor*
        * Invoking the *ASM Processor*

    * **`ASMComment`** - First-level child class of `ASMOp` used to specify the requirements for a Comment op.
    * **`ASMExternalLabel`** - First-level child class of `ASMOp` used to specify the requirements for an External Label op.
    * **`ASMGeneric`** - First-level child class of `ASMOp` used to specify the requirements for any generic op. ***This op is deprecated and is being removed as it breaks the separated-target-architecture model***
    * **`ASMGlobalLabel`** - First-level child class of `ASMOp` used to specify the requirements for a Global Label op.
    * **`ASMLabel`** - First-level child class of `ASMOp` used to specify the requirements for a Label op. *This is unrelated to the External Label and Global Label ops*

* **`Attributes` Namespace** (`Kernel\Drivers\Compiler\Attributes` folder) - Contains attributes used by the compiler and libraries that are compiled by the compiler to indicate special information or processing directives.

* **Tools folder** - The Tools folder contains programs and files used by the compiler which are pre-built or provided by an external source. For example, this includes programs such as NASM and Ld as well as the IsoLinux bootloader files.

### Drivers.Compiler.Architectures.x86_32

* **`Drivers.Compiler.Architectures.x86` Namepsace** (`Kernel\Drivers\Compiler\Architectures\x86_32\ILOps` folder) - Contains the second-level inheritance `ILOp`s which implement the `Convert` and `PerformStackOperations` methods for the x86 32-bit architecture.
* **`Drivers.Compiler.Architectures.x86.ASMOps` Namepsace** (`Kernel\Drivers\Compiler\Architectures\x86_32\ASMOps` folder) - Contains the second-level inheritance `ASMOp`s which implement the `Convert` method for the x86 32-bit architecture.

---

## Key points of operation

The overall intention of the Drivers Compiler is to convert IL code into an ISO or ELF file. ISO files are for kernels, ELF files are for drivers. Both, however, are just formats for storing machine code. So ultimately, machine code is the output of the Drivers Compiler. This section will describe the key steps taken to convert the IL code to machine code. It will skip over some of the technical details which have an impact on the specific output, but not the overall process. For the specific, detailed points and their explanations, see *"Detailed points of operation".

The sections above should have given you a good idea what the classes and structure within the compiler are, along with a lot of the terminology. This section will pull it all together to hopefully give you a good feel for how everything gets converted. The sections are broken down into numbered steps which correspond directly to the stages of the compiler (which were listed as part of Drivers.Compiler.App.CompilerProcess). These steps are:

1. Loading
2. Conversion of IL Ops to ASM Ops
3. Conversion of ASM Ops to ASM text
4. Conversion of ASM text to Object files
5. Conversion of Object files to ISO/ELF

Each of these steps is further broken down in each of the sections below.

### 1. Loading
The loading steps are:

1. Loading the target architecture library
2. Opening the .dll file
3. Loading .dll's of dependencies (if any)
4. Scanning for information (in all libraries that have been loaded)

The loading stage is all about reading in data and interpreting the data but not converting it in any way. The loading stage just builds a wealth of information about the library and code that is going to be compiled. By the end of the loading stage, all the required information for processing the actual IL code will have be loaded.

#### 1. Loading the target architecture library
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.LoadTargetArchiecture. This section also makes describes the use of the TargetILOps and TargetASMOps dictionaries.***

The first step of loading is for the compiler to load the target architecture library. The target architecture library is the thing which contains all the code for converting a specific IL op into a specific set of ASM ops. The target architecture library consists of lots of classes which inherit from classes in the main Drivers.Compiler.IL.ILOps and Drivers.Compiler.ASM namespaces.

To be able to convert an IL op into a set of ASM ops, the compiler needs to know which class in the target architecture library contains the Convert function it needs to call. To help this along (and to improve consistency between target architecture libraries), the base classes found in the Compiler.IL.ILOps namespace have attributes which specify which IL Ops their Convert method's will handle. A similar attributes system is used for ASM ops.

So after reading the ".dll" file for the target architecture library, the loading process scans through all the classes declared in the library. For each class that it finds, it gets a list of the attributes that were applied to the class. It then searches that list of attributes to find out if the class in question was marked as being able to handle IL ops (and if so, which IL ops) or ASM ops (and again, if so, which ASM ops). It stores this information in two dictionaries (one for IL ops, one for ASM ops). The dictionaries correlate op type (by either ASM or IL op code as appropriate) to the class that will handle that op type.

Later on, when the compiler wants to convert an IL op to ASM ops, it can look up the IL op's op-code in the dictionary, get the class which handles that op-code and then call its Convert method. The Convert method is implemented in the target architecture library so will convert the IL op to that architecture's specific ASM ops.

A similar thing happens when the compiler wants to add an ASM op of some type to an ASM block. The compiler can look up the ASM op-code in the dictionary, find out the architecture-specific class for the ASM op-type and then create an instance of that type. By creating an instance of the type it can then add it to the list of ASM ops in the ASM block. This is subtly different from how IL op conversion works. This is because normally the ILOp.Convert methods in the target architecture would create the ASM ops directly and then add them to the ASM ops list. However, here, the compiler is trying to create an architecture specific op to add into the ASM ops list. Thus it must create an instance of the ASM op class to be able to add it to the list.

So that's the target architecture loaded and a bit of extra information about what goes on much later in the compiler. What happens after loading the target architecture library?

#### 2. Opening the .dll file
***The relevant method for this section is Drivers.Compiler.LibraryLoader.LoadILLibraryFromFile.***
Opening the .dll file for the library being compiled (.e.g Kernel.dll or TestDriver.dll) is a fairly trivial step. It simply involves checking the file exists and then loading the .dll file using the System.Assembly class's LoadFrom method which loads an Assembly from a file. If a library by the same name has already been loaded, the LibraryLoader will simply use a copy from the program's cache. This prevents the same library from being processed multiple times later on.

#### 3. Loading .dll's of dependencies (if any)
***The relevant method for this section is Drivers.Compiler.LibraryLoader.LoadDependencies.***
Dependencies are just other libraries which the library in question makes reference to. In Visual Studio they can be seen under the References node of a project's tree in the solution window. To load dependencies the compiler must simply get a list of the dependencies and then work out the paths to their files. It then loads the libraries in the same way as step 2 above and checks whether the new libraries also have dependencies.

The only complication to this step is that the compiler must ignore any dependencies which are part of the .Net Framework. This is done using an exclusion list in the compiler configuration (see the Drivers.Compiler.Options class).

#### 4. Scanning for information
***The relevant method for this section is Drivers.Compiler.Types.TypeScanner and the Info classes in the same namespace.***

You may be wondering what information there is that needs loading. Well, here are the four key types of information (which correlate to the Drivers.Compiler.Types.XXXXInfo classes described in earlier sections).

1. Type information - This is information about all the types (aka classes aka object descriptions) declared in the library being loaded.
2. Field information - This is information about all the fields declared in a given type. Each TypeInfo contains a list of FieldInfos.
3. Method information - This is information about all the methods declared in a given type. Each TypeInfo contains a list of MethodInfos.
4. Variable information - This is information about all the variables declared for a given method. Each MethodInfo contains two lists of VariableInfos. One for arguments and one for locals.

All of this information becomes very important later in the compiler. Here are a few highlights of some of the most important ways the information gets used.

Type information is used to:

* Calculate the size of a type when it is represented on the stack or allocated on the heap (the two sizes may be different. This will be explained later.)
* Determine whether fields and variables of a given type need to be managed by the Garbage Collector.
* Many more things...

Field information is used to:

* Calculate a consistent position (relative to the start of an object in memory) for non-static fields to be stored at.
* Determine if a field is static or not. If it is static, it will need to be statically allocated (as opposed to being allocated on the heap every time a new instance of the type is created).
* Some more things...

Method information is used to:

* Load the IL ops that need to be compiled (this is probably one of the most important things in the compiler!)
* Create labels for use in assembly code. (Labels provide the mechanism for calling the methods.)
* Many more things...

Variable information is used to:

* Calculate the location of the variables and arguments in memory relative to the stack pointer,
* Determine where to temporarily store return values
* Very few more things...

So how is all this information loaded? Well, it's done in stages. Each stage contains a recursive step. Also, the following stages occur for each library separately (with the bottom-most dependencies going first).

1. Scan all types in the library doing a single-depth search to load cursory information about the types (i.e. load what fields there are, but not information about the type of those fields. Also, don't scan base types).
2. Scan all types in the library doing a full-depth search to load detailed information about the types (i.e. look at all the fields and base types and thus determine information about the type in question.)
3. Scan all fields of all types in the library to load information about the fields

Steps 2 and 3 both scan the fields of a type. The difference is that step 2 focuses on determining information about the type (in which the fields were declared). Step 3 focuses on determining information about the fields themselves.

Step 1 (ScanType method) determines basic information about the type. This means compiler will know:

* Some unchanging-global type information such as whether it is a reference type or not. This information is stuff that remains the same regardless of the number of fields or methods.
* All the attributes applied to the type
* The existence of all the fields declared by the type (but no more information than that)
* All the methods (including constructors) declared by the type and the attributes applied to those methods

With this information the compiler can determine whether a type is a reference type or not, whether the type is managed by the garbage collector or not and the name of the type (amongst some other information which isn't used at this stage so is not worth outlining.)

Step 2 (ProcessType method) determines more detailed information about the type. Primarily it aims to determine the size of the type when it is on the stack and the heap. It uses the information loaded in step 1 to allow each type to be processed without the risk of entering an infinite loop. This will be explained after the recursive nature of step 2 is explained. Step 2 goes through all the types in the library. For each type it does the following:

1. If the type is a value type:
	* It can directly, without any further processing / without an recursive processing, determine the size of the type on the heap and stack. This is because value types cannot have base types so will not require another type to be processed before the size of the value type can be determined.
2. If the type is a reference type:
	1. It determines the size of the type on the stack. For reference types this will always just be the size of a pointer (4 bytes for a 32-bit target architecture).
		* It can determine the size on the stack without processing any fields or the base type because when a reference type is on the stack it is just a pointer to the object in memory! It is very important to note that the stack size is unaffected by the number and size of fields because it is key to why the recursive processing works.
	2. It checks if there is a base type that isn't from the .Net Framework (i.e. not from MSCorLib.dll). If there is, then it calls ProcessType on the base type - this is the recursive step!
	3. It adds up the size of all non-static fields within the type. It adds the result to the size of the base type to give the overall size of the type when it is on the heap.
		* For fields which are value types, it calls ProcessType on the types of the fields.
		* Note: It does **not** call ProcessType for the types of fields which are reference types.

By not calling ProcessType for reference type fields, we avoid getting caught in an infinite loop. If we didn't avoid this, for example, we could suppose that we start processing a type. That type has a base type, so we start processing the base type. The base type has a field which has the same type as the type we'd started processing in the first place. The result would be us trying to process the first type all over again and we'd get stuck in an infinite loop. It is safe to call ProcessType for types of fields which are value types because value types cannot have base types so cannot end up in the infinite recursive calls situation.

If steps 1 and 2 were to happen simultaneously, then there would be even more situations where we would end up in infinite recursive loops, some of which are only avoidable by separating out the processing.

So steps 1 and 2 allow us to determine all the information about types. Step 3 (ProcessTypeFields mehtod) then proceeds to use this information to determine all information about fields. Most importantly, it calculates the offset of every field (in both value and reference types). This uses the information about the size of types, which is why it is important that step 2 is complete before we start to process field information. For each type, it loops through the fields. It starts with an offset of zero and sets the first field's offset to that. It then adds the size of the first field to the offset and goes to the next. It sets the next field's offset to the current value, then adds its size to the current offset value. And so on and so forth until it reaches the last field.

By this point, the compiler should have determined everything about all the types, methods, fields and variables - most importantly, the two sizes for each type and the offset of every field.

### 2. Conversion of IL Ops to ASM Ops
The conversion steps (carried out for each loaded library) are:

1. Reading in IL bytes to create IL Ops in an IL block
2. Pre-processing the IL Ops in an IL block to handle special cases
	*Further explanation of this will be skipped as it consists of detailed points.*
3. Generating Static Fields ASM block
4. Generating Types Table ASM block
5. Generating Method Tables ASM block
6. Generating Field Tables ASM block
7. Generating ASM blocks for IL blocks
8. Generating String Literals ASM block

#### 1. Reading in IL bytes to create IL ops in an IL block
***The relevant method for this section is Drivers.Compiler.IL.ILReader.ReadNonPlugged.***

The *IL Library* does not provide us with the IL ops represented in a particularly convenient format for processing. The IL ops are stored as a byte code. This means that for each method there is a byte array. The byte array consists of IL ops and their associated parameters (such as which method to call etc.) in a highly compact format. So for each method in an IL library, the compiler must read the byte code and create instances of the IL op class to represent the IL ops in the method. It adds these IL ops to the IL Block for the method.

Each MethodInfo provides a MethodBody (see properties of the MethodInfo class). The MethodBody has a function called GetILAsByteArray() which allows the compiler to get the IL code as a byte array. The IL Reader then proceeds to read the bytes one by one, interpreting them to construct IL ops and then adding those IL ops to the IL Block. The MethodBody also supplies the information about try-catch-finally blocks defined by the C# code within the method (these are referred to as ExceptionBlocks or ExceptionHandledBlocks within the compiler).

The precise format of the byte code will not be discussed here as it is:

1. Inconsequential,
2. Apparent from the code,
3. Well documented by Microsoft online (see MSDN)

I should point out, however, that each IL op may (or may not) have a Metadata Token associated with it. For many ops, this just supplies a constant value to load (e.g. for ldc ops it will specify the value to load). For some ops, it specifies a reference to something else in the IL Library. The reference can be resolved using the Assembly property (contained in the `ILLibrary` class) and the `ILLibrary` class functions.

It should be pointed out, that for Call, Calli, Callvirt, Ldftn and NewObj ops, the metadata token refers to a method to call. The method to call is pre-loaded from the Metadata Token into the MethodToCall field of the IL op. The MethodToCall field should always be used instead of the Metadata token. This is because later, the IL Preprocessor will inject some call operations which do not use the metadata token - they only set the MethodToCall field. The MethodToCall field will, therefore, always be valid. The Metadata Token will not always be valid.

#### 2. Preprocessing the IL ops in an IL block
***The relevant class for this section is Drivers.Compiler.IL.ILPreprocessor.***
*Further explanation of this will be skipped as it consists of detailed points.*

#### 3. Generating Static Fields ASM block
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.ScanStaticFields.***

The compiler outputs (to the final assembly code) fixed memory allocations for static fields. This saves the driver or kernel from having to allocate them at runtime and allows the static constructors to initialise the static fields directly. Also, for kernels, there would be a logical problem with having static fields allocated once at runtime. That issue is that the Heap implementation (which manages memory allocation) uses static fields. So it would have to succeed in allocating memory for its own fields, before its initialised! Clearly this is impossible.

The Static Fields ASM block, therefore, is used to allocate fixed locations of memory for static fields at compile time so they are available from startup at runtime.

*Further explanation of this will be skipped as it consists of detailed points.*

#### 4. Generating Types Table ASM block
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.ScanType.***

The compiler outputs (to the final assembly code) a table of entries called the Types Table. Each entry contains information about a type in the library. This information is used, for example, when the garbage collector needs to create a new object of a particular type. It loads the type information (which is an entry in the Types Table) then loads the size of the type from the entry and then allocates that amount of memory for the new object.

Each entry in the Types Table has the format defined by the Kernel.FOS_System.Type class (which can be found in Kernel\Libraries\Kernel.FOS_System\Type.cs). The entry consists of the fields from the type class. The fields have the following layout in memory (and thus are allocated in the assembly code in this order). *Sizes and offsets are in bytes*

| Offset | Size | Name | Description |
|:--------:|:-------:|:---------|:---------------|
| 0    | 4   | Id    | Unique identifier number for the type. |
| 4    | 4   | IDString | Unique identifier string for the type. |
| 8    | 4   | Signature | Human-readable, unique signature of the type. |
| 12   | 4   | Size     | Size of an instance of the type when it is allocated on the heap. |
| 16   | 4   | StackSize | Size of an instance of the type when it is represented on the stack. |
| 20   | 1   | IsValueType | Whether the type is a value type or not. |
| 21   | 1   | IsPointer    | Whether the type is a pointer type or not. |
| 22   | 4   | MethodTablePtr | Pointer to the type's method table. |
| 26   | 4   | FieldTablePtr   | Pointer to the type's field table. |
| 30   | 4   | TheBaseType   | Reference to the base type (in fact this is a pointer to the base type's Type Table entry) |
|===========================================================================================|
|      | 34  | Total Size |   |

*Further explanation of this will be skipped as it consists of detailed points.*

#### 5. Generating Method Tables ASM block
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.ScanMethods.***

The compiler outputs (to the final assembly code) a set of tables i.e. multiple tables! These tables are methods tables. There is one methods table per type. A methods table contains information about all the non-static methods defined by a given type. This information is used, for example, when a virtual call is made. The runtime code searches through the method tables from the lowest child-type to the highest base-type until it finds an entry with the same Id value as the one it wants to call. It then loads the pointer to the method from the table entry and then calls it like a normal method.

Each entry in a method table has the following format (i.e. layout in memory). *Sizes and offsets are in bytes*

| Offset | Size | Name | Description |
|:--------:|:-------:|:---------|:---------------|
| 0    | 4   | MethodID | The Id Value (i.e. unique number) which identifies the method. If this is zero, then it is the last entry in the table and the MethodPtr field does not contain a pointer to a method. |
| 4    | 4   | MethodPtr | The pointer to the method. If it is the last entry in the table then: If this field is zero, the pointer is invalid and this is the methods table for highest base-type. Otherwise, the pointer is a pointer to the start of the type's, base-type's methods table. |
|========================|
|      | 8   | Total Size |   |

*Further explanation of this will be skipped as it consists of detailed points.*

#### 6. Generating Field Tables ASM block
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.ScanFields.***

The compiler outputs (to the final assembly code) another set of tables i.e. even more tables! These tables are fields tables. They work in exactly the same way as methods tables except they are used by the garbage collector for identifying fields which are of reference type and so will need cleaning up.

Each entry in a field table has the following format (i.e. layout in memory). *Sizes and offsets are in bytes*

| Offset | Size | Name | Description |
|:--------:|:-------:|:---------|:---------------|
| 0    | 4   | Offset | The offset of the field from the start of an object's memory. |
| 4    | 4   | Size  | The size of the field in an object's memory. If this is zero, then it is the last entry in the table and the FieldType field does not contain a pointer to a type. |
| 8    | 4   | FieldType | A pointer to the type (i.e. an entry in the Types Table) for the type of the field. If it is the last entry in the table then: If this field is zero, the pointer is invalid and this is the fields table for highest base-type. Otherwise, the pointer is a pointer to the start of the type's, base-type's fields table. |
|========================|
|      | 12   | Total Size |   |

*Further explanation of this will be skipped as it consists of detailed points.*

#### 7. Generating ASM blocks for IL blocks
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.ScanNonpluggedILBlock.***

The actual conversion of IL to ASM (from the compiler's perspective not the target architecture perspective) is very simple. The following simple steps are performed for each IL block:

1. Create a new ASM block
2. Create an ILConversionState object (which the target architecture's IL op implementation use to keep track of state between converting IL ops)
3. For each IL op in the IL block:
	1. Add an ASM Comment op to mark the ASM output with information about the IL op
	2. Check whether the op is a custom IL op (i.e. one added by the Drivers Compiler and not specified by Microsoft):
		* If it is, call the Convert method for the custom op (passing in the conversion state and the current IL Op's information)
	3. Otherwise:
		1. Load the op-code for the current IL op from the current IL op's information
		2. Load the target architecture's IL op implementation from the TargetILOp's dictionary (described earlier in the Loading section).
		3. Call the target architecture IL op's Convert method (passing in the conversion state and the current IL Op's information)
	4. If the current IL op's information indicates the first ASM op produced for the IL should be marked with an IL label:
		* *(The IL op's information is changed during the Convert method call to indicate whether the IL Label is required or not()*
		* Set the ILLabelPosition and RequiresILLabel fields of first ASM op produced by calling the Convert method.
4. Add the ASM block to the ASM Library for the *IL Library* of the method being compiled.

#### 8. Generating String Literals ASM block
***The relevant method for this section is Drivers.Compiler.IL.ILScanner.Scan.***

The code contains string literals (the stuff in double quotes e.g. "Hello, world!"). Each string literal must be stored in the output assembly code for use at runtime. The Drivers Compiler does this by outputting a block of assembly code which contains allocations of memory for string objects which have pre-initialised values. I.e. the character array for the string is pre-set to the correct values for each string literal. A string literal in the ASM code has the same format as a String object (see FOS_System.String in Kernel\Libraries\Kernel.FOS_System\String.cs). This format is:

| Offset | Size | Name | Description |
|:--------:|:-------:|:---------|:---------------|
| 0    | 4   | _Type | *Field inherited from Object from ObjectWithType*. Always a reference to the String type (i.e. the String type's Types Table entry) |
| 4    | 4   | length | The length of the string (in characters).
| 8    | ???  | --- | The remaining memory is a char array (where a char is two bytes). The length of the char array is equal to the value of the *length* field. |
|==========================|
|      | (*length* * 2) + 8 | Total Size |   |

*Further explanation of this will be skipped as it consists of detailed points.*

### 3. Conversion of ASM Ops to ASM text
The conversion steps (carried out for each loaded library) are:

1. Pre-processing the ASM ops in an ASM block to handle consistent or special cases
	*While this contains detailed points, there are few of them and they are fairly simple, so this will be covered further in this section.*
2. Generating ASM text (code) for ASM blocks

#### 1. Pre-processing the ASM ops in an ASM block
***The relevant methods for this section are the Drivers.Compiler.ASM.ASMPreprocessor.Preprocess methods (both versions of the overloaded function).***

The ASM Compiler calls the ASM Preprocessor's public Preprocess method, passing it an ASMLibrary to preprocess. The ASM Preprocessor takes the following steps for each ASM Block in the ASM Library:

1. If the block is plugged:
	1. If the plug path is empty or just white space (which is not the same as being null), it ignores the block's contents and plug specification and removes it from the ASM Library's list of ASM blocks.
	2. Otherwise, it leaves the ASM block as-is
2. Otherwise, the ASM block is not plugged, so it:
	1. Adds a Label op in front of the first ASM op that is the method label. The method label is the label called when a method call occurs. The method label is a substitute for the address of the start of the method. (*Note: Labels are always substitutes for addresses and are used like pointers).
	2. Adds an ASM Global Label op in front of the method label op to make the method label accessible outside of the output .asm file (which will contain only the single ASM block's code).
	3. Adds ASM External Label ops for all distinct, required external labels (in front of the global label). Required external labels are registered during calls to ILOp.Convert method in the ILScanner stage.
	4. Add an ASM Header op in front of the external labels to add directives to the assembler compiler (e.g. the "BITS 32" directive for NASM when the target architecture is x86).

#### 2. Generating ASM text (code) for ASM blocks
***The relevant method for this section is the Drivers.Compiler.ASM.ASMPreprocessor.ProcessBlock method.***

At this stage, generating the ASM text for an ASM block is fairly simple. The following steps are followed for each ASM block in the ASM Library being compiled:

1. If the ASM block is plugged:
	1. Generate the plug file's full file name from the plug path and target architecture name.
	2. Read the plug file's contents and use it as the block's ASM text
	3. Replace any macros in the ASM text with their actual values
2. Otherwise, if the ASM block is not plugged:
	1. For each ASM op in the ASM block, call the op's Convert method and append the resulting text to the ASM block's Text.
3. In both cases:
	1. Clean up the ASM text (by forcing capitalisation of extern and global statements)
	2. Save the ASM text to the .asm output file.

### 4. Conversion of ASM text to Object files
The conversion steps (carried out for each loaded library) are:

1. Converting the .asm files to Object files (.o) using an external ASM compiler (e.g. NASM for x86 ASM code)

Basically this just executes NASM or a similar tool for every ASM block in an ASM Library. It passes the full file path of the output .asm file to the external compiler tool.

Output ASM File Path: "bin\Debug\DriversCompiler\ASM\*.asm"
Output Object File Path: "bin\Debug\DriversCompiler\Objects\*.o"

### 5. Conversion of Object files to ISO/ELF

1. Ordering ASM blocks (by this stage they are Object files)
2. Using a linker (e.g. ld) to link the output into a binary file (either .elf or .bin)
3. If compiling to ISO, using the ISO9660Generator to create the .ISO file from the .bin file.

---

## Detailed points of operation
