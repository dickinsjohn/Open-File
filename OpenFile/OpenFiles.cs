using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace OpenFile
{
    public partial class OpenFiles : Form
    {
        List<string> linkedFiles = new List<string>();
        string fileLocation = null;
        string directory = null;

        public OpenFiles()
        {
            InitializeComponent();
        }

        public void ShowLinkedData(List<String> tempList, string tempDirectory)
        {
            directory = tempDirectory;

            linkedFiles = tempList;
            if (linkedFiles.Count != 0)
            {
                // Convert to DataTable.
                DataTable table = ConvertListToDataTable(linkedFiles);
                dataGridView1.DataSource = table;
            }
        }

        static DataTable ConvertListToDataTable(List<string> list)
        {
            // New table.
            DataTable table = new DataTable();

            List<string> correctedString = new List<string>();
            correctedString.Clear();

            table.Columns.Add();

            //remove Linked Files from the file list
            foreach (string str in list)
            {
                correctedString.Add(str.Replace("Linked Files",""));
            }

            // Add rows.
            foreach (string str in correctedString)
            {
                table.Rows.Add(str);
            }
            return table;
        }

        private void GridClickEvent(object sender, DataGridViewCellEventArgs e)
        {            
                textBox1.Text = dataGridView1.CurrentCell.Value.ToString();
                fileLocation = directory.ToString() + textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(fileLocation);
            }
            catch
            {
                MessageBox.Show("Failed to Open File!");
            }
            
        }
    }
}
