/////////////////////////////////////////////////////////////////////
// RepoCore.h - Handles all repo functionality                     //
// ver 1.0                                                         //
//Author- Sharath Nagendra			                               //
//Source- Jim Fawcett                                              //
//CSE687 - Object Oriented Design, Spring 2018                     //
/////////////////////////////////////////////////////////////////////


#include "../RepoCore/RepoCore.h"

using namespace NoSqlDb;


#ifdef Test_REPOCORE

void repositoryTest() {
	DbCore<PayLoad> db;
	Path_ srcpath = "../PayLoad/PayLoad.h";
	Namespace_ namespace_ = "NoSqlDb";
	CoreRepository<PayLoad> repocore(db);
	repocore.CheckIn(srcpath,namespace_);
	std::string x = "xxxx";
	repocore.CheckOut(namespace_,x,1);

	showDb(db);
}

int main() 
{
	repositoryTest();
	getchar();
}

#endif // Test_DBCORE


