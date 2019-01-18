using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Security_Check;

namespace OpenFile
{
    //Transaction assigned as automatic
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    //Creating an external command to open linked files
    public class OpenFile:IExternalCommand
    {
        //instances to store application and the document
        UIDocument m_document=null;
        DirectoryInfo directory = null;

        bool security = false;

        bool choice=false;

        string  linkDirectoryPath=null, linkTextFileName = null, elementFamily = null;

        List<string> linkedFiles = new List<string>();

        //execute method for the IExternalCommand
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //call to the security check method to check for authentication
                security = SecurityLNT.Security_Check();

                if (security == false)
                {
                    return Result.Succeeded;
                }


                //open  the active document in revit
                m_document = commandData.Application.ActiveUIDocument;

                bool loopContinue = false;

                do
                {
                    choice=ChooseAndFindFamily();
                    if (choice)
                    {
                        linkedFiles.Clear();

                        GetDirectoryAndFile();

                        //create instance of the form
                        using (OpenFiles formInstance = new OpenFiles())
                        {
                            formInstance.ShowLinkedData(linkedFiles, linkDirectoryPath);

                            DialogResult dialogueResult = formInstance.ShowDialog();

                            if (DialogResult.Cancel == dialogueResult)
                            {
                                loopContinue = false;
                            }
                            else if (DialogResult.Retry == dialogueResult)
                            {
                                loopContinue = true;
                            }

                            formInstance.Close();
                        }
                    }
                    else
                        loopContinue = false;
                                        
                } while (loopContinue == true);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Autodesk.Revit.UI.Result.Failed;
            }
            throw new NotImplementedException();
        }



        //method to prompt user to get the element and find its family
        private bool ChooseAndFindFamily()
        {
            Element element = null;
            Reference elementReference = null;

            try
            {
                //prompt the user to make the slection and convert the reference into element
                elementReference = m_document.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element
                    , new SelectionFilter(m_document.Document), "Pick the element display the linked files.");
                element = m_document.Document.GetElement(elementReference);

                //get the element type from the selected element
                element = m_document.Document.GetElement(element.GetTypeId());

                elementFamily = element.Name;

                return true;
            }
            catch
            {
                return false;
            }
            
        }



        //method to get directory and file
        private bool GetDirectoryAndFile()
        {
            try
            {
                //get the path of the active document in revit
                linkTextFileName = m_document.Document.PathName;

                //convert the active file path into directory name
                if (File.Exists(linkTextFileName))
                {
                    directory = new FileInfo(linkTextFileName).Directory;
                    linkTextFileName = Path.GetFileNameWithoutExtension(linkTextFileName);      
                }

                //linked files folder path in the project directory
                linkDirectoryPath = directory.ToString() + @"\" + linkTextFileName;

                //check if directory exists
                if (Directory.Exists(linkDirectoryPath))
                {
                    //if file in the directory is existing
                    linkTextFileName = linkDirectoryPath + @"\LinkedFiles.txt";
                    if (System.IO.File.Exists(linkTextFileName))
                    {
                        GetDataFromFile();
                    }
                    else
                    {
                        MessageBox.Show("File Missing!");
                    }
                }
                else
                    MessageBox.Show("Directory Missing!");
                return true;
            }
            catch
            {
                return false;
            }                               
        }



        //method to get data from file corresponding to the element family
        private void GetDataFromFile()
        {
            bool familyExists = false;
            string line = null;

            try
            {
                //start a new file reader
                System.IO.StreamReader file = new System.IO.StreamReader(linkTextFileName);

                //check whether the file contains the currently selected family or not
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(elementFamily))
                    {
                        familyExists = true;
                        break;
                    }
                }
                //close the file
                file.Close();
            
                //if the family exist read data into a string list and input it into the data grid view 
                if (familyExists == true)
                {
                    //read all the contents of the file into a string array
                    string[] fileContents = File.ReadAllLines(linkTextFileName);

                    for (int i = 0; i < fileContents.Count(); i++)
                    {
                        //find where the family is repeated in the string array
                        if (fileContents[i].Contains(elementFamily))
                        {
                            //from the above detected location onwards copy the filenames into another list                       
                            for (int j = i+1; j < fileContents.Count(); j++)
                            {
                                if (fileContents[j].Contains("Linked Files"))
                                    linkedFiles.Add(fileContents[j]);
                                else
                                    break;
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("No Records Found!");
            }
            catch 
            {
                return;
            }
            
        }

    }
}
