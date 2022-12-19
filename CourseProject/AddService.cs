using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseProject
{
    public partial class AddService : Form
    {
        private CheckUser user_;
        private Hotel hotel = new Hotel();
        private int rowStateIndex = 3;
        private int selectedRow;
        public AddService(CheckUser user)
        {
            user_ = user;
            InitializeComponent();
        }

        private void AddService_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshDataGrid(dataGridView1);
        }

        private void CreateColumns()
        {
            dataGridView1.Columns.Add("Id_Service", "id");
            dataGridView1.Columns.Add("Price", "price");
            dataGridView1.Columns.Add("NameService", "name");
            dataGridView1.Columns.Add("IsNew", String.Empty);
        }

        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();
            string queryString = $"select * from Service";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());

            hotel.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }
            reader.Close();
        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0),
                record.GetInt32(1),
                record.GetString(2),
                RowState.ModifiedNew);
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            MainMenu mainMenu = new MainMenu(user_);
            this.Hide();
            mainMenu.Show();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string searchString = $"select * from Service where NameService like '%" + textBox_Search.Text + "%'";

            SqlCommand command = new SqlCommand(searchString, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader read = command.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRow(dgw, read);
            }

            read.Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            ClearField();
        }

        private void ClearField()
        {
            textBox_Name.Text = "";
            textBox_Price.Text = "";
        }
        
        private bool CheckInfo()
        {
            if (textBox_Name.Text == "" || textBox_Price.Text == "")
            {
                MessageBox.Show("Information isn't complete");
                return true;
            }
            if (!int.TryParse(textBox_Price.Text, out int n))
            {
                MessageBox.Show("Invalid Info");
                return true;
            }
            return false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(CheckInfo())
            {
                return;
            }

            SqlParameter name = new SqlParameter("@name", textBox_Name.Text);
            SqlParameter price = new SqlParameter("@price", textBox_Price.Text);

            string queryString = $"insert into Service values(@price, @name)";

            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(queryString, hotel.GetConnection());
            cmd.Parameters.Add(name);
            cmd.Parameters.Add(price);

            cmd.ExecuteNonQuery();
            hotel.closeConnection();
            MessageBox.Show("New service has been added");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            deleteRow();
        }

        private void deleteRow()
        {
            int index = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows[index].Visible = false;
            dataGridView1.Rows[index].Cells[rowStateIndex].Value = RowState.Deleted;
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            Change();
        }

        private void Change()
        {
            if (CheckInfo())
            {
                return;
            }
            int selectedRowIndex = dataGridView1.CurrentCell.RowIndex;
            string id_s = dataGridView1.Rows[selectedRow].Cells[0].Value.ToString();
            int id = int.Parse(id_s);
            string name = textBox_Name.Text;
            string price = textBox_Price.Text;

            dataGridView1.Rows[selectedRowIndex].SetValues(id, price, name);
            dataGridView1.Rows[selectedRowIndex].Cells[rowStateIndex].Value = RowState.Modified;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            update();
        }
        private void update()
        {
            hotel.openConnection();

            for (int i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                var rowState = dataGridView1.Rows[i].Cells[rowStateIndex].Value;

                switch (rowState)
                {
                    case RowState.Existed:
                        continue;
                    case RowState.Deleted:
                        {
                            int id = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value);
                            string deleteQuery = $"delete from Service where ID_Service = {id}";
                            SqlCommand command = new SqlCommand(deleteQuery, hotel.GetConnection());
                            hotel.openConnection();
                            command.ExecuteNonQuery();
                            break;
                        }

                    case RowState.Modified:
                        {
                            string id = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            SqlParameter name = new SqlParameter("@name", dataGridView1.Rows[i].Cells[2].Value.ToString());
                            SqlParameter price = new SqlParameter("@price", dataGridView1.Rows[i].Cells[1].Value.ToString());


                            string changeQuery = $"update Service set Price = @price, " +
                                $"NameService = @name " +
                                $"where ID_Service = {id};";

                            SqlCommand command = new SqlCommand(changeQuery, hotel.GetConnection());
                            command.Parameters.Add(name);
                            command.Parameters.Add(price);
                            hotel.openConnection();
                            command.ExecuteNonQuery();

                            break;
                        }

                }
               
            }
            ClearField();
            hotel.closeConnection();
            MessageBox.Show("Information saved");
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBox_Name.Text = row.Cells[2].Value.ToString();
                textBox_Price.Text = row.Cells[1].Value.ToString();
            }
        }
    }

}
