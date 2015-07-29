---
layout: reference-article
title: Drivers Compiler
date: 2015-07-29 11:32:00
categories: docs reference
---

# Drivers Compiler
## IL to Machine Code

The Drivers Compiler's sole purpose is to take IL code and to convert it into machine code. It does so via the following steps (the sequence of conversions are shown in increasing levels of detail).

```
IL                          ->                         Machine Code
IL              ->          ASM         ->             Machine Code
IL   ->   Information / Structure   ->   ASM    ->     Machine Code
IL  ->  Info  ->  IL Blocks  ->  ASM Blocks  ->  Object Files  ->  ISO/ELF
IL  ->  Info  ->  IL Blocks (/IL Ops)  ->  ASM Blocks (/ASM Ops)  ->  Object Files  -> ISO/ELF
```

---

## Description of key classes / structure

*Note: The namepsace / class structure of the compiler matches the folder structure. So if something is called Drivers.Compiler.IL.ILOp, the class is called "ILOp" and will be found in a file called "ILOp.cs" under the folder:
"Kernel\Drivers\Compiler\IL". It will also be found (in Visual Studio) under the Drivers folder in the Drivers.Compiler project. The project names match their namespaces.*

*Note: It is helpful to think of "types" as "descriptions of objects" and "objects" as "allocations of heap memory which conform to a given description".*

*Note: There are two kinds of class in the Drivers Compiler. There are classes for doing things and classes for storing data / information (sometimes their name's end in Info). Classes for doing things are usually static and contain static methods (often called "ExecuteXYZ"). Classes for storing data usually contain no methods and have the data given to them.*

### Drivers.Compiler.App
* **CompilerProcess** - The main entrypoint for the compiler when it is being used as a command-line application.

    This class handles calling the steps of the compiler as described in the sequence of conversions above. The exact list is:
    1. (Validate command line options)
    2. Load the IL Library (includes the Information/Structure conversion step)
    3. Call the IL Compiler (handles IL Block to ASM Block conversion)
    4. Call the ASM Compiler (handles ASM Block to ASM file to Object file conversion)
    5. Call the Link Manager (handles Object File to ISO/ELF conversion)

### Drivers.Compiler.MSBuild
* **BuildTask** - The main entrypoint for the compiler when it is being used as a build task for MSBuild.

    The Build Task is started by MSBuild immediately after MSBuild finishes converting C# into IL and after it has created the .dll file (which is the IL library).

    The steps taken by the BuildTask are identical to those of the CompilerProcess in Drivers.Compiler.App.

### Drivers.Compiler

* **Types Namespace** (Types folder) - This contains all the classes (and most of the methods) related to converting a .dll file into information objects within the compiler.

    Information objects include FieldInfo, MethodInfo, TypeInfo and VariableInfo. Information objects are used by the Drivers Compiler to keep track of information about everything the compiler has to convert. They are discussed in more detail below. There is one additional class called TypeScanner in this namespace.

    * **TypeScanner** - This is the static class which has all the methods for reading an IL Library (a .dll file) and loading all the information. It loads the information, creates a new information object and then populates that new object with the information.

    The TypeScanner is also capable of determining some information from what it loads. The three things it determines relate to information about types. It determines: The size of an instance of a given type if it were referred to on the stack, the size of an instance of a type when it is allocated in memory (i.e. how much memory a call to memalloc would need to request to have enough to match the given type) and lastly, it determines whether the type is managed by the Garbage Collector or not.

    In general, value types have the same size on the stack and the heap. Value types are not managed by the garbage collector. In contrast, reference types are 4 bytes on the stack (size of a pointer/address in 32-bit architecture) and their normal size on the heap. All reference types are managed by the garbage collector. Pointer types are treated as integer value types and so follow all the same processing steps as value types.

    * **FieldInfo** - Instances of FieldInfo are created by the Drivers Compiler to keep track of information about the fields of a type. Note that fields are something like:

    ```
    class X
    {
        int AField;
    }
    ```

    Which are distinct from properties. Properties are not fields. Properties use get/set and look something like:

    ```
    class Y
    {
        int AProperty
        {
            get;
            set;
        }
    }
    ```

    Field Info keeps track of some very simple data:
        * **FieldType** - The type of the field. In the example above, the type of AField is "int".
        * **IsStatic** - Whether the field is static or not. Static fields are treated differently to normal (/instance) fields. Static fields are pre-initialised and have separate memory allocated by the compiler as part of the final machine code. Instance fields, on the other hand, contribute to a type's size when it is on the heap. Each separate instance of a type (i.e. each separate memory allocation) has its own values for instance fields.
        * **OffsetInBytes** - This only applies to instance fields (i.e. non-static fields). This is the offset, from the start of the object when it is in heap memory, to the start of the field. It is equivalent to the sum of all the sizes of the fields which precede it in memory.
        * **ID** - This is a string which uniquely identifies the field or static field. It can be used as a label in the output ASM code.
        * **Name** - This is a string which is a human readable name for the field. It is the name given in the C# code.

    * **MethodInfo** - Instances of MethodInfo are created by the Drivers Compiler to keep track of information about the methods of a type. This includes all static, non-static, private, public and abstract or virtual methods.

    Method info keeps track of the following data:
        * **PlugAttribute** - If a PlugMethodAttribute was applied to the method, then this field contains the information about that attribute. Otherwise, this field is null.
        * **ArgumentInfos** - A list of VariableInfos which describe all the arguments for the function (including the hidden "this" argument).
        * **LocalInfos** - A list of VariableInfos which describe all the local variables used within the method. Includes hidden or extra locals that aren't declared by the C# but are created by MSBuild.
        * **ApplyGC** - Whether to apply the garbage collector inside the method or not. use of the garbage collector causes the compiler to insert extra IL operations. If ApplyGC is false, the compiler will not add the extra GC-related IL operations. This will be set to false if the NoGC attribute is applied to the method.
        * **ApplyDebug** - Whether to apply debug operations inside the method or not. In debug build mode, the compiler will inject extra Il and ASM operations used for debugging at runtime. If ApplyDebug is false, the compiler will not add the extra debug-related IL and ASM operations. This will be set to false if the NoDebug attribute is applied to the method.
        * **IsPlugged** - Whether the method is plugged or not.
        * **IsConstructor** - Whether the method is a constructor or not.
        * **IsStatic** - Whether the method is static or not.
        * **MethodBody** - The method body of a method contains the bytes which represent the method's IL code.
        * **ID** - A unique string identifier for the method. This is also used as a label for the method in the ASM code.
        * **Signature** - A unique but human-readable string for the method. Cannot be used as a label but is used to generate the ID.
        * **IDValue** - A unique integer identifier for the method. This is used in the method tables.
        * **Priority** - The priority of the method. Zero by default. Set using the priority attribute. Used for ordering the output assembly code / machine code during the linker stage.
        * **Preprocessed** - Whether the method has undergone preprocessing or not. This is used to prevent the method being preprocessed multiple times!

    * **TypeInfo** - Instances of TypeInfo are created by the Drivers Compiler to keep track of information about all the types that are in the library(/libraries) being compiled.

    Type info keeps track of the following data:
        * **FieldInfos** - A list of all the fields declared in the type.
        * **MethodInfos** - A list of all the methods declared in the type.
        * **Processed** - Whether the type's data (excludes methods and fields) has been processor or not.
        * **ProcessedFields** - Whether the type's fields have been processed or not.
        * **IsValueType** - Whether the type is a value type or not.
        * **IsPointer** - Whether the type is a pointer type or not.
        * **SizeOnStackInBytes** - The size of the type when it is represented on the stack. See description in *TypeScanner*.
        * **SizeOnHeapInBytes** - The size of the type when it is represented on the heap.  See description in *TypeScanner*.
        * **IsGCManaged** - Whether the type is managed by the garbage collector. true for reference types, false for value types. Occasionally this value is altered from that rule.
        * **ID** - A unique string identifier for the type. This is also used as a label for the type in the ASM code.

    * **VariableInfo** - Instances of VariableInfo are created by the Drivers Compiler to keep track of information about variables (locals or arguments) that exist within/for a method.

    Variable info keeps track of the following data:
        * **TheTypeInfo** - Information about the type of the variable.
        * **Position** - The position of the variable as an index. It is the index in the list which contains the variable info.
        * **Offset** - The offset in bytes from the Base Pointer (under x86, this is EBP).

* **IL Namespace** (IL folder) - This contains all of the classes (and most of the methods) related to taking IL operations and converting them into ASM operations. However, it does not (or should not) include any code which produces architecture-specific assembly operations. Any code related specifically to converting a specific IL op into ASM ops for a particular architecture goes in a target architecture library such as the Drivers.Compiler.Architectures.x86_32 library.

    For information about the IL.ILOps namespace (Drivers\Compiler\IL\ILOps folder), see the next section.

    * **ILOp** - This class has dual purposes. These purposes are independent and so are described individually below.

    The first purpose of the ILOp class is as a data class. It stores data about IL operations. IL operations are like a high-level assembly code. They are stored as bytes in a byte array in a method. See ILBlock for details.

    The second purpose of the ILOp class is as a base class for IL op converter implementations. There are a significant number of different IL ops and each of them inherits from the base ILOp class. The ILOp class declares an abstract Convert method which second-level child classes must implement. The child classes of ILOp go two levels deep. The first level is contained within the Drivers Compiler (in the IL.ILOps namespace), the second level is contained in a target architecture library.

    The first level of inheritance from the ILOp class is in the IL.ILOps namespace. There are classes for every basic form of IL Op that the compiler supports. The child classes have attributes which detail which variants of the basic IL ops the classes support. These first-level classes do not  implement the Convert method since they are in the Drivers Compiler, and the Drivers Compiler does not directly handle architecture specific conversion.

    The second level of child classes from the ILOp class is in a target architecture library. These classes inherit from the first-level classes. The second-level classes implement the Convert method and handle the actual conversion of the IL Op into ASM Ops. The ASM Ops produced are also found in the target architecture library.

    The IL Op class also declares an abstract PerformStackOperations method which is used by the ILPreprocessor prior to the ILScanner calling the Convert method. The Drivers Compiler keeps track of descriptions of what is on the stack during processing of a method. The PerformStackOperations method performs all the same stack operations as the Convert method but without outputting any actual assembly code.

    * **ILBlock** - This class is used to represent a block of IL code. A "block of IL code" is exactly equivalent to a method in C#. The compiler uses MethodInfo objects to store information about the methods being compiled but does not store the list of IL ops for the method. This is where the IL Block comes in. The IL Block stores a list of all the IL Ops for the method and a reference to the MethodInfo for which the IL Block was generated. An IL Block also keeps track of some additional data related to converting the block from IL to ASM.

    An IL Block contains the following data:
        * **PlugPath** - Path to the ASM file to use as a plug for the IL block. Null if the block is not plugged.
        * **IsPlugged** - Whether the block is plugged or not.
        * **ILOps** - The list of IL Ops in the block which make up the method. These are loaded by the IL Reader from the byte array in *MethodInfo.MethodBody*. The IL Ops list is subsequently modified by the ILPreprocessor to add (and occasionally remove) some IL operations. This happens prior to the ILScanner converting the IL Ops into ASM ops.
        * **ExceptionHandledBlocks** - The list of try-catch-finally blocks for the method.

    * **ILLibrary** - This class is used to represent a library being compiled. An IL Library orignates from a .dll file. An IL Library primarily holds information about the types and all methods within the library. It also holds information about library dependencies i.e. libraries which are referred to be the library in question.

    An IL Library contains the following data:
        * **TheAssembly** - The Assembly object which represents the (.dll) file from which the IL Library is loaded.
        * **TheASMLibrary** - The ASM Library to which the IL Library will be or has been compiled.
        * **Dependencies** - All direct dependencies of the library.
        * **TypeInfos** - List of all the information about all the types in the library.
        * **ILBlocks** - A dictionary mapping all the information about methods (from all types) in the library to their respective IL Blocks.
        * **SpecialClasses** - This is a dictionary which maps compiler-specific attributes (which flag special classes such as the replacement Object and String classes) to the TypeInfo(s) of the classes to which the attributes were applied. Special classes are used by the compiler to replace .Net Framework classes.
        * **SpecialMethods** - This is a dictionary which maps compiler-specific attributes (which flag special methods such as the replacement Throw Exception method) to the MethodInfo(s) of the methods to which the attributes were applied. Special methods are used by the compiler to replace .Net Framework methods or to complete compiler-required operations.
        * **ILRead** - *(past tense)* Indicates whether the IL Reader has been executed for the library or not.
        * **ILPreprocessed** - *(past tense)* Indicates whether the IL Preprocessor has been executed for the library or not.
        * **ILScanned** - *(past tense)* Indicates whether the IL Scanner has been executed for the library or not.
        * **TheStaticConstructorDependencyTree** - Tree describing the dependencies between static constructors. This is described in more detail in the *"Detailed points of operation"* section.
        * **StringLiterals** - A dictionary mapping IDs to values for all the string literals declared within the library.

    The IL Library class also contains a number of methods for accessing the contained data which take into account data from dependencies.

    * **ILPreprocessor** - This class is static and used to perform actions. Specifically it handles the following tasks:
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

    * **ILScanner** - This class is static and used to perform actions. Specifically it handles the following tasks:
        * Loading the target architecture library
        * Generating ASM Blocks for the following:
            * Static fields
            * Type Tables
            * Method Tables
            * Field Tables
            * Plugged IL Blocks
            * Non-plugged IL Blocks

    These steps are described in more detail in *"Key points of operation"* and *"Detailed points of operation"*.

    * **ILCompiler** - This class is static and used to perform actions. Specifically it handles the following tasks:
        * Invoking the IL Reader
        * Invoking the IL Preprocessor
        * Invoking the IL Scanner

     This class is little more than a wrapper to ensure the respective IL processing steps are called in the correct order.

    * **ILConversionState** - This class represents the state of a method (such as items on the stack) while it is being converted. It is passed by the IL Scanner to theConvert method of the second-level inheritance IL Ops of the target architecture library. The target architecture library's IL ops can then use it to keep track of necessary state information that must be passed between Convert calls.

    * **ILPreprocessState** - A stripped-down version of the ILConversionState state used for the *ILOp.PerformStackOperations* method which is called by the ILPreprocessor.

    * **ExceptionBlock** - Used for storing information about try-catch-finally blocks. The exact data stored in this is discussed in more detail in the "Detailed points of operation" section.

    * **StaticConstructorDependency** - Describe a dependency between static constructors. This is discussed in more detail in the *"Detailed points of operation"* section.

* **IL.ILOps Namespace** (IL/ILOps folder) -
* **ASM Namespace** (ASM folder and ASM/ASMOps folder) -
* **Attributes Namespace** (Attributes folder) -
* **Tools folder** -

### Drivers.Compiler.Architectures.x86_32

* **Drivers.Compiler.Architectures.x86 Namepsace** (ILOps folder) -
* **Drivers.Compiler.Architectures.x86.ASMOps Namepsace** (ASMOps folder) -

---

## Key points of operation

---

## Detailed points of operation
