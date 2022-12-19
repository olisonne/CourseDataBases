using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CourseProject
{
    enum RowState
    {
        Existed,
        New,
        Modified,
        ModifiedNew,
        Deleted
    }
    public partial class AddClient : Form
    {
        private Hotel hotel = new Hotel();
        private int selectedRow;
        private int rowStateIndex = 8;
        CheckUser _user;
        public AddClient(CheckUser user)
        {
            _user = user;
            InitializeComponent();
        }

        private void CreateColumnsClient()
        {
            dataGridView1.Columns.Add("ID_Client", "id");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("Address", "Address");
            dataGridView1.Columns.Add("Sex", "Sex");
            dataGridView1.Columns.Add("Date_Birth", "Birth date");
            dataGridView1.Columns.Add("Document_Type", "Document type");
            dataGridView1.Columns.Add("Document_Number", "Document number");
            dataGridView1.Columns.Add("Phone", "Phone");
            dataGridView1.Columns.Add("IsNew", String.Empty);
        }

        private void IsAdmin()
        {
            btnDelete.Enabled = _user.IsAdmin;
            btnDelete.Visible = _user.IsAdmin;
            btnChange.Enabled = _user.IsAdmin;
            btnChange.Visible = _user.IsAdmin;
            btnSave.Enabled = _user.IsAdmin;
            btnSave.Visible = _user.IsAdmin;
        }

        private void ReadSingleRowClient(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0),
                record.GetString(1),
                record.GetString(2),
                record.GetString(3),
                record.GetDateTime(4),
                record.GetString(5),
                record.GetString(6),
                record.GetString(7),
                RowState.ModifiedNew);
        }

        private void RefreshDataGridClient(DataGridView dgw)
        {
            dgw.Rows.Clear();
            string queryString = $"select * from Client";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());

            hotel.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRowClient(dgw, reader);
            }
            reader.Close();
        }


        private void FAddClient_Load_1(object sender, EventArgs e)
        {
            CreateColumnsClient();
            IsAdmin();
            RefreshDataGridClient(dataGridView1);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBox_Name.Text = row.Cells[1].Value.ToString();
                textBox_Address.Text = row.Cells[2].Value.ToString();
                comboBox_Sex.SelectedIndex = comboBox_Sex.FindString(row.Cells[3].Value.ToString());
                dateTimePicker_BirthDate.Value = DateTime.Parse(row.Cells[4].Value.ToString());
                comboBox_DocumentType.SelectedIndex = comboBox_DocumentType.FindString(row.Cells[5].Value.ToString());
                textBox_DocumentNumber.Text = row.Cells[6].Value.ToString();
                textBox_Phone.Text = row.Cells[7].Value.ToString();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDataGridClient(dataGridView1);
            ClearField();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(textBox_Name.Text == "" || textBox_Address.Text == "" || comboBox_DocumentType.Text == "" || 
                textBox_DocumentNumber.Text == "" || textBox_Phone.Text == "" || comboBox_Sex.Text == "")
            {
                MessageBox.Show("Information not complete");
                return;
            }
            SqlParameter nameP = new SqlParameter("@name", textBox_Name.Text);
            SqlParameter bithDateP = new SqlParameter("@bDate", dateTimePicker_BirthDate.Value);
            SqlParameter addressP = new SqlParameter("@address", textBox_Address.Text);
            SqlParameter documentTypeP = new SqlParameter("@docType", comboBox_DocumentType.Text);
            SqlParameter documentNP = new SqlParameter("@docNum", textBox_DocumentNumber.Text);
            SqlParameter phoneP = new SqlParameter("@phone", textBox_Phone.Text);
            SqlParameter sexP = new SqlParameter("@sex", comboBox_Sex.Text);

            string addQuery = $"insert into Client values (@name, @address, @sex, @bDate, @docType , @docNum, @phone)";

            SqlCommand command = new SqlCommand(addQuery, hotel.GetConnection());
            command.Parameters.Add(nameP);
            command.Parameters.Add(bithDateP);
            command.Parameters.Add(addressP);
            command.Parameters.Add(documentTypeP);
            command.Parameters.Add(documentNP);
            command.Parameters.Add(phoneP);
            command.Parameters.Add(sexP);

            hotel.openConnection();
            command.ExecuteNonQuery();

            MessageBox.Show("New client has been added");

            hotel.closeConnection();
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string searchString = $"select * from Client where name like '%" + textBox_Search.Text + "%'";

            SqlCommand command = new SqlCommand(searchString, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader read = command.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRowClient(dgw, read);
            }

            read.Close();
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void deleteRow()
        {
            int index = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows[index].Visible = false;
            dataGridView1.Rows[index].Cells[rowStateIndex].Value = RowState.Deleted;
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
                            string deleteQuery = $"delete from Client where ID_Client = {id}";
                            SqlCommand command = new SqlCommand(deleteQuery, hotel.GetConnection());
                            command.ExecuteNonQuery();
                            break;
                        }

                    case RowState.Modified:
                        {
                            string id = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            SqlParameter nameP = new SqlParameter("@name", dataGridView1.Rows[i].Cells[1].Value.ToString());
                            SqlParameter bithDateP = new SqlParameter("@bDate", Convert.ToDateTime(dataGridView1.Rows[i].Cells[4].Value.ToString()));
                            SqlParameter addressP = new SqlParameter("@address", dataGridView1.Rows[i].Cells[2].Value.ToString());
                            SqlParameter documentTypeP = new SqlParameter("@docType", dataGridView1.Rows[i].Cells[5].Value.ToString());
                            SqlParameter documentNP = new SqlParameter("@docNum", dataGridView1.Rows[i].Cells[6].Value.ToString());
                            SqlParameter phoneP = new SqlParameter("@phone", dataGridView1.Rows[i].Cells[7].Value.ToString());
                            SqlParameter sexP = new SqlParameter("@sex", dataGridView1.Rows[i].Cells[3].Value.ToString());

                            string changeQuery = $"update Client set Name = @name," +
                                $"Address = @address," +
                                $"Sex = @sex," +
                                $"Date_Birth = @bDate," +
                                $"Document_Type = @docType," +
                                $"Document_Number = @docNum," +
                                $"Phone = @phone " +
                                $"where ID_Client = {id};";

                            SqlCommand command = new SqlCommand(changeQuery, hotel.GetConnection());
                            command.Parameters.Add(nameP);
                            command.Parameters.Add(bithDateP);
                            command.Parameters.Add(addressP);
                            command.Parameters.Add(documentTypeP);
                            command.Parameters.Add(documentNP);
                            command.Parameters.Add(phoneP);
                            command.Parameters.Add(sexP);


                            command.ExecuteNonQuery();


                            break;
                        }

                }
            }
            
            ClearField();
            hotel.closeConnection();
            MessageBox.Show("Information saved");
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            deleteRow();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            update();
        }

        private void ClearField()
        {
            textBox_Name.Text = "";
            textBox_Address.Text = "";
            textBox_DocumentNumber.Text = "";
            textBox_Phone.Text = "";
            comboBox_DocumentType.SelectedIndex = -1;
            comboBox_Sex.SelectedIndex = -1;
            dateTimePicker_BirthDate.Value = DateTime.Now;
        }

        private void Change()
        {
  
            if(textBox_Name.Text == "" || textBox_DocumentNumber.Text == "")
            {
                MessageBox.Show("Information not complete");
                return;
            }
            int selectedRowIndex = dataGridView1.CurrentCell.RowIndex;
            string id_s = dataGridView1.Rows[selectedRow].Cells[0].Value.ToString();
            int id = int.Parse(id_s);
            string name = textBox_Name.Text;
            string address = textBox_Address.Text;
            string documentNumber = textBox_DocumentNumber.Text;
            string phone = textBox_Phone.Text;
            string birthDate = dateTimePicker_BirthDate.Value.ToString();
            string documentType = comboBox_DocumentType.Text;
            string sex = comboBox_Sex.Text;

            dataGridView1.Rows[selectedRowIndex].SetValues(id, name, address, sex, birthDate, documentType, documentNumber, phone);
            dataGridView1.Rows[selectedRowIndex].Cells[rowStateIndex].Value = RowState.Modified;
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            Change();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            MainMenu form = new MainMenu(_user);
            form.Show();
            this.Hide();

        }
    }
}
