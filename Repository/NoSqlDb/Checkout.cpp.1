////////////////////////////////////////////////////////////////////
// CheckOut.cpp - Calls the function defined in CheckOut.h		  //
// ver 1.0														  //
// Author - Sharath Nagendra									  //
// CSE 687 - Object Oriented Design								  //
////////////////////////////////////////////////////////////////////



#include "Checkout.h"

using namespace NoSqlDb;

//Function to check-out file from the repository to a user folder

bool CheckOut::checkoutFile(Path_ srcpath_,Namespace_ namespace_)
{
	if (!FileSystem::Directory::exists("../ServerPrototype/SendFiles/"))
	{
		FileSystem::Directory::create("../ServerPrototype/SendFiles/");
	}
	std::string finalname;
	 finalname = srcpath_.substr(0, srcpath_.find_last_of("."));
	if (file_copy(srcpath_, finalname))
	{
		std::cout << std::endl << finalname << " File has been copied";
		return true;
	}
	else
	{
		std::cout << std::endl << finalname << " File could not be copied";
	}
	return false;
}

//Function to copy files from Repository to user folder

bool CheckOut::file_copy(Path_ & srcpath, std::string finalname)
{
	std::string finalName = finalname.substr(finalname.find_last_of("/"),finalname.length());
	std::string destination = "../ServerPrototype/SendFiles" + finalName;
	if (FileSystem::File::copy(srcpath, destination))
	{
		return true;
	}
	return false;
}

//Function to show the file name

void NoSqlDb::CheckOut::identify(std::ostream & out)
{
	out << "\n  \"" << __FILE__ << "\"";
}




#ifdef Test_Checkout
void test() {
	CheckOut checkout;
	std::string x = "../Repository/NoSqlDb/PayLoad.h.1";
		checkout.checkoutFile(x, "NoSqlDb");
}

int main() 
{
	test();
	getchar();
}



#endif // Test_Checkout
