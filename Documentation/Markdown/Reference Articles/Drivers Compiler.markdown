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

* **IL Namespace** (IL folder) -
* **IL.ILOps Namespace** (IL/ILOps folder) -
* **ASM Namespace** (ASM folder and ASM/ASMOps folder) -
* **Attributes Namespace** (Attributes folder) -
* **Tools folder** -

### Drivers.Compiler.Architectures.x86_32

* **Drivers.Compiler.Architectures.x86 Namepsace** (ILOps folder) -
* **Drivers.Compiler.Architectures.x86.ASMOps Namepsace** (ASMOps folder) -

---

## Description of key points of operation

---

## Description of detailed points of operation
