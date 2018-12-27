
/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI Functionality is handled               //
// ver 1.0                                                         //
//Author- Sharath Nagendra                                         //
//Source- Jim Fawcett                                              //
//CSE687 - Object Oriented Design, Spring 2018                     //
/////////////////////////////////////////////////////////////////////
/*
* File Operations:
* -------------------
* This package provides functionality for the following objects: 
* - Dropdown box selection handlers
* - All Button clicks
* - Listbox for Files and directories
* 
* Public Interface:
* ---------------------
* processMessgs() - recieve messgs and invokes the received message     
* clearDirectories()- Directory listbox is cleared
* insrtParent()- Navigated to the parent directory
* addDirectories()- Items are added to the directory listbox
* clrFiles()- All the items in the listbox is cleared
* addFile()- Items to file listbox are added
* addClientProcess()- enq the processes
* DispatcherLoadGetDirectories()- Data from server to directory listbox is loaded
* DispatcherLoadGetFiles()- Data from server to file listbox is loaded
* DispatcherReadFile() - Data from the file is read
* DispatcherLoadMakeConnection() - Connection function is handled
* DispatcherLoadGetFile()- single file from server is retrieved 
* DispatcherAcknowledgementCheckin()- Acknowledges checkin
* DispatcherAcknowledgementGetVersion()- Available version of the file from server is retrieved
* DispatcherAcknowledgementViewmeta()- Meta data of a particular file is retrieved 
* DescriptionEnumerator()- Description of a file is retrieved
* DateEnumerator() - Date of creation of a file is retrieved 
* PathEnumerator()- Path of a file is retrieved 
* StatusEnumerator()- Status of a file is retrieved 
* DispatcherAcknowledgementCheckout()- Checkout is acknowledged
* showFile(string fileName, string fileContent)- Content of the file is opened in a new window

* ListDirectoriesOnDoubleClick(object sender, MouseButtonEventArgs e)- Double click on dir list is handled 
* ClickOnConnectButton(object sender, RoutedEventArgs e)- Click on conect button is handled 
* ClickOnCheckinTab(object sender, MouseButtonEventArgs e)- Click on checkin button is handled 
* BrowseOnMouseLeftButtonClick(object sender, MouseButtonEventArgs e)- Click on browse button is handled 
* MouseDoubleClickOnDirList(object sender, MouseButtonEventArgs e)- Double click on checkin directory list is handled
* ClickOnCheckinButton(object sender, RoutedEventArgs e)- Handles checkin button
* ClickOnCheckoutButton(object sender, RoutedEventArgs e)- Handles checkout button
* BrowseFileRadioButton(object sender, RoutedEventArgs e)- Handles radio button to select file to checkin
* AddDependRadioButton(object sender, RoutedEventArgs e)- Handles radio button to add dependency 
* AddCategoryRadioButton(object sender, RoutedEventArgs e)- Handles radio button to add category
ClickOnWPButton(object sender, RoutedEventArgs e) - to get files without parent
* Required Files:
* ---------------
* MainWindow.xaml
* 

* Maintenance History:
* --------------------
*Ver 1.0 : 1 May 2018
*  - first release
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.ObjectModel;
using System.IO;
using MsgPassingCommunication;
namespace GUI
{

    ////// Interaction logic for MainWindow.xaml

    public partial class MainWindow : Window
    {
        public class BoolStringClass
        {
            public string LBTxt { get; set; }
            public bool LBSltd { get; set; }
            public bool ListDepencency { get; set; }
        }
        public ObservableCollection<BoolStringClass> ListOfFileB { get; set; }
        public MainWindow()
        {
            ListOfFileB = new ObservableCollection<BoolStringClass>();
            InitializeComponent();
            this.DataContext = this;
        }

        private string saveFilesPath = "../CppCommWithFileXfer/saveFiles";
        private string sendFilesPath = "../CppCommWithFileXfer/sendFiles";

        private List<String> categoryList = new List<string>();
        private string checkinFileCategory;
        private string checkinFileDependency = "na";
        private string checkinStatus;
        private FileDisplayWin fileWin = null;
        private Stack<string> pathStack_ = new Stack<string>();
        private Translater translater;
        private CsEndPoint endPoint_;
        private Thread rcvThread = null;
        private Dictionary<string, Action<CsMessage>> dispatcher_
          = new Dictionary<string, Action<CsMessage>>();
        private bool connecFlag = false;
        private int windowCond = 0;
        private string CheckinfileName;
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
  
        // recieve messgs and invokes the received message 

        private void processMessages()
        {
            ThreadStart threadProcess = () => {
                while (true)
                {
                    CsMessage messgs = translater.getMessage();
                    string messgId = messgs.value("command");
                    if (dispatcher_.ContainsKey(messgId))
                        dispatcher_[messgId].Invoke(messgs);
                }
            };
            rcvThread = new Thread(threadProcess);
            rcvThread.IsBackground = true;
            rcvThread.Start();
        }

        // Directory listbox is cleared

        private void clearDirectories()
        {
            if (windowCond == 0)
            {
                DirList.Items.Clear();
            }
            else { ciDirListBox.Items.Clear(); }
        }

        // Items are added to the directory listbox

        private void addDirectories(string dir)
        {
            if (windowCond == 0)
            {
                DirList.Items.Add(dir);
            }
            else { ciDirListBox.Items.Add(dir); }
        }

        // Navigated to the parent directory

        private void insrtParent()
        {
            if (windowCond == 0)
            {
                DirList.Items.Insert(0, "..");
            }
            else { ciDirListBox.Items.Insert(0, ".."); }
        }

        // All the items in the listbox is cleared

        private void clrFiles()
        {
            if (windowCond == 0)
            {
                FileList.Items.Clear();
            }
            else { ListOfFileB.Clear(); }
        }

        // Items to file listbox are added

        private void addFile(string file, string path)
        {
            string filename;
            if (System.IO.Path.GetFileName(path) == "Storage") { filename = "root" + "::" + file; }
            else
            {
                filename = System.IO.Path.GetFileName(path) + "::" + file;
            }
            if (windowCond == 0)
            {
                FileList.Items.Add(file);
            }
            else { ListOfFileB.Add(new BoolStringClass { LBSltd = false, LBTxt = filename, ListDepencency = false }); }
        }
        // add client processing for message with key 

        private void addClientProcess(string key, Action<CsMessage> clientProc)
        {
            dispatcher_[key] = clientProc;
        }

        // Data from server to directory listbox is loaded

        private void DispatcherLoadGetDirectories()
        {
            Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
            {
                Action clrDirs = () =>
                {
                    clearDirectories();
                };
                Dispatcher.Invoke(clrDirs, new Object[] { });
                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDir = (string dir) =>
                        {
                            addDirectories(dir);
                        };
                        Dispatcher.Invoke(doDir, new Object[] { enumerator.Current.Value });
                    }
                }
                Action insertUp = () =>
                {
                    insrtParent();
                };
                Dispatcher.Invoke(insertUp, new Object[] { });
            };
            Action<CsMessage> getAuthor = (CsMessage rcvMsg) =>
            {
                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("Author"))
                    {
                        Action<string> doDir = (string dir) =>
                        {
                            addDirectories(dir);
                        };
                        Dispatcher.Invoke(doDir, new Object[] { enumerator.Current.Value });
                    }
                }
            };
            addClientProcess("getDirs", getDirs);
            addClientProcess("getAuthor", getAuthor);
        }
        // Data from server to file listbox is loaded

        private void DispatcherLoadGetFiles()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    this.clrFiles();
                };
                Dispatcher.Invoke(clrFiles, new Object[] { });
                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            addFile(file, rcvMsg.value("path"));
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumerator.Current.Value });
                    }
                }
            };
            addClientProcess("getFiles", getFiles);
        }

        // Data from the file is read

        private void DispatcherReadFile()
        {
            Action<CsMessage> readFile = (CsMessage rcvMsg) =>
            {
                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("content"))
                    {
                        Action<string> mess = (string value) =>
                        {
                            fileWin.FileSpace.Text = enumerator.Current.Value;
                        };
                        Dispatcher.Invoke(mess, new Object[] { enumerator.Current.Value });
                    }
                    if (key.Contains("name"))
                    {
                        Action<string> mess = (string value) =>
                        {
                            fileWin.Title = enumerator.Current.Value;
                        };
                        Dispatcher.Invoke(mess, new Object[] { enumerator.Current.Value });
                    }
                }
            };
            addClientProcess("readFile", readFile);
        }

        // Function to make connection with the server 

        private void DispatcherLoadMakeConnection()
        {
            Action<CsMessage> connection = (CsMessage rcvMsg) =>
            {
                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "Connected")
                            {
                                connecFlag = true;
                                this.statusBarText.Text = "Connected..";
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumerator.Current.Value });
                    }
                }
            };
            addClientProcess("connection", connection);
        }

        //Single file from server is retrieved 

        private void DispatcherLoadGetFile()
        {
            Action<CsMessage> sendFile = (CsMessage rcvMsg) =>
            {
                Action displayFile = () =>
                {
                    string szFileName = rcvMsg.value("fileName");
                    string szFileContent = System.IO.File.ReadAllText("../../SaveFiles" + "/" + szFileName);

                    showFile(szFileName, szFileContent);

                };
                Dispatcher.Invoke(displayFile, new Object[] { });

                Console.WriteLine("File sent message recieved");
            };
            addClientProcess("sendFile", sendFile);
        }

        // Checkin function is loaded and Acknowledges checkin 

        private void DispatcherAcknowledgementCheckin() {

            Action<CsMessage> acknowledgeCheckin = (CsMessage rcvMsg) => {

                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "Checkindone")
                            {

                                this.statusBarText.Text = " Checkin of file is successfull";
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumerator.Current.Value });
                    }
                }

            };
            addClientProcess("CheckinAcknowledgement", acknowledgeCheckin);

        }


        // Available version of the file from server is retrieved

        private void DispatcherAcknowledgementGetVersion()
        {

            Action<CsMessage> acknowledgeGetVersion = (CsMessage rcvMsg) => {

                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.Key;
                    if (key.Contains("version"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            VM_Ver_CB.Items.Clear();
                            for (int i = 0; i < Int32.Parse(status); i++) {

                                VM_Ver_CB.Items.Add(i + 1);
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumerator.Current.Value });
                    }
                }
            };
            addClientProcess("VersionAcknowledgement", acknowledgeGetVersion);
        }


        // Meta data of a particular file is retrieved 

        private void DispatcherAcknowledgementViewmeta()
        {

            Action<CsMessage> AcknowledgeViewMetadata = (CsMessage rcvMsg) => {

                var enumerator = rcvMsg.attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key == "description")
                    {
                        DescriptionEnumerator(enumerator, rcvMsg);
                    }
                    if (enumerator.Current.Key == "date")
                    {
                        DateEnumerator(enumerator, rcvMsg);
                    }
                    if (enumerator.Current.Key == "status")
                    {
                        StatusEnumerator(enumerator, rcvMsg);
                    }
                    if (enumerator.Current.Key == "path")
                    {
                        PathEnumerator(enumerator, rcvMsg);
                    }
                    if (enumerator.Current.Key == "depend")
                    {
                        DependencyEnumerator(enumerator, rcvMsg);
                    }
                    if (enumerator.Current.Key == "cat")
                    {
                        CategoryEnumerator(enumerator, rcvMsg);
                    }
                }
            };
            addClientProcess("MetadataAcknowledgement", AcknowledgeViewMetadata);
        }

        // Description of a file is retrieved

        private void DescriptionEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg) {
            Action<string> connect = (string status) =>
            {
                VM_FD_TB.Text = msg.attributes["description"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });

        }

        // Date of creation of a file is retrieved 

        private void DateEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg)
        {
            Action<string> connect = (string status) =>
            {
                VM_date_TB.Text = msg.attributes["date"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
        }

        // Path of a file is retrieved 

        private void PathEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg)
        {
            Action<string> connect = (string status) =>
            {
                VM_FP_TB.Text = msg.attributes["path"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
        }

     
        // Status of a file is retrieved 
        private void StatusEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg)
        {
            Action<string> connect = (string status) =>
            {
                VM_CIS_TB.Text = msg.attributes["status"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
        }

        //Display the depencency details in view meta data section 

        private void DependencyEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg)
        {
            Action<string> connect = (string status) =>
            {
                VM_depend_TB.Text = msg.attributes["depend"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
        }

        // Display the category of the files in the view meta data screen

        private void CategoryEnumerator(Dictionary<string, string>.Enumerator enumer, CsMessage msg)
        {
            Action<string> connect = (string status) =>
            {
                VM_cat_TB.Text = msg.attributes["cat"];
            };
            Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
        }

        // checkout dispatcher 
        private void Dispatcherackcheckout()
        {

            Action<CsMessage> ackcheckout = (CsMessage rcvMsg) => {

                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "Checkoutdone")
                            {
                                this.statusBarText.Text = " File Checkout Succesfull";
                                for (int i = 0; i < Convert.ToInt64(rcvMsg.value("count")); i++)
                                {
                                    CsEndPoint serverEndPoint = new CsEndPoint();
                                    serverEndPoint.machineAddress = "localhost";
                                    serverEndPoint.port = 8080;

                                    CsMessage msg = new CsMessage();
                                    msg.add("to", CsEndPoint.toString(serverEndPoint));
                                    msg.add("from", CsEndPoint.toString(endPoint_));
                                    msg.add("command", "sendFile");
                                    msg.add("nopop", "true");
                                    string depend = "depend" + (i).ToString();

                                    int pos = rcvMsg.value(depend).IndexOf("::");
                                    string name = rcvMsg.value(depend).Substring(pos + 2, rcvMsg.value(depend).Length - pos - 3);
                                    msg.add("fileName", name);
                                    translater.postMessage(msg);
                                }

                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProcess("ackcheckout", ackcheckout);
        }


        // Function to acknowledge the query result 


        private void DispatcherackQuery()
        {

            Action<CsMessage> ackQuery = (CsMessage rcvMsg) => {

                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "query done")
                            {
                                this.statusBarText.Text = " File query Succesfull";
                                showdbTB(rcvMsg);
                                Console.WriteLine("----------------------------------------------  Query Result --------------------------------------------------");
                                showdb(rcvMsg);
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProcess("ackQuery", ackQuery);
        }

        // Function to get the database from the server 

        private void DispatcherackgetDB()
        {

            Action<CsMessage> ackgetDB = (CsMessage rcvMsg) => {

                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("message"))
                    {
                        Action<string> connect = (string status) =>
                        {
                            if (status == "sending db")
                            {
                                this.statusBarText.Text = " sending db Succesfull";
                                showdb(rcvMsg);
                            }
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProcess("ackgetDB", ackgetDB);
        }

        // Function to get files without parents 

        private void DispatcherackgetWOP()
        {

            Action<CsMessage> ackgetWOP = (CsMessage rcvMsg) => {

                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("keys"))
                    {
                        Action<string> connect = (string status) =>
                        {

                            QueryDbTB.Clear();
                            QueryDbTB.Text = status;
                        };
                        Dispatcher.Invoke(connect, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProcess("ackgetWOP", ackgetWOP);
        }

        // Function to query the database in a textbox
        private void showdbTB(CsMessage msg)
        {
            QueryDbTB.Clear();
            for (int i = 0; i < Convert.ToInt64(msg.value("count")); i++)
            {
                string key = "key" + i.ToString();
                string filename = "fileName" + i.ToString();
                string path = "path" + i.ToString();
                string description = "description" + i.ToString();
                string status = "status" + i.ToString();
                string datetime = "dateTime" + i.ToString();
                string depend = "depend" + i.ToString();
                string category = "category" + i.ToString();

                QueryDbTB.AppendText("\n" + "\bKey :-" + msg.value(key).PadRight(5) + "   \bName :-" + msg.value(filename).PadRight(5)
                    + "\n\bPath :-" + msg.value(path).PadRight(5) +
                    "\n\bDescription :-" + msg.value(description).PadRight(5) +
                    "\n\bStatus :-" + msg.value(status).PadRight(5) + "     \bDate Time :-" + msg.value(datetime).PadRight(1));
                QueryDbTB.AppendText("\n\bDependencies:-  " + msg.value(depend));
                QueryDbTB.AppendText("\n\bCategories:-  " + msg.value(category));
                QueryDbTB.AppendText(Environment.NewLine);
            }

        }

        // Function to display the database 
        private void showdb(CsMessage msg)
        {
            Console.WriteLine("------------------------------------------ Showing DB ----------------------------------------------");
            Console.WriteLine("\n              Key             Name               Path                 Description           Status            Time ");
            for (int i = 0; i < Convert.ToInt64(msg.value("count")); i++)
            {
                string key = "key" + i.ToString();
                string filename = "fileName" + i.ToString();
                string path = "path" + i.ToString();
                string description = "description" + i.ToString();
                string status = "status" + i.ToString();
                string datetime = "dateTime" + i.ToString();
                string depend = "depend" + i.ToString();
                string category = "category" + i.ToString();
                Console.WriteLine(msg.value(key) + "     " + msg.value(filename) + "     " + msg.value(path) + "     " + msg.value(description) + "     " + msg.value(status).PadRight(10) + msg.value(datetime).PadRight(1));
                Console.WriteLine("Dependencies:  " + msg.value(depend));
                Console.WriteLine("Categories:  " + msg.value(category));
                Console.WriteLine("\n");
            }
        }

        // Function to show the file in a differnt window 
        private void showFile(string fileName, string fileContent)
        {
            Console.WriteLine(fileName.ToString());
            FileDisplayWin p = new FileDisplayWin();
            p.FileSpace.Text = fileContent;
            p.Show();
        }

        // Loads all the dispatcher processes

        private void loadDispatcherBrowse()
        {
            DispatcherLoadGetDirectories();
            DispatcherLoadGetFiles();
        }
        private void loadDispatcher()
        {
            DispatcherackQuery();
            Dispatcherackcheckout();
            DispatcherAcknowledgementGetVersion();
            DispatcherLoadMakeConnection();
            DispatcherAcknowledgementCheckin();
            DispatcherReadFile();
            DispatcherAcknowledgementViewmeta();
            loadDispatcherBrowse();
            DispatcherLoadGetFile();
            DispatcherackgetDB();
            DispatcherackgetWOP();
        }

        // Starts the communication, fills the window display with files and directories 

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            endPoint_ = new CsEndPoint();
            endPoint_.machineAddress = "localhost";
            endPoint_.port = 8082;
            translater = new Translater();
            translater.listen(endPoint_);
            processMessages(); // message processing is started here
            loadDispatcher(); // load all the dispatcher functions 
            Thread.Sleep(500);
            saveFilesPath = translater.setSaveFilePath("../../SaveFiles");
            sendFilesPath = translater.setSendFilePath("../../SendFiles");
            Automatic();
        }
  
        // removes the name of the first part of the path

        private string removeFirstDirectory(string path)
        {
            string changedPath = path;
            int pos = path.IndexOf("/");
            changedPath = path.Substring(pos + 1, path.Length - pos - 1);
            return changedPath;
        }
        // Double click on directory list is handled 

        private void ListDirectoriesOnDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (DirList.SelectedItem == null)
                return;
            string selectedDirectory = (string)DirList.SelectedItem;
            string path;
            if (selectedDirectory == "..")
            {
                if (pathStack_.Count > 1) 
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                path = pathStack_.Peek() + "/" + selectedDirectory;
                pathStack_.Push(path);
            }  // display path in Dir TextBlcok
            PathTextBlock.Text = removeFirstDirectory(pathStack_.Peek()); // build message to get dirs and post it
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPt));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            translater.postMessage(msg);        // build message to get files and post it
            msg.remove("command");
            msg.add("command", "getFiles");
            translater.postMessage(msg);
        }

        //Click on conect button is handled 

        private void ClickOnConnectButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("connecting to localhost 8080");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = this.IPAddrName.Text;
            serverEndPt.port = Int32.Parse(this.PortName.Text);
            CsMessage messgs = new CsMessage();
            messgs.add("to", CsEndPoint.toString(serverEndPt));
            messgs.add("from", CsEndPoint.toString(endPoint_));
            messgs.add("command", "connection");
            translater.postMessage(messgs);
            messgs.remove("command");
            getDatabase();
        }

        //Functionality of checkin tab is handled 

        private void ClickOnCheckinTab(object sender, MouseButtonEventArgs e)
        {
            windowCond = 1;
            dependfinal.Items.Clear();
        }
        private void MouseLeftButtonTab(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("\nTab to connect");
        }

        private void MouseLeftButtonTab_1(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("\n Tab to ckeckout");
        }

        private void MouseLeftButtonTab_2(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("\n Tab to view meta data");
        }

        //Functionality of browse button is handled 

        private void BrowseOnMouseLeftButtonClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("\n Tab to browse");
            if (!connecFlag)
                return;
            windowCond = 0;

            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;

            PathTextBlock.Text = "Storage";
            pathStack_.Push("../Storage");
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getDirs");
            messg.add("path", pathStack_.Peek());
            translater.postMessage(messg);
            messg.remove("command");
            messg.add("command", "getFiles");
            translater.postMessage(messg);
            messg.remove("command");
        }

        // Double click on directory list box is handled 

        private void MouseDoubleClickOnDirList(object sender, MouseButtonEventArgs e) {
            if (ciDirListBox.SelectedItem == null)
                return;
            // build path for selected dir
            string selectedDir = (string)ciDirListBox.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                if (pathStack_.Count > 1)  // don't pop off "Storage"
                    pathStack_.Pop();
                else
                    return;
            }
            else
            {
                path = pathStack_.Peek() + "/" + selectedDir;
                pathStack_.Push(path);
            }


            // build message to get dirs and post it
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getDirs");
            messg.add("path", pathStack_.Peek());
            translater.postMessage(messg);
            messg.remove("command"); // build message to get files and post it
            messg.add("command", "getFiles");
            translater.postMessage(messg);
        }

        // Click on checkin button is handled 

        private void ClickOnCheckinButton(object sender, RoutedEventArgs e)
        {
            if (CheckinCheckList())
            {
                Console.WriteLine("Checkin file");
                CsEndPoint serverEndPt = new CsEndPoint();
                serverEndPt.machineAddress = "localhost";
                serverEndPt.port = 8080;
                CsMessage msg = new CsMessage();
                msg.add("to", CsEndPoint.toString(serverEndPt));
                msg.add("from", CsEndPoint.toString(endPoint_));
                msg.add("command", "Checkinfile");
                msg.add("namespace", NamespaceTB.Text);
                msg.add("path", SelectFileTB.Text.ToString());
                msg.add("filename", ExtractFilename(SelectFileTB.Text.ToString()));
                msg.add("description", DescripTB.Text.ToString());
                msg.add("status", checkinStatus);
                msg.add("category", checkinFileCategory);
                msg.add("dependency", checkinFileDependency);
                string sourceFile = System.IO.Path.GetFullPath(SelectFileTB.Text.ToString());
                string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename(SelectFileTB.Text.ToString()));
                System.IO.File.Copy(sourceFile, destFile, true);
                msg.add("sendingFile", ExtractFilename(SelectFileTB.Text.ToString()));
                translater.postMessage(msg);
                msg.show();
            }

        }

        // click on checkout button is handled 

        private void ClickOnCheckoutButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("checkingin file: " + SelectFileCOTB.Text.ToString());
            Console.WriteLine("Checkout file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkoutfile");
            messg.add("namespace", NamespaceCOTB.Text.ToString());
            messg.add("filename", SelectFileCOTB.Text.ToString());
            messg.add("version", VersionCheckList());

            translater.postMessage(messg);
            messg.show();
        }


        // Handles radio button to select file to checkin

        void RadioB_browsefile(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Browse clicked");
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cpp";
            dlg.Filter = "*.h|*.cpp";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                SelectFileTB.Text = filename;
            }
        }

        // Handles radio button to add dependency 

        void AddDependRadioButton(object sender, RoutedEventArgs e)
        {
            ciDirListBox.IsEnabled = true;
            ciFilListBox.IsEnabled = true;
            Console.WriteLine("Add Dependcy clicked");
            AddcatButton.IsEnabled = false;
            confirmDependclick.IsEnabled = true;
            dependfinal.Items.Clear();
            dependfinalTB.Text = "Dependencies";
            dependfinal.SelectionMode = SelectionMode.Single;
            if (!connecFlag)
                return;
            windowCond = 1;
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            PathTextBlock.Text = "Storage";
            pathStack_.Clear();
            pathStack_.Push("../Storage");
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getDirs");
            messg.add("path", pathStack_.Peek());
            translater.postMessage(messg);
            messg.remove("command");
            messg.add("command", "getFiles");
            translater.postMessage(messg);
            messg.remove("command");
        }

        // Handles radio button to add category 

        void AddCategoryRadioButton(object sender, RoutedEventArgs e)
        {
            ciDirListBox.IsEnabled = false;
            ciFilListBox.IsEnabled = false;
            Console.WriteLine("Add Category clicked");
            AddcatButton.IsEnabled = true;
            confirmDependclick.IsEnabled = false;
            dependfinalTB.Text = "Categories";
            categoryList.Clear();
            dependfinal.Items.Clear();
            dependfinal.SelectionMode = SelectionMode.Multiple;
            dependfinal.Items.Add("A");
            dependfinal.Items.Add("B");
            dependfinal.Items.Add("C");
            dependfinal.Items.Add("D");
            dependfinal.Items.Add("E");
            dependfinal.Items.Add("F");
        }

        // Add the dependent files from file list to dependency listbox 

        private void AddSelectFileForDep(object sender, RoutedEventArgs e)
        {

            if (ciFilListBox.IsEnabled == true)
            {
                foreach (var item in ListOfFileB)
                {
                    if (item.LBSltd == true && item.ListDepencency == false)
                    {
                        item.ListDepencency = true;
                        dependfinal.Items.Add(item.LBTxt);
                    }
                    else if (item.LBSltd == false && item.ListDepencency == true)
                    {
                        item.ListDepencency = false;

                        dependfinal.Items.Remove(item.LBTxt);
                    }

                    dependfinal.Items.Refresh();
                }
            }

        }


        //-Function to get files from list box

        private void GetDepFileFromListbox(object sender, SelectionChangedEventArgs e)
        {
            if (dependfinal.SelectionMode == SelectionMode.Multiple)
            {
                foreach (var item in dependfinal.SelectedItems) {
                    if (!categoryList.Contains(item.ToString()))
                    {
                        categoryList.Add(item.ToString());
                    }
                }
            }
        }

        // Function to get name of the file from file path

        private string ExtractFilename(string filename)
        {
            CheckinfileName = System.IO.Path.GetFileName(filename);
            return CheckinfileName;
        }

        // Function to add category in the file meta data

        private void AddCategoryToMeta(object sender, RoutedEventArgs e)
        {

            if (!String.IsNullOrEmpty(otherCatTB.Text))
            {
                categoryList.Add(otherCatTB.Text);
            }
            int i = 0;
            string[] temp = new string[categoryList.Count];

            foreach (var item in categoryList)
            {
                temp[i] = item.ToString();
                i++;
            }
            checkinFileCategory = string.Join("|", temp);
        }

        // Function to select the checkin status of a file

        private void SelectionOfFileCheckinStatus(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)CScombox.SelectedItem).Content.ToString() == "Open") {
                checkinStatus = "Open";
            }
            else if (((ComboBoxItem)CScombox.SelectedItem).Content.ToString() == "Close") {
                checkinStatus = "Close";
            }
        }

        // Function that handles the view meta data button

        private void ClickOnVieMetaButton(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPt));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "viewmeta");
            msg.add("namespace", VM_NS_TB.Text.ToString());
            msg.add("filename", VM_FN_TB.Text.ToString());
            msg.add("version", VM_Ver_CB.SelectedItem.ToString());
            translater.postMessage(msg);
        }

        // Function to get the version of the file

        private void GetVersionOnClick(object sender, RoutedEventArgs e)
        {
            VM_Ver_CB.IsEnabled = true;


            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getversion");
            msg.add("namespace", VM_NS_TB.Text.ToString());
            msg.add("filename", VM_FN_TB.Text.ToString());
            translater.postMessage(msg);
        }

        // Function to confirm the dependency selection

        private void ConfirmDependencyOnClick(object sender, RoutedEventArgs e)
        {
            int i = 0;
            string[] temp = new string[dependfinal.Items.Count];
            if (dependfinal.Items.Count == 0) {
                checkinFileDependency = "na";
                return;
            }
            foreach (var item in dependfinal.Items) {
                temp[i] = item.ToString();
                i++;
            }

            checkinFileDependency = string.Join("|", temp);
        }

        // Function to check whether the textboxs are emprty before checkin 

        private bool CheckinCheckList() {
            if (!string.IsNullOrEmpty(DescripTB.Text) && !string.IsNullOrEmpty(NamespaceTB.Text) && !string.IsNullOrEmpty(SelectFileTB.Text) && !string.IsNullOrEmpty(checkinStatus) && !string.IsNullOrEmpty(checkinFileCategory) /*&& !string.IsNullOrEmpty(checkinFileDependency)*/)
            {
                return true;
            }
            return false;
        }

        // Function to get the resulting version of file

        private string VersionCheckList() {
            if (string.IsNullOrEmpty(versionTB.Text.ToString())) {
                return "0";
            }
            return versionTB.Text.ToString();
        }

        // Function below is triggered when a version is selected in combobox 

        private void VM_Ver_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

        // Function to open a file when doubled clicked in browse filelist

        private void ListFileOnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // build path for selected dir
            string selectedFile = pathStack_.Peek() + "/" + (string)DirList.SelectedItem;

            Console.WriteLine("++" + selectedFile);


            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;

            CsMessage messgs = new CsMessage();
            messgs.add("to", CsEndPoint.toString(serverEndPt));
            messgs.add("from", CsEndPoint.toString(endPoint_));
            messgs.add("command", "sendFile");
            messgs.add("path", selectedFile);
            messgs.add("nopop", "false");
            string[] temp = new string[2];
            temp = FileList.SelectedItem.ToString().Split(':');

            messgs.add("fileName", temp[0]);
            translater.postMessage(messgs);
        }

        // Function to demostrate the functionalities automatically 

        private void Automatic() {

            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI Connect functionality ");
            AutomaticConnection();
            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI Checkin functionality");
            AutomaticCheckin();
            Console.WriteLine("\n--------------------------------------\nDemonstrating GUI Checkout functionality");
            AutomaticCheckout();
            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI Browse and View File functionality");
            Console.WriteLine("\n \b For Browse requiremnt files are loaded in browse tab \nClick on DateTime.h contents are dsplayed \n file transfer is also shown");
            AutomaticBrowse();
            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI View Meta data functionality");
            AutomaticViewMetadata();
            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI View file without parent functionality");
            AutomaticWithoutParents();
            Console.WriteLine("\n---------------------------------------\nDemonstrating GUI Query functionality");
            autoquery();
        }

        // Function implemention the automated connect function

        private void AutomaticConnection() {
            Console.WriteLine("Address: localhost Port:8080");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = this.IPAddrName.Text;
            serverEndPt.port = Int32.Parse(this.PortName.Text);
            CsMessage messgs = new CsMessage();
            messgs.add("to", CsEndPoint.toString(serverEndPt));
            messgs.add("from", CsEndPoint.toString(endPoint_));
            messgs.add("command", "connection");
            translater.postMessage(messgs);
            messgs.remove("command");
            messgs.show();
        }

        // Function implemention the automated checkin function

        private void AutomaticCheckin() {

            checkinTest1();
            checkinTest2();
            checkinTest3();
            checkinTest4();
            checkinTest5();
            checkinTest6();
            checkinTest7();
            getDatabase();
        }

        //- Function implemention the automated checkout function

        private void AutomaticCheckout() {
            Console.WriteLine("Checking out file of namespace : NoSqlDb");
            Console.WriteLine("File: Persist.h");
            Console.WriteLine("Version: lastest");
            NamespaceCOTB.Text = "NoSqlDb";
            SelectFileCOTB.Text = "Persist.h";
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkoutfile");
            messg.add("namespace", NamespaceCOTB.Text.ToString());
            messg.add("filename", SelectFileCOTB.Text.ToString());
            messg.add("version", VersionCheckList());
            translater.postMessage(messg);
            messg.show();
            Console.Write("Checkingout file at GUI/savefiles \n requesting Dependencies too");
        }

        // Function implemention the automated browse function

        private void AutomaticBrowse() {

            Console.WriteLine("\nSelecting DateTime.h from filelist \n On double click View Content Are Displyed \n For this demonstration DateTime.h is transfered from ../Storage/NoSqlDb/DateTime.h to ../GUI/SaveFiles ");
            string selectedFile = "../Storage/NoSqlDb/";
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "sendFile");
            messg.add("fileName", "DateTime.h");
            messg.add("path", selectedFile);
            messg.add("nopop", "false");
            translater.postMessage(messg);
            messg.show();
        }

    // Function to implement automatic view metadata function 

        private void AutomaticViewMetadata() {
            Console.WriteLine("Namespace: NoSqlDb");
            Console.WriteLine("File: Message.h");

            VM_NS_TB.Text = "SomeDb";
            VM_FN_TB.Text = "Checkin.h";
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getversion");
            messg.add("namespace", VM_NS_TB.Text.ToString());
            messg.add("filename", VM_FN_TB.Text.ToString());
            translater.postMessage(messg);
            Console.WriteLine("\ngetting Versions of file");
            messg.show();
            Console.WriteLine("Getting Meta Data of 1th version of NoSqlDb::Message.h");
            VM_Ver_CB.SelectedIndex = 1;

            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg1 = new CsMessage();
            messg1.add("to", CsEndPoint.toString(serverEndPt));
            messg1.add("from", CsEndPoint.toString(endPoint_));
            messg1.add("command", "viewmeta");
            messg1.add("namespace", VM_NS_TB.Text.ToString());
            messg1.add("filename", VM_FN_TB.Text.ToString());
            messg1.add("version", "1");
            translater.postMessage(messg1);
            messg1.show();
            getDatabase();
        }

        // Function to automate files without parent directory

        private void AutomaticWithoutParents()
        {
            Console.Write("\ngetting all files without parent");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getWOP");
            translater.postMessage(messg);
            getDatabase();
        }

        // Function to handle click on query button 

        private void ClickOnQueryButton(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "query");
            messg.add("name", queryNameTB.Text.ToString());
            messg.add("version", verTB.Text.ToString());
            messg.add("descrip", queryDescripTB.Text.ToString());
            messg.add("datefrom", queryFromTB.Text.ToString());
            messg.add("dateto", queryToTB.Text.ToString());
            messg.add("category", queryCatTB.Text.ToString());
            messg.add("depend", queryDependTB.Text.ToString());
            translater.postMessage(messg);
        }
        
        // Function to display the database 

        private void getDatabase()
        {
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getDatabase");
            translater.postMessage(messg);
        }

        // Function to show the file checkin

        private void checkinTest1()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "NoSqlDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../DateTime/DateTime.h"));
            messg.add("filename", ExtractFilename("../../../DateTime/DateTime.h"));
            messg.add("description", "Header file for datetime project ");
            messg.add("status", "Close");
            messg.add("category", "A|B");
            messg.add("dependency", "na");
            string sourceFile = System.IO.Path.GetFullPath("../../../DateTime/DateTime.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../DateTime/DateTime.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../DateTime/DateTime.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest2()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "NoSqlDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../Translater/Translater.h"));
            messg.add("filename", ExtractFilename("../../../Translater/Translater.h"));
            messg.add("description", "Header file for Translater project ");
            messg.add("status", "Close");
            messg.add("category", "A|C");
            messg.add("dependency", "na");
            string sourceFile = System.IO.Path.GetFullPath("../../../Translater/Translater.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../Translater/Translater.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../Translater/Translater.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest3()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "NoSqlDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../Persist/Persist.h"));
            messg.add("filename", ExtractFilename("../../../Persist/Persist.h"));
            messg.add("description", "Header file for Persist project ");
            messg.add("status", "Close");
            messg.add("category", "B|C");
            messg.add("dependency", "NoSqlDb::DateTime.h.1");
            string sourceFile = System.IO.Path.GetFullPath("../../../Persist/Persist.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../Persist/Persist.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../Persist/Persist.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest4()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "NoSqlDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../Persist/Persist.h"));
            messg.add("filename", ExtractFilename("../../../Persist/Persist.h"));
            messg.add("description", "Header file for Persist project ");
            messg.add("status", "Open");
            messg.add("category", "B|C");
            messg.add("dependency", "NoSqlDb::DateTime.h.1");
            string sourceFile = System.IO.Path.GetFullPath("../../../Persist/Persist.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../Persist/Persist.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../Persist/Persist.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest5()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "SomeDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../DbCore/DbCore.h"));
            messg.add("filename", ExtractFilename("../../../DbCore/DbCore.h"));
            messg.add("description", "Header file for DbCore project ");
            messg.add("status", "Close");
            messg.add("category", "C|D");
            messg.add("dependency", "na");
            string sourceFile = System.IO.Path.GetFullPath("../../../DbCore/DbCore.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../DbCore/DbCore.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../DbCore/DbCore.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest6()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "SomeDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../Checkin/Checkin.h"));
            messg.add("filename", ExtractFilename("../../../Checkin/Checkin.h"));
            messg.add("description", "Header file for Persist project ");
            messg.add("status", "Close");
            messg.add("category", "D|E");
            messg.add("dependency", "SomeDb::DbCore.h.1");
            string sourceFile = System.IO.Path.GetFullPath("../../../Checkin/Checkin.h");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../Checkin/Checkin.h"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../Checkin/Checkin.h"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function to show the file checkin

        private void checkinTest7()
        {
            Console.WriteLine("Checkin file");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "Checkinfile");
            messg.add("namespace", "SomeDb");
            messg.add("path", System.IO.Path.GetFullPath("../../../Checkin/Checkin.cpp"));
            messg.add("filename", ExtractFilename("../../../Checkin/Checkin.cpp"));
            messg.add("description", "Header file for Checkin project ");
            messg.add("status", "Close");
            messg.add("category", "E");
            messg.add("dependency", "SomeDb::DbCore.h.1|SomeDb::Checkin.h.1");
            string sourceFile = System.IO.Path.GetFullPath("../../../Checkin/Checkin.cpp");
            string destFile = System.IO.Path.Combine("../../SendFiles/", ExtractFilename("../../../Checkin/Checkin.cpp"));
            System.IO.File.Copy(sourceFile, destFile, true);
            messg.add("sendingFile", ExtractFilename("../../../Checkin/Checkin.cpp"));
            translater.postMessage(messg);
            messg.show();
        }

        // Function for double click on file without parent 

        private void ClickOnWPButton(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "getWOP");
            translater.postMessage(messg);
        }

        private void autoquery()
        {
            Console.WriteLine("Query on categories ----------  A,B");
            CsEndPoint serverEndPt = new CsEndPoint();
            serverEndPt.machineAddress = "localhost";
            serverEndPt.port = 8080;
            CsMessage messg = new CsMessage();
            messg.add("to", CsEndPoint.toString(serverEndPt));
            messg.add("from", CsEndPoint.toString(endPoint_));
            messg.add("command", "query");
            messg.add("name", queryNameTB.Text.ToString());
            messg.add("version", verTB.Text.ToString());
            messg.add("descrip", queryDescripTB.Text.ToString());
            messg.add("datefrom", queryFromTB.Text.ToString());
            messg.add("dateto", queryToTB.Text.ToString());
            messg.add("category", "B|A");
            messg.add("depend", queryDependTB.Text.ToString());
            translater.postMessage(messg);
            Console.WriteLine("Query on filename ---- DbCore.h");
            CsMessage messg1 = new CsMessage();
            messg1.add("to", CsEndPoint.toString(serverEndPt));
            messg1.add("from", CsEndPoint.toString(endPoint_));
            messg1.add("command", "query");
            messg1.add("name", "DbCore");
            messg1.add("version", verTB.Text.ToString());
            messg1.add("descrip", queryDescripTB.Text.ToString());
            messg1.add("datefrom", queryFromTB.Text.ToString());
            messg1.add("dateto", queryToTB.Text.ToString());
            messg1.add("category", queryCatTB.Text.ToString());
            messg1.add("depend", queryDependTB.Text.ToString());
            translater.postMessage(messg1);
            Console.WriteLine("Query on version ------- 2");
            CsMessage messg2 = new CsMessage();
            messg2.add("to", CsEndPoint.toString(serverEndPt));
            messg2.add("from", CsEndPoint.toString(endPoint_));
            messg2.add("command", "query");
            messg2.add("name", queryNameTB.Text.ToString());
            messg2.add("version", "2");
            messg2.add("descrip", queryDescripTB.Text.ToString());
            messg2.add("datefrom", queryFromTB.Text.ToString());
            messg2.add("dateto", queryToTB.Text.ToString());
            messg2.add("category", queryCatTB.Text.ToString());
            messg2.add("depend", queryDependTB.Text.ToString());
            translater.postMessage(messg2);
            Console.WriteLine("Query on dependencies  -----  SomeDb::DbCore.h.1");
            CsMessage messg3 = new CsMessage();
            messg3.add("to", CsEndPoint.toString(serverEndPt));
            messg3.add("from", CsEndPoint.toString(endPoint_));
            messg3.add("command", "query");
            messg3.add("name", queryNameTB.Text.ToString());
            messg3.add("version", verTB.Text.ToString());
            messg3.add("descrip", queryDescripTB.Text.ToString());
            messg3.add("datefrom", queryFromTB.Text.ToString());
            messg3.add("dateto", queryToTB.Text.ToString());
            messg3.add("category", queryCatTB.Text.ToString());
            messg3.add("depend", "SomeDb::DbCore.h.1");
            translater.postMessage(messg3);

        }

        private void VM_FD_TB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
