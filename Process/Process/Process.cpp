///////////////////////////////////////////////////////////////////////
// Process.cpp - class used to start process                         //
// ver 1.0                                                           //
// Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018         //
///////////////////////////////////////////////////////////////////////

#include "Process.h"
#include <iostream>

#ifdef TEST_PROCESS

int main()
{
  std::cout << "\n  Demonstrating code pop-up";
  std::cout << "\n ===========================";

  Process p;
  p.title("test application");
  std::string appPath = "c:/su/temp/Project4Sample/Debug/CodeAnalyzer.exe";
  p.application(appPath);

  std::string cmdLine = "c:/su/temp/Project4Sample/Debug/CodeAnalyzer.exe c:/su/temp/Project4Sample  *.h *.cpp /m /r /f";
  p.commandLine(cmdLine);

  std::cout << "\n  starting process: \"" << appPath << "\"";
  std::cout << "\n  with this cmdlne: \"" << cmdLine << "\"";
  if (!p.create())
  {
    std::cout << "\n  could not create process " << appPath;
  }

  CBP callback = []() { std::cout << "\n  --- child process exited ---"; };
  p.setCallBackProcessing(callback);
  p.registerCallback();

  //std::cout << "\n  after OnExit";
  //std::cout.flush();
  //char ch;
  //std::cin.read(&ch,1);
  return 0;
}

#endif
