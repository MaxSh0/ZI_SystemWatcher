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
using System.Security.Permissions;
using System.Data.SqlClient;

namespace ZI_SystemWatcher
{
    public partial class Form1 : Form
    {
        List<FileSystemWatcher> Wachers = new List<FileSystemWatcher>();
        public Form1()
        {
            InitializeComponent();
        }

        private void AddFound_Click(object sender, EventArgs e)
        {
            labelError.Visible = false;
            try 
            {
                Directory.EnumerateFiles(textBox1.Text);
                dataGridView1.Rows.Add(textBox1.Text, true);
                FileSystemWatcher FSW = new FileSystemWatcher();
                FSW.Path = textBox1.Text;

                FSW.Changed += fileSystemWatcher1_Changed;
                FSW.Deleted += fileSystemWatcher1_Deleted;
                FSW.Renamed += fileSystemWatcher1_Renamed;
                FSW.Created += fileSystemWatcher1_Created;

                FSW.IncludeSubdirectories = true;
                FSW.EnableRaisingEvents = true;
                Wachers.Add(FSW);

                SqlCommand command = new SqlCommand("INSERT INTO [Folders](Path)VALUES(@Path)", Connect);
                command.Parameters.AddWithValue("Path", textBox1.Text);

                textBox1.Clear();
                
                
            }
            catch 
            {
                labelError.Visible = true;
            }
            

        }



      

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " удален" + "\r\n";
            });   
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " cоздан" + "\r\n";
            });
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " изменен" + "\r\n";
            });
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " переименован" + "\r\n";
            });
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty) 
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);   
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (Wachers.Count > 0)
            {
                Wachers[e.RowIndex].EnableRaisingEvents = !Wachers[e.RowIndex].EnableRaisingEvents;
            }
        }
    }
}

