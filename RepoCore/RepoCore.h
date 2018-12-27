#pragma once
//////////////////////////////////////////////////////////////////
// CoreRepository.h - Handles all the repository functionality  //
// ver 1.0														//
// Author - Sharath Nagendra									//
// Source - Jim Fawcett, Syracuse University					//
// CSE 687 - Object Oriented Design, Spring 2018				//
//////////////////////////////////////////////////////////////////

/*
Operations Handled:
-----------------------
This is the core repository for this database, and it handles the
following functionalities
- Provides functionalities for file checkin.
- Provides functionalities for file checkout
- Provides functionalities for browsing a file
- Provides functionalities for querying the database for a particular file

Required files for the operation:
-----------------------------------
- CheckIn.h, CheckIn.cpp
- CheckOut.h, CheckOut.cpp
- PayLoad.h, PayLoad.cpp
- Browse.h, Browse.cpp
- CoreRepository.cpp

Maintainance History:
------------------------
Ver 1.0 - March 14 2018 - First Release
*/


#include "../Checkin/Checkin.h"
#include "../Checkout/Checkout.h"
#include "../PayLoad/PayLoad.h"
#include "../Browse/Browse.h"



namespace NoSqlDb {

	template<typename T>
	class CoreRepository
	{
		
	public:
		using RegularExp = const std::string&;
		CoreRepository(DbCore<T>& db) :dbcore(db),query(db),browse(db)
		{
			keys_ = query.keys();
		}
		bool CheckIn(Path_& srcpath_, Namespace_& namespace_, Keys& keys, Keys& categories, std::string Descrip);
		bool CloseStatus(Namespace_& namespace_, Filename_& filename_);
		bool statusHold(Namespace_& namespace_, Filename_& filename_);
		bool CheckOut(Namespace_ namespace_,Filename_ filename_,const VersionNo&);
		bool dependCheck(DbElement<T> dbelem);
		void showDbFile(Namespace_ namespace_, Filename_ filename_, const VersionNo&);

		
		CoreRepository& select(Conditions<T>& cond);
		CoreRepository& from_(const Keys& keys);
		Keys& keys() { return keys_; }
		void show_dependency(Key& key_);
		static void identify(std::ostream& out = std::cout);
		void getQueryFiles(RegularExp regExp);
		void getCatQueryFiles(RegularExp regExp);

	private:
		bool checkCyclic(Filename_& filename);
		bool dataToDb(Key& key, Filename_& filename, Path_& path, Keys& cat, Keys& childs, std::string Descrip);
		bool dependExist(Keys& keys_);
		NoSqlDb::CheckIn<T> checkin;
		NoSqlDb::CheckOut checkout;
		Version<T> version;
		DbCore<T>& dbcore;
		Browse<T> browse;
		Query<T> query;
		Keys keys_;
		
	};

	// Function to display the contents of a file

	template<typename T>
	inline void CoreRepository<T>::showDbFile(Namespace_ namespace_, Filename_ filename_, const VersionNo& version)
	{
		browse.displayFile(namespace_,filename_,version);
	}

	//Function to checkin the file to the repository

	template<typename T>
	inline bool CoreRepository<T>::CheckIn(Path_& srcpath_, Namespace_& namespace_, Keys& keys,Keys& categories,std::string Descrip)
	{
		if (!FileSystem::Directory::exists("../Repository/"+namespace_))
		{
			FileSystem::Directory::create("../Repository/"+namespace_);
		}
		Filename_ filename = srcpath_.substr(srcpath_.find_last_of("\\") + 1);
		if (version.checkVersion(namespace_, filename, dbcore))
		{
			int appVersion =  version.appendVersion(namespace_, filename, dbcore);
			std::string finalname = namespace_ + "/" + filename + "." + std::to_string(appVersion);
			Key keyname= namespace_ + "::" + filename + "." + std::to_string(appVersion);
			filename=filename+ "." + std::to_string(appVersion);//
			if ( checkin.filePush(srcpath_,finalname))
			{
				if (dependExist(keys))
				{
					dataToDb(keyname, filename, srcpath_, categories,keys, Descrip);
					return true;
				}
				else
				{
					std::cout << std::endl << "Dependencies are not present checkin failed";
					return false;
				}
			}
		}
		else
		{
			Key keyname = namespace_ + "::" + filename + ".1";
			Filename_ finalname = namespace_ + "/" + filename + ".1"  ;
			filename = filename + ".1";//
			if (checkin.filePush(srcpath_, finalname))
			{
				if (dependExist(keys))
				{
					dataToDb(keyname, filename, srcpath_, categories, keys, Descrip);
					return true;
				}
				else
				{
					std::cout << std::endl << "Dependencies are not present checkin failed";
					return false;
				}
			}
		}
		return false;
	}

	// Function to close the checkin status of a file in the repository

	template<typename T>
	inline bool CoreRepository<T>::CloseStatus(Namespace_ & namespace_, Filename_ & filename_)
	{
		if (version.checkVersion(namespace_, filename_, dbcore))
		{
			int appVersion = version.getlatestVersion(namespace_, filename_, dbcore);
			Filename_ finalname = namespace_ + "::" + filename_ + "." + std::to_string(appVersion);
			DbElement<T> elem;
			elem = dbcore[finalname];
			if (dependCheck(elem))
			{
				std::cout << std::endl << "file checkin closed";
				
				elem.CheckinStat(DbElement<PayLoad>::Close);
				dbcore[finalname] = elem;
				return true;
			}
			else
			{
				std::cout << std::endl << "dependency are open";
				if (checkCyclic(finalname))
				{
					CloseStatus(namespace_, filename_);

				}
				return false;
			}
		}
		else
		{
			std::cout << std::endl << "file doesnt exist";
			return false;
		}
		return false;
	}

	// Function to put the checkin status of a file as pending in repository

	template<typename T>
	inline bool CoreRepository<T>::statusHold(Namespace_ & namespace_, Filename_ & filename_)
	{
		if (version.checkVersion(namespace_, filename_, dbcore))
		{
			int nextVer = version.getlatestVersion(namespace_, filename_, dbcore);
			Filename_ finalname = namespace_ + "::" + filename_ + "." + std::to_string(nextVer);
			DbElement<T> elem;
			elem = dbcore[finalname];
			if (dependCheck(elem))
			{
				std::cout << std::endl << "file checkin is pending";
				elem.CheckinStat(DbElement<PayLoad>::Pending);
				dbcore[finalname] = elem;
				return true;
			}
			else
			{
				std::cout << std::endl << "dependency are open";
				return false;
			}
		}
		else
		{
			std::cout << std::endl << "file doesnt exist";
			return false;
		}
		return false;
	}

	// Function to checkout file from the repository 

	template<typename T>
	inline bool CoreRepository<T>::CheckOut(Namespace_ namespace_, Filename_ filename_,const VersionNo& version_)
	{
		Path_ filesrc = "../Repository/" + namespace_ + "/" + filename_ + "." + std::to_string(version_);
		filename_=filename_+"."+ std::to_string(version_);
		if (version.checkVersion(namespace_, filename_, dbcore))
		{
			if (checkout.checkoutFile(filesrc,namespace_)) 
			{
				std::string keyname=namespace_+"::"+filename_;
				DbElement<PayLoad> elem = dbcore[keyname];
				for (auto key:elem.children())
				{
					DbElement<PayLoad> elem1 = dbcore[key];
					filesrc = "../Repository/" + namespace_ + "/" + elem1.name();
					checkout.checkoutFile(filesrc, namespace_);
					for (auto value:elem1.children())
					{
						DbElement<PayLoad> elem2 = dbcore[value];
						filesrc = "../Repository/" + namespace_ + "/" + elem2.name();
						checkout.checkoutFile(filesrc, namespace_);
					}
				}
				std::cout << std::endl << "file checked out";
				return true;
			}
		}
		else
		{
			std::cout << std::endl << "file not found";
			return false;
		}
	
		return false;
	}

	// Function to check whether the file dependencies are closed

	template<typename T>
	inline bool CoreRepository<T>::dependCheck(DbElement<T> dbelem)
	{
		DbElement<T> elem;
		if (!dbelem.children().empty())
		{
			for (auto key : dbelem.children())
			{
				elem = dbcore[key];
				if (elem.CheckinStat()== DbElement<PayLoad>::Open)
				{
					std::cout << std::endl << key << "file is open";
					return false;
				}
			}
		}
		else
		{
			return true;
		}
		return true;
	}

	//Function to check cyclic dependencies 

	template<typename T>
	inline bool CoreRepository<T>::checkCyclic(Filename_& filename)
	{
		DbElement<PayLoad> elem = dbcore[filename];
		for (auto key:elem.children())
		{
			DbElement<PayLoad> elem1 = dbcore[key];
			if  (std::find(elem1.children().begin(), elem1.children().end(), filename) != elem1.children().end())
			{
				elem1 = dbcore[key];
				elem1.CheckinStat( DbElement<PayLoad>::Pending);
				dbcore[key] = elem1;
				return true;
			}
		}
		return false;
	}

	//Function to add checking info to the database

	template<typename T>
	inline bool CoreRepository<T>::dataToDb(Key& key,Filename_& filename,Path_& path,Keys& cat,Keys& childs, std::string Descrip)
	{
		PayLoad payload;
		payload.value(path);
		payload.categories(cat);
		DbElement<T> elem;
		elem.name( filename);
		elem.descrip(Descrip);
		elem.payLoad(payload);
		elem.children(childs);
		dbcore[key] = elem;
		return true;
	}

	// Function to check whther a dependency exists for a key

	template<typename T>
	inline bool CoreRepository<T>::dependExist(Keys& keys_)
	{
		if (keys_.empty())
		{
			return true;
		}
		else
		{
			for (auto key:keys_)
			{
				if (!dbcore.contains(key))
				{
					return false;
				}
			}
			return true;
		}
	}

	

	//Function to query on a vertain selected condition 

	template<typename T>
	inline CoreRepository<T>& CoreRepository<T>::select(Conditions<T>& cond)
	{
		query.select(cond);
		keys_ = query.keys();
		return *this;
	}

	//Function to get keys for a certain condition 

	template<typename T>
	inline CoreRepository<T>& CoreRepository<T>::from_(const Keys & keys)
	{
		query.from(keys);
		keys_ = query.keys();
		return *this;
	}

	// Function to show dependency of a file

	template<typename T>
	inline void CoreRepository<T>::show_dependency(Key& key_)
	{
		DbCore<T> db_;
		DbElement<PayLoad> elem;
		elem = dbcore[key_];
		for (auto value : elem.children())
		{
			db_[value] = dbcore[value];
		}
		showDb(db_);
	}
	//Function to show the name of a file

	template<typename T>
	inline void CoreRepository<T>::identify(std::ostream & out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//Function to query on a key

	template<typename T>
	inline void CoreRepository<T>::getQueryFiles(RegularExp regExp)
	{
		browse.getFiles(regExp);
	}

	//Function to query on a particular category

	template<typename T>
	inline void CoreRepository<T>::getCatQueryFiles(RegularExp regExp)
	{
		browse.getCatFiles(regExp);
	}

}