# GenerateNintexFormXML

The Nintex Simple Form Builder (GenerateFormXML) is a console application that will convert a basic XML file into a Nintex Form.
I wrote this as part of the Nintex Workflow Plateform SDK (for Forms 2013). You can find the documentation here: http://bit.ly/2qwBr35

The application uses the XmlDocument document class from the .Net Framework to assemble XML control fragments and insert them into a blank desktop layout.

As input the application uses a paired down XML file that lists the controls, specifies the control dimensions, and key values for each type of control. This application is limited to the following form elements:

- Label control
- TextBox control
- ListBox control
- Checkbox control
- Image control
T
he sample demonstrates how to take input (in this case a bare-bones XML file), to create Nintex XML elements that describe a Nintex form.

For example, you may need to convert a large number of HTML form files to Nintex Forms. A possible workflow would perform a transformation on the HTML form to the bare-bones XML used by this application, generate Form XML, then load the form into a SharePoint list, and bind the input fields to SharePoint List columns.
