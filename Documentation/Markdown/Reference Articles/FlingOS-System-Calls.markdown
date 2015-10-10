---
layout: reference-article
title: FlingOS System Calls
date: 2015-09-20 21:04:00
categories: [ docs, reference ]
parent_name: FlingOS™
description: FlingOS system specific details of its system calls.
---

# Note

The documentation on this page is prelimenary. It is still in the design stage and subject to significant changes. Most of it has also yet to be implemented in the FlingOS system itself.

# System Calls list

| Num | Name | Description |
|:--------:|:----------|:----------------|
| 0  | INVALID | Not a valid system call. |
| 1  | Register Interrupt Handler | Registers a handler method for a given interrupt number (excluding the syscall interrupt number). |
| 2  | Deregister Interrupt Handler | Performs the inverse of Register Interrupt Handler |
| 3  | Register Syscall Handler | Registers a handler method for a specific system call number. |
| 4  | Deregister Syscall Handler | Performs the inverse of Register Syscall Handler. |
| 5  | Request Page | Requests a new free page or specific page of memory be mapped into the current process's address space. |
| 6  | Unmap Page | Removes a page of memory from the current process's address space. |
| 7  | Share Page | Shares a page of the current process's memory with another process. Both sides must agree/request the share. |
| 8  | Start Process | Starts a new process. |
| 9  | End Process | End the specified process. |
| 10 | Set Process Attributes | Sets the attributes of the specified process. |
| 11 | Get Process List | Gets a list of all currently executing processes. |
| 12 | Wait On Process | Waits for the specified process to end (meaning all threads end). |
| 13 | Start Thread | Starts a new thread within the current process. |
| 14 | End Thread | Ends the specified thread. |
| 15 | Set Thread Attributes | Sets the attributes of the specified thread. |
| 16 | Get Thread List | Gets a list of all the threads for ths specified process. |
| 17 | Wait On Thread | Waits for the specified thread to end. |
| 18 | Create Semaphore | Creates a new semaphore and returns its handle. |
| 19 | Release Semaphore | Releases an existing semaphore. When no more references are held, the semaphore is destroyed. |
| 20 | Wait Semaphore | Waits on a semaphore. |
| 21 | Signal Semaphore | Signals a semaphore. |
| 22 | Get Time | Gets the current system time. |
| 23 | Set Time | Sets the current system time. |
| 24 | Get Up Time | Gets the system up time in milliseconds. |
| 25 | Register Pipe Endpoint | Registers a pipe endpoint (output) for the current process. |
| 26 | Get Pipe Endpoints | Gets a list of all pipe endpoints. |
| 27 | Create Pipe | Creates a pipe between the current process and the specified endpoint. |
| 28 | Wait on Pipe Create | Waits for a pipe to be created to the specified endpoint of the current process. |
| 29 | Read Pipe | Reads data from a pipe. |
| 30 | Write Pipe | Writes data to a pipe. |
| 31 | Register Device | Registers the existence of a new device. |
| 32 | Deregister Device | Deregisters the existence of a specified device. It must be assigned to the process which attempts to deregister it. |
| 33 | Assign Device | Assigns a known device to be managed by a specific driver. |
| 34 | Release Device | Releases a specified device so that it can be assigned to another driver. |
| 35 | Sleep | Sleeps the current thread for the specified number of milliseconds. "-1" results in an indefinite sleep until an external event wakes the thread. |