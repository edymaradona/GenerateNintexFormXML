using System;
using System.Xml.Serialization;

namespace _2013_ONPREM_NF_GenerateFormXML
{
    /// <summary>
    ///     Class to generate a serialized Form Control Layout for each control.
    /// </summary>
    [XmlRoot("FormControlLayout")]
    public class FormControlLayout
    {
        public string FormControlLayouts;
        public Guid FormControlUniqueId;
        public bool FromTemplate;
        public int Height;
        public int Left;
        public int Top;
        public int Width;
        public int ZIndex;

        public FormControlLayout()
        {
            FormControlLayouts = "";
            FormControlUniqueId = Guid.Empty;
            FromTemplate = false;
            Height = 0;
            Left = 0;
            Top = 0;
            Width = 0;
            ZIndex = 100;
        }
    }
}