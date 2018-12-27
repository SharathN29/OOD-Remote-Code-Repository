#pragma once
///////////////////////////////////////////////////////////////
// CheckOut.h - Copies the checked-out file to a location	 //
// ver 1.0													 //
// Author - Sharath Nagendra								 //
// CSE 687 - Object Oriented Design, Spring 2018			 //
///////////////////////////////////////////////////////////////

/*
Operations Handled:
-------------------
- Required file will be copied from CoreRepository to the user when asked

Required Files:
-----------------
- Version.h, Version.cpp
- FileSystem.h, FileSystem.cpp
- Definition.h
- CheckOut.cpp

Maintainance History:
---------------------
ver 1.0 - March 14 2018 - first release
*/

#include "../Versioning/Version.h"
#include "../DbCore/Definitions.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"

using namespace FileSystem;

namespace NoSqlDb 
{
	class CheckOut
	{
	public:
		bool checkoutFile(Path_ srcpath_,Namespace_ namespace_);
		bool file_copy(Path_ & srcpath, std::string finalname);
		static void identify(std::ostream& out = std::cout);
		CheckOut() 
		{
			if (!FileSystem::Directory::exists("../ServerPrototype/SendFiles"))
			{
				FileSystem::Directory::create("../ServerPrototype/SendFiles");

			}
		}
	private:
	};


}