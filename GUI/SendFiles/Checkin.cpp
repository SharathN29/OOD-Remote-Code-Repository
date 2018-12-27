#ifdef TEST_Checkin
//////////////////////////////////////////////////////////////
// CheckIn.cpp - Implements the checkin functionalities		//
// ver 1.0												    //
// Author - Sharath Nagendra							    //
// CSE 687 - Object Oriented Design, Spring 2018		    //
//////////////////////////////////////////////////////////////


#include "Checkin.h"
#include "../PayLoad/PayLoad.h"

using namespace NoSqlDb;

void test() {
	DbCore<PayLoad> db;

	DbElement<PayLoad> elem = makeElement<PayLoad>("a name", "a description");
	PayLoad pl;
	pl.value("..//..//Repository//NoSqlDb//PayLoad.h.1");
	pl.categories().push_back("cat2");
	elem.payLoad(pl);
	db["NoSqlDb::PayLoad.h.1"] = elem;
	db["NoSqlDb::PayLoad.h.2"] = elem;
	showDb(db);
	Version<PayLoad> v;
	int y = v.appendVersion("NoSqlDb", "PayLoad.h", db);
	std::cout << std::endl << y;
	if (v.checkVersion("NoSqlDb", "PayLoad.h.2", db))
	{
		std::cout << "yes";
	}
	else
	{
		std::cout << "no";
	}

	CheckIn<PayLoad> checkin;
	std::string xx= "../PayLoad/PayLoad.h";
	std::string yy = "NoSqlDb/PayLoad.h.1";
	checkin.filePush(xx,yy);

}

int main() {
	test();
	
	getchar();
}

#endif