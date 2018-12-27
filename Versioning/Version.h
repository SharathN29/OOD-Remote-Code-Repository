#pragma once
/////////////////////////////////////////////////////////////////////
// Verson.h - Handles version assignments                          //
// ver 1.0                                                         //
//Author- Sharath Nagendra			                               //                                        //
//CSE687 - Object Oriented Design, Spring 2018                     //
/////////////////////////////////////////////////////////////////////
/*
* File Operations:
* -------------------
* Handles versioning of file
* getting latest version
* check if version exist

* Required Files:
* ---------------
* Defination.h
* DbCore.h,DbCore.cpp
* Version.cpp

* Maintenance History:
* --------------------
* Ver 1.0 : 14 Mar 2018 - first release
*/

#include <string>
#include <regex>
#include "../DbCore/Definitions.h"
#include "../DbCore/DbCore.h"

namespace NoSqlDb 
{
	template<typename T>
	class Version
	{
	public:
		int getlatestVersion(Namespace_ namespace_, Filename_ filename,DbCore<T>&);
		int appendVersion(Namespace_ namespace_, Filename_ filename, DbCore<T>&);
		bool checkVersion(Namespace_ namespace_, Filename_ filename, DbCore<T>&);
		static void identify(std::ostream& out = std::cout);
	private:
		int assignVersion(Filename_& filename);
	};


	//Function to get latest version of a file from Repository

	template<typename T>
	inline int Version<T>::getlatestVersion(Namespace_ namespace_, Filename_ filename, DbCore<T>& dbCore)
	{
		Keys keyVector;
		keyVector = dbCore.keys();
		std::regex regex(namespace_ + "::" + filename);
		int maxVersion = 0;
		std::smatch str;
		for (auto it : keyVector)
		{
			int max = 0;
			if (std::regex_search(it, regex))
			{
				max = assignVersion(it);
				if (max>maxVersion)
				{
					maxVersion = max;
				}
			}
		}
		return maxVersion;
	}

	//Function to append the version of a file

	template<typename T>
	inline int Version<T>::appendVersion(Namespace_ namespace_, Filename_ filename, DbCore<T>& dbCore)
	{
		DbElement<T> elem;
		Keys keyVector;
		keyVector = dbCore.keys();
		std::regex regex(namespace_ + "::" + filename);
		int maxVersion=0;
		std::smatch str;
		for(auto it:keyVector)
		{
			int max=0;
			if (std::regex_search(it, regex))
			{
				max = assignVersion(it);
				if (max>maxVersion)
				{
					maxVersion = max;
				}
			}
		} 
		Filename_ name = namespace_ + "::" + filename + "." + std::to_string(maxVersion);
		elem = dbCore[name];
		if (elem.CheckinStat()==elem.Open)
		{
			return maxVersion;
		}
		else
		{
			return maxVersion + 1;
		}
		return 0;
	}

	//Function to check whether a particular version of a file exists

	template<typename T>
	inline bool Version<T>::checkVersion(Namespace_ namespace_, Filename_ filename, DbCore<T>& dbCore)
	{
		Keys keyVector;
		keyVector = dbCore.keys();
		std::regex regex(namespace_ + "::" + filename);
		std::smatch str;
		for (auto it : keyVector)
		{
			int max = 0;
			if (std::regex_search(it, regex))
			{
				return true;
			}
		}
		return false;
	}

	//Function to display the filename

	template<typename T>
	void Version<T>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//Function to assign a version to a particular file

	template<typename T>
	inline int Version<T>::assignVersion(Filename_ & filename)
	{
		std::string rawname;
		rawname = filename.substr(filename.find_last_not_of("."), filename.length());
		return std::stoi(rawname);
	}

	


}