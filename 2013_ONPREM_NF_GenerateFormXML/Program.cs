/* Program.cs

   Copyright (c) 2016 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace _2013_ONPREM_NF_GenerateFormXML
{
    /// <summary>
    ///     A console application for generating Nintex Form XML from XML fragments and an XML form description file.
    ///     The input file is named (form_input.xml). You can follow the instructions on drafting or generating the file.
    ///     You can find the XML files references in this application in the solution folder in a folder named XMLParts.
    ///     Make sure the file references are specific to your environment before executing the application.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            GenerateFormXml();
        }

        public static void GenerateFormXml()
        {
            // Create, load, and then extract the Form XML document without specified controls.
            // the filepath to your Form XML.
            var formxml = new XmlDocument();
            // TODO: update path to: <filepathpath>Form_blank.xml
            formxml.Load(@"");

            // Add namespaces to the namespace manager (prefix and then namespace) in Form XML.
            var formsnsmgr = new XmlNamespaceManager(formxml.NameTable);
            formsnsmgr.AddNamespace("n", "http://schemas.datacontract.org/2004/07/Nintex.Forms");
            formsnsmgr.AddNamespace("n1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            formsnsmgr.AddNamespace("d2p1", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");

            //Set the values for the form used in the layout process.
            var yLeft = 0; // y coordinates of position in layout.
            var xTop = 0; // X coordinates of position in layout.
            var formWidth = 701;
                // Width of the Form in the Form layout. Form height (default 600) in desktop is not used in the app.

            //Add the path to your form instructions XML document.
            var formInst = new XmlDocument();

            // TODO: update path to: <filepathpath>Form_input.xml
            formInst.Load(@"");

            // Loop through each control in the instruction XML
            var xmlControl = formInst.GetElementsByTagName("control");
            for (var i = 0; i < xmlControl.Count; i++)
            {
                var con = i + 1;

                // FormControlUniqueId
                var iterguid = Guid.NewGuid(); // Guid for control set (controlproperties and contorllayout)

                //Read the height value for the control in the form instruction.
                var xpathHeight = "/form/control[" + con + "]/height";
                var n1 = formInst.DocumentElement.SelectSingleNode(xpathHeight);
                var val1 = n1.FirstChild.Value;
                var height = int.Parse(val1);

                //Read the width value for the control in the form instruction.
                var xpathWidth = "/form/control[" + con + "]/width";
                var n2 = formInst.DocumentElement.SelectSingleNode(xpathWidth);
                var val2 = n2.FirstChild.Value;
                var width = int.Parse(val2);

                // Create the contorllayout object and assign the height, width, and current control layout positon (X, Y).
                var controllayout = new FormControlLayout();
                controllayout.FormControlUniqueId = iterguid;
                controllayout.Height = height;
                controllayout.Width = width;
                controllayout.Top = xTop;
                controllayout.Left = yLeft;

                //Point to FormControLayouts in the Form XML and add the layout object using AddControlLayout method.
                var laypath = "/n:Form/n:FormLayouts/n:FormLayout/n:FormControlLayouts";
                var laynode = formxml.DocumentElement.SelectSingleNode(laypath, formsnsmgr);
                AddControlLayout(laynode, controllayout);

                //Read the type of control and then add, and then run the routine for adding each to the Form XML.
                var xpathType = "/form/control[" + con + "]/type";
                var typeNode = formInst.DocumentElement.SelectSingleNode(xpathType);
                var controlType = typeNode.FirstChild.Value;

                //Read the type of control and then add, and then run the routine for adding each to the Form XML.
                var xpathText = "/form/control[" + con + "]/text";
                var textNode = formInst.DocumentElement.SelectSingleNode(xpathText);
                var controlText = textNode.FirstChild.Value;

                switch (controlType)
                {
                    case "TextBox":
                        AddTextBoxProperties(formxml, formsnsmgr, iterguid, controlText);
                        break;
                    case "Label":
                        AddLabelProperties(formxml, formsnsmgr, iterguid, controlText);
                        break;
                    case "YesNo":
                        AddYesNoProperties(formxml, formsnsmgr, iterguid, controlText);
                        break;
                    case "Button":
                        AddButtonProperties(formxml, formsnsmgr, iterguid, controlText);
                        break;
                    case "ListBox":
                        // Load choces into the choiceList=
                        var choicesPath = "/form/control[" + con + "]/choices";
                        var controlChoicesNode = formInst.DocumentElement.SelectSingleNode(choicesPath);
                        // XmlNodeList controlChoices = controlChoicesNode.ChildNodes;
                        var choiceArray = new string[controlChoicesNode.ChildNodes.Count];
                        for (var c = 0; c < controlChoicesNode.ChildNodes.Count; c++)
                        {
                            var aChoice = controlChoicesNode.ChildNodes[c].InnerText;
                            choiceArray[c] = aChoice;
                        }
                        AddChoiceProperties(formxml, formsnsmgr, iterguid, controlText, choiceArray);
                        break;
                    case "Image":
                        //Load the image URL value.
                        var xpathTypeUrl = "/form/control[" + con + "]/url";
                        var imUrl = formInst.DocumentElement.SelectSingleNode(xpathTypeUrl);
                        var controlUrl = imUrl.InnerText;
                        AddImageProperties(formxml, formsnsmgr, iterguid, controlText, controlUrl);
                        break;
                }

                // This the routine that will perform the autolayout of the form controls by moving the control layout position with
                // in the bounds of the form width.
                yLeft += width;
                if (yLeft + width > formWidth)
                {
                    yLeft = 0;
                    xTop += height;
                }
            }

            //Convert the form to text, and strip the namespace string fragment inserted on the serialization
            //of the formcontrollayout object, and then save the file to the the target location. 
            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            formxml.WriteTo(xmlTextWriter);
            var outprocess = stringWriter.ToString();
            outprocess = outprocess.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"\"", " ");
            // TODO: update path to: <filepathpath>. Use the path and filename of the outputfile.
            File.WriteAllText(@"", outprocess);
            Console.WriteLine(outprocess);
            Console.ReadLine();
        }

        /// <summary>
        ///     Method to load the TextBox XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control complex
        ///     elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr">Namespace Manager object. The namespace manager for the Nintex Forms XML.</param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        private static void AddTextBoxProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText)
        {
            // Load TextBoxFormControlProperties XML.
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>TextBoxFormControlProperties.xml
            propsTextbox.Load(@"");

            //Create the XML Namangespace manager and load with the Nintex Form namespaces.
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");

            //Create the GUID used to links each pair of control complex elements.
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //Load the display name from the control text.
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //Load the name from the control text.
            var namePath = "/FormControls/d2p1:FormControlProperties/d2p1:Name";
            var nameNode = propsTextbox.DocumentElement.SelectSingleNode(namePath, controlnsmgr);
            nameNode.InnerText = controlText;

            //Import TextBoxFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Method to load the Label XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control
        ///     complex  elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr"></param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        private static void AddLabelProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText)
        {
            // Load LabelFormControlProperties XML
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>LabelFormControlProperties.xml
            propsTextbox.Load(@"");

            //Create the XML Namangespace manager and load with the Nintex Form namespaces.
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");

            //update the GUID
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //update the Display Name
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //update the Text
            var textPath = "/FormControls/d2p1:FormControlProperties/d2p1:Text";
            var textNode = propsTextbox.DocumentElement.SelectSingleNode(textPath, controlnsmgr);
            textNode.InnerText = controlText;

            //update the Name
            var namePath = "/FormControls/d2p1:FormControlProperties/d2p1:Name";
            var nameNode = propsTextbox.DocumentElement.SelectSingleNode(namePath, controlnsmgr);
            nameNode.InnerText = controlText;

            //Import LabelFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Method to load the YesNo (Boolean) XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control
        ///     complex  elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr">Namespace Manager object. The namespace manager for the Nintex Forms XML.</param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        private static void AddYesNoProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText)
        {
            // Load BooleanFormControlProperties XML.
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>BooleanFormControlProperties.xml
            propsTextbox.Load(@"");

            //Create the XML Namangespace manager and load with the Nintex Form namespaces.
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");

            //update the GUID.
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //Update the display name.
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //Import LabelFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Method to load the Button XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control
        ///     complex  elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr">Namespace Manager object. The namespace manager for the Nintex Forms XML.</param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        private static void AddButtonProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText)
        {
            // Load ButtonFormControlProperties XML
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>ButtonFormControlProperties.xml
            propsTextbox.Load(@"");

            //XMLManager
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");

            //update the GUID
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //update the Display Name
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //update the Text
            var textPath = "/FormControls/d2p1:FormControlProperties/d2p1:Text";
            var textNode = propsTextbox.DocumentElement.SelectSingleNode(textPath, controlnsmgr);
            textNode.InnerText = controlText;

            //Import LabelFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Method to load the ChoiceFormControlProperties (ListBox) XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control
        ///     complex  elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr">Namespace Manager object. The namespace manager for the Nintex Forms XML.</param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        /// <param name="choiceArray">Array. This contains the choice strings in the drop-down list.</param>
        private static void AddChoiceProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText, string[] choiceArray)
        {
            // Load ChoiceFormControlProperties XML.
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>ChoiceFormControlProperties.xml
            propsTextbox.Load(@"");

            //Create the XML Namangespace manager and load with the Nintex Form namespaces.
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");
            controlnsmgr.AddNamespace("d4p1", "http://schemas.microsoft.com/2003/10/Serialization/Arrays");

            //Update the GUID.
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //update the display name.
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //Update the choices.
            var choicesPath = "/FormControls/d2p1:FormControlProperties/d2p1:Choices";
            var choicesNode = propsTextbox.DocumentElement.SelectSingleNode(choicesPath, controlnsmgr);
            for (var c = 0; c < choiceArray.Length; c++)
            {
                var optionNode = propsTextbox.CreateNode("element", "d4p1:string",
                    "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
                optionNode.InnerText = choiceArray[c];
                choicesNode.AppendChild(optionNode);
            }

            //Import LabelFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Method to load the ImageFormControlProperties XML Fragment and update with the control Guid.
        /// </summary>
        /// <param name="formxml">
        ///     An XML document. The Nintex Forms XML file that does not contain the pair of control
        ///     complex  elements for the control properties and control layout.
        /// </param>
        /// <param name="formsnsmgr">Namespace Manager object. The namespace manager for the Nintex Forms XML.</param>
        /// <param name="iterguid">.NET GUID. The Guid links each pair of control complex elements.</param>
        /// <param name="controlText">
        ///     String. Each control in the input XML description document contains a string used
        ///     display name, label text, and so on.
        /// </param>
        /// <param name="image">The relative or absolute address of the image file (PNG, JPG, GIF).</param>
        private static void AddImageProperties(XmlDocument formxml, XmlNamespaceManager formsnsmgr, Guid iterguid,
            string controlText, string image)
        {
            // Load ImageFormControlProperties XML.
            var propsTextbox = new XmlDocument();
            // TODO: update path to: <filepathpath>ImageFormControlProperties.xml
            propsTextbox.Load(@"");

            //Create the XML Namangespace manager and load with the Nintex Form namespaces.
            var controlnsmgr = new XmlNamespaceManager(propsTextbox.NameTable);
            controlnsmgr.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
            controlnsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");

            //Update the GUID.
            var UniqueIdpath = "/FormControls/d2p1:FormControlProperties/d2p1:UniqueId";
            var selectSingleNode = propsTextbox.DocumentElement.SelectSingleNode(UniqueIdpath, controlnsmgr);
            selectSingleNode.InnerText = iterguid.ToString();

            //Update the display name.
            var disPath = "/FormControls/d2p1:FormControlProperties/d2p1:DisplayName";
            var disNode = propsTextbox.DocumentElement.SelectSingleNode(disPath, controlnsmgr);
            disNode.InnerText = controlText;

            //Update the Alt text.
            var altPath = "/FormControls/d2p1:FormControlProperties/d2p1:AlternateText";
            var altNode = propsTextbox.DocumentElement.SelectSingleNode(altPath, controlnsmgr);
            altNode.InnerText = controlText;

            //Update the Image URL.
            var imagePath = "/FormControls/d2p1:FormControlProperties/d2p1:ImageUrl";
            var imageNode = propsTextbox.DocumentElement.SelectSingleNode(imagePath, controlnsmgr);
            imageNode.InnerText = image;

            //Import LabelFormControlProperties into the original document.
            var newControl = formxml.ImportNode(propsTextbox.DocumentElement.LastChild, true);
            var proppath = "/n:Form/n:FormControls";
            var controlnode = formxml.DocumentElement.SelectSingleNode(proppath, formsnsmgr);
            controlnode.AppendChild(newControl);
        }

        /// <summary>
        ///     Serialize the FormControlLayout created for each control; Requires an XNode and the FormControlLayout
        ///     class attribute. The serializatoin adds a namespace to the root elemenet that is stripped from the file:
        ///     xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"\". The Form XML will not load without this being removed.
        /// </summary>
        /// <param name="parentNode">The parent node under which to attach the serialized object as a child.</param>
        /// <param name="obj">
        ///     The Form Control Layout Object from the FormControlLayout class specifes the attributes of the
        ///     contro layout: width, height, and x and y coordinates
        /// </param>
        private static void AddControlLayout(XmlNode parentNode, object obj)
        {
            var nav = parentNode.CreateNavigator();
            using (var writer = nav.AppendChild())
            {
                var serializer = new XmlSerializer(obj.GetType());
                writer.WriteWhitespace("");
                serializer.Serialize(writer, obj);
                writer.Close();
            }
        }
    }
}