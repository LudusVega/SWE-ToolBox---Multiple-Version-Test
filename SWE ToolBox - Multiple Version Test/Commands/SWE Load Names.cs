using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

//Identify the namespace
namespace SWE_Toolbox
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class LoadNameConvert : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Access the electrical settings
            ElectricalSetting electricalSetting = ElectricalSetting.GetElectricalSettings(doc);

            //Get all circuits (ElectricalSystems) that exist in the project. Class type is ElectricalSystem, element is an instance and not a type.
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementCollector electricalSystems = collector.OfClass(typeof(ElectricalSystem));
            electricalSystems.WhereElementIsNotElementType();

            //Define list to store all circuits (ElectricalSystems)
            List<ElectricalSystem> circuitList = electricalSystems.Cast<ElectricalSystem>().ToList();

            //Define new load name and instantiate it as empty string.
            string sWELoadName = "";

            //Define sets of space number and equipment IDs
            HashSet<string> spaceNumbers = new HashSet<string>(1000);
            HashSet<string> tagNos = new HashSet<string>(1000);

            //Define string builder elements for debugging.
            StringBuilder sb = new StringBuilder();
            StringBuilder ln = new StringBuilder();

            //Begin the revit transaction here. Transaction only occurs when setting a value to a parameter. I.E. load name switch
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Load Names to SWE Style");

                //Iterate through each circuit (ElectricalSystem) and find the panel name, load classification, circuit number, load class 
                foreach (var circuit in circuitList)
                {

                    //Define variables for circuit information
                    string panelName = circuit.PanelName;
                    string circuitNumber = circuit.CircuitNumber;
                    string loadClass = circuit.get_Parameter(BuiltInParameter.CIRCUIT_LOAD_CLASSIFICATION_PARAM).AsString();

                    //Generate a set of all elements on the circuit
                    ElementSet elementSet = circuit.Elements;

                    //Clear the space numbers from the previous iteration of the loop to eliminate continus appending
                    spaceNumbers.Clear();

                    //Iterate through each family instance on the circuit and assign space numbers
                    foreach (FamilyInstance familyInstance in elementSet)
                    {
                        var space = familyInstance.Space;
                        var spaceNumber = "null";

                        //If null, ignore and continue
                        if (space != null)
                        {
                            spaceNumber = space.Number;
                            spaceNumbers.Add(spaceNumber);
                        }
                        else
                        {
                        }

                    }

                    //Check if circuit is mechanical, plumbing, or user loads. (I.E. anything other than receptacles or lighting.
                    //If so, list it as {"EQ: " tagNos "RM " spaceNumbers}
                    if (loadClass == "Motor" || loadClass == "Resistance" || loadClass == "Miscellaneous")
                    {
                        //Define parameters and string for equipment Ids
                        Parameter comment = null;
                        Parameter mark = null;
                        string tagNo = "null";
                        tagNos.Clear();

                        //Iterate through each family instance in the circuit and extract and combine the ingredients for the equipment Ids.
                        foreach (FamilyInstance familyInstance in elementSet)
                        {
                            tagNo = familyInstance.LookupParameter("Tag No.").AsString();
                            tagNos.Add(tagNo);
                        }
                    }
                    else
                    {
                    }

                    //Switch paramenter "Load Name" with sWELoadName depending on criteria.
                    //The actual transaction with Revit is here. 
                    //Would like to change this to switch case.
                    Parameter loadNameParam = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_NAME);
                    if (loadNameParam != null && loadNameParam.StorageType == StorageType.String)
                    {
                        string currentLoadName = loadNameParam.AsString();
                        if (!string.IsNullOrEmpty(currentLoadName))
                        {
                            if (loadClass == "Recept")
                            {
                                sWELoadName = "RCPT: RM " + string.Join(", ", spaceNumbers);
                            }
                            else
                            {
                                if (loadClass == "Lighting")
                                {
                                    sWELoadName = "LTG: RM " + string.Join(", ", spaceNumbers);
                                }
                                else
                                {
                                    sWELoadName = "EQ: " + string.Join(", ", tagNos)+ spaceNumbers;
                                }
                                
                            }
                            
                            // Set the new SWE styled load name to uppercase
                            loadNameParam.Set(sWELoadName.ToUpper());
                        }
                        else
                        {
                        }
                    }

                    //The below commented-out line of code, appends the string builder string to display later in the task dialog for debugging
                    sb.AppendLine("");
                    sb.AppendLine($"Panel: {panelName}, Circuit Number: {circuitNumber}");
                    sb.AppendLine($"Load Class: {loadClass}");
                    sb.AppendLine($"RM: {string.Join(", ", spaceNumbers)}");
                    sb.AppendLine($"Load Name: {sWELoadName}");
                    sb.AppendLine($"Tag No.: {string.Join(", ", tagNos)}");
                }

                //Commit the revit transaction
                tx.Commit();
                tx.Dispose();
            }

            //Uncomment this out to display the string with all circuit info. Helpful for troubleshoodting.
            //TaskDialog.Show("Electrical System Info", sb.ToString());

            return Result.Succeeded;
        }


    }
}
