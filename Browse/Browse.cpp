///////////////////////////////////////////////////////
// Browse.cpp - Implements browse functionalities    //
// ver 1.0											 //
// Author - Sharath Nagendra						 //
// Source - Jim fawcett, Syracuse University		 //
// CSE 687 - Object Oriented Design					 //
///////////////////////////////////////////////////////

#include "Browse.h"
#include "../PayLoad/PayLoad.h"
using namespace NoSqlDb;


#ifdef test_Browse


int main() {
	DbCore<PayLoad> db;
	DbElement<PayLoad> elem;
	elem.name("file_name");
	elem.descrip("sample file");
	db["fn"] = elem;
	Browse<PayLoad> browser(db);
	browser.getQueryFiles("file");

	return 0;
}


#endif // test_Browse
