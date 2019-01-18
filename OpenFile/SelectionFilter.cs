using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace OpenFile
{
    class SelectionFilter : ISelectionFilter
    {
        Document m_doc = null;
        //constructor
        public SelectionFilter(Document doc)
        {
            m_doc = doc;
        }

        //allow all elements to be selected
        public bool AllowElement(Element elem)
        {
            if (elem is IndependentTag||elem is AnnotationSymbol
                || elem is Dimension||elem is DetailCurve||elem is TextElement||elem is Grid)
                return false;
            return true;
        }

        //allow all references to be selected
        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
    }
}
