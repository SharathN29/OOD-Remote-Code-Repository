# OOD-Remote-Code-Repository

## Purpose
A Code Repository is a Program responsible for managing source code resources, e.g., files and documents. A fully developed Repository will support file persistance, managment of versions, and the acquisition and publication of source and document files. A Remote Repository adds the capability to access the Repository's functionality over a communication channel, e.g., interprocess communication, inter-network communication, and communication across the internet.
In this project we will develop a Remote Code Repository, local Client, and communication channel that supports client access to the Repository from any internet enabled processor.
The communication channel will use sockets and support an HTTP like message structure. 
### The channel could support:
* HTTP style request/response transactions
* One-way communication, allowing asynchronous messaging between any two endpoints that are capable of listening for connection requests and connecting to a remote listener.
* Transmission of byte streams that are set up with one or more negotiation messages followed by transmission of a stream of bytes of specified stream size2.

One way asynchronous communication is required, and file transfer using messages with binary bodies has already been implemented and that is sufficient to meet all requirements related to communication, below.

### The Remote Code Repository will:
* Support check-in, check-out, versioning, and browsing.
* Transfer source code text files to and from a local Client.
* Support defining dependency relationships between code packages saved in specific repository folders, based on the functionality you provided in Project #2.
* Support storing package metadata in the NoSql database.

### Clients of the Remote Code Repository will provide a Graphical User Interface (GUI) with means to:
* Upload one or more source code text files to the Remote Repository, specifying one or more categories with which those files are associated1.
* Display package categories, based on metadata stored in the NoSql database.
* Display collections of filenames based on queries defining categories, filenames, dependencies, and versions.
* Display all of the files in any category that have no parents.

## What has been developed 
* Shall use Visual Studio 2017 and the standard C++ libraries, as provided in the ECS computer labs. You may also use C#, the .Net Windows Presentation Foundation framework, and C++\CLI for the graphical part of each Client.
* Shall provide a Repository Server that provides functionality to check-in, check-out, and browse packages, specified by NoSql database queries.
* Shall provide a Client program that can upload and download files3, and view Repository contents, as described in the Purpose section, above.
* Shall provide a message-passing communication system, based on Sockets, used to access the Repository's functionality from another process or machine.
* The communication system shall provide support for passing HTTP style messages using asynchronous one-way messaging.
* The communication system shall also support sending and receiving blocks of bytes6 to support file transfer.
* Shall include an automated unit test suite that demonstrates you meet all the requirements of this project4 including the transmission of files.
