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
using MySql.Data.MySqlClient;

namespace ZI_SystemWatcher
{
    public partial class Form1 : Form
    {
        DB db = new DB();
        List<FileSystemWatcher> Wachers = new List<FileSystemWatcher>(); 
        string newLog = ""; // Переменная, которая хранит не записанный в лог текст 

        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }
        //Добавление папки
        private void AddFound_Click(object sender, EventArgs e)
        {
            labelError.Visible = false;
            try 
            {
                Directory.EnumerateFiles(textBox1.Text);
                dataGridView1.Rows.Add(textBox1.Text, true, true);
                FileSystemWatcher FSW = new FileSystemWatcher();
                FSW.Path = textBox1.Text;

                FSW.Changed += fileSystemWatcher1_Changed;
                FSW.Deleted += fileSystemWatcher1_Deleted;
                FSW.Renamed += fileSystemWatcher1_Renamed;
                FSW.Created += fileSystemWatcher1_Created;

                FSW.IncludeSubdirectories = true;
                FSW.EnableRaisingEvents = true;
                Wachers.Add(FSW);

                MySqlCommand command = new MySqlCommand("INSERT INTO `folders` (`Path`) VALUES (@Path)", db.getConnection());
                command.Parameters.Add("@Path", MySqlDbType.VarChar).Value = textBox1.Text;
                db.openConnection();

                if (command.ExecuteNonQuery() != 1) 
                {
                    MessageBox.Show("Ошибка добавления","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                db.closeConnection();


                textBox1.Clear();
                
                
            }
            catch 
            {
                labelError.Visible = true;
            }
            

        }



        //События обработчика 
        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " удален" + " "+ DateTime.Now.ToString() +  "\r\n";
                newLog += e.FullPath + " удален" + " " + DateTime.Now.ToString() + "\n";
            });   
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " cоздан" + " " + DateTime.Now.ToString() + "\r\n";
                newLog += e.FullPath + " cоздан" + " " + DateTime.Now.ToString() + "\n";
            });
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " изменен" + " " + DateTime.Now.ToString() + "\r\n";
                newLog += e.FullPath + " изменен" + " " + DateTime.Now.ToString() + "\r\n";
            });
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text += e.FullPath + " переименован" + " " + DateTime.Now.ToString() + "\r\n";
                newLog += e.FullPath + " переименован" + " " + DateTime.Now.ToString() + "\r\n";
            });
        }



        //Обработка событий таблицы
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty) 
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);   
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                if (Wachers.Count > 0)
                {
                    Wachers[e.RowIndex].EnableRaisingEvents = !Wachers[e.RowIndex].EnableRaisingEvents;
                }
            }
            else if(e.ColumnIndex == 2) 
            {
                if (Wachers.Count > 0) 
                {
                    Wachers[e.RowIndex].IncludeSubdirectories = !Wachers[e.RowIndex].IncludeSubdirectories;
                }
            }


        }



        //Загрузка данных в форму
        private void Form1_Load(object sender, EventArgs e)
        {

            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand command = new MySqlCommand("SELECT * FROM `folders`", db.getConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table);

            for(int i = 1; i<table.Rows.Count; i++) 
            {
                dataGridView1.Rows.Add(table.Rows[i][0], true,true);
                FileSystemWatcher FSW = new FileSystemWatcher();
                FSW.Path = table.Rows[i][0].ToString();

                FSW.Changed += fileSystemWatcher1_Changed;
                FSW.Deleted += fileSystemWatcher1_Deleted;
                FSW.Renamed += fileSystemWatcher1_Renamed;
                FSW.Created += fileSystemWatcher1_Created;

                FSW.IncludeSubdirectories = true;
                FSW.EnableRaisingEvents = true;
                Wachers.Add(FSW);
            }
        }


        //Сохранение логов по кнопке
        private void ButtonLog_Click(object sender, EventArgs e)
        {
            FileStream Dic = new FileStream("log.txt", FileMode.Append);

            using (StreamWriter outputFile = new StreamWriter(Dic, Encoding.Default)) 
            {
                if (newLog.Length > 0)
                {
                    outputFile.WriteLine(newLog);
                    newLog = "";
                }
            }
        }

        //Сохранение логов по таймеру
        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox2.Text += "Лог сохранен\r\n";

            FileStream Dic = new FileStream("log.txt", FileMode.Append);

            using (StreamWriter outputFile = new StreamWriter(Dic, Encoding.Default))
            {
                if (newLog.Length > 0)
                {
                    outputFile.WriteLine(newLog);
                    newLog = "";
                }
            }
        }

        //Удаление записей
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите удалить запись", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Wachers[e.RowIndex].Dispose();
                    Wachers.RemoveAt(e.RowIndex);

                    MySqlCommand command = new MySqlCommand("DELETE FROM `folders` WHERE Path = @Path", db.getConnection());
                    command.Parameters.Add("@Path", MySqlDbType.VarChar).Value = dataGridView1[0, e.RowIndex].Value.ToString();
                    db.openConnection();

                    if (command.ExecuteNonQuery() != 1)
                    {
                        MessageBox.Show("Ошибка удаления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                    }
                    db.closeConnection();
                }
            }
        }
    }
}

