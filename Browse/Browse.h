#pragma once
//////////////////////////////////////////////////////////
// Browse.h - Display the information using Process		//
// ver 1.0												//
// Author - Sharath Nagendra							//
// CSE 687 - Object Oriented Design, Spring 2018		//
// Source - Jim Fawcett, Syracuse University			//
//////////////////////////////////////////////////////////

/*
Operations Handeled:
---------------------
- Provide functionality of querying on database
- Provide functionality to display the content of a file

Required files for the operation:
---------------------------------
- Definitions.h
- Process.h, Process.cpp
- FileSystem.h, FileSystem.cpp
- Query.h, Query.cpp
- Browse.cpp

Maintainance History:
---------------------
Ver 1.0 - March 14 2018 - first relase
*/


#include "../DbCore/Definitions.h"
#include "../Process/Process.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Query/Query.h"
using namespace FileSystem;



namespace NoSqlDb 
{
	template <typename T>
	class Browse
	{
	public:
		using RegularExp = const std::string&;
		Browse(DbCore<T>& db):dbcore(db) {}
		void displayFile(Namespace_& namespace_,Filename_& filename_,const VersionNo& version);
		void displayDir();
		static void identify(std::ostream& out = std::cout);
		void getFiles(RegularExp regExp);
		void getCatFiles(RegularExp regExp);


	private:
		Keys keys_;
		Directory direct;
		DbCore<T>& dbcore;
	};

	//Following function uses the package to display the file on a notepad file	

	template<typename T>
	inline void Browse<T>::displayFile(Namespace_ & namespace_, Filename_ & filename_,const VersionNo & version)
	{
		
		Process p;
		p.title("Browsing File");
		std::string applicationPath = "C:/Windows/System32/notepad.exe";
		p.application(applicationPath);
		std::string cmdLine = "/A ../Storage/"+namespace_+"/"+filename_+"."+std::to_string(version);
		p.commandLine(cmdLine);
		std::cout << "\n  starting process: \"" << applicationPath << "\"";
		std::cout << "\n  with this cmdlne: \"" << cmdLine << "\"";
		p.create();
		CBP callback = []() { std::cout << "\n  --- child process exited with this message ---"; };
		p.setCallBackProcessing(callback);
		p.registerCallback();
	}

	//Function to get all the directories in the repository 

	template<typename T>
	inline void Browse<T>::displayDir()
	{
		direct.getDirectories();
	}
	//Function to display all the filename

	template<typename T>
	inline void Browse<T>::identify(std::ostream & out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//Function to query on keys

	template<typename T>
	inline void Browse<T>::getFiles(RegularExp regExp)
	{
		DbCore<T> db_;
		Keys veckeys;
		for (auto key:dbcore.keys())
		{
			std::regex regex(regExp);
			if (std::regex_search(key, regex))
			{
				db_[key]= dbcore[key];
			}
		}
		showDb(db_);
	}

	//Function to query on the category

	template<typename T>
	inline void Browse<T>::getCatFiles(RegularExp regExp)
	{
		DbCore<T> db_;
		Keys veckeys;
		DbElement<PayLoad> elem;
		for (auto key : dbcore.keys())
		{
			elem = dbcore[key];
			for (auto value : elem.payLoad().categories())
			{
				std::regex regex(regExp);
				if (std::regex_search(value, regex))
				{
					db_[key] = dbcore[key];
				}
			}
		}
		showDb(db_);
	}


}