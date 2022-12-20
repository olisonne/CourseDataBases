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
    public partial class BookingCancel : Form
    {
        private Hotel hotel = new Hotel();
        private CheckUser user_;
        private int rowStateIndex = 7;
        public BookingCancel(CheckUser user)
        {
            user_ = user;
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void BookingCancel_Load(object sender, EventArgs e)
        {
            CreateColums();
            RefreshDataGrid(dataGridView1);
        }

        private void CreateColums()
        {
            dataGridView1.Columns.Add("ID_Checkin", "id"); //0
            dataGridView1.Columns.Add("Date_CheckIn", "checkin"); //1
            dataGridView1.Columns.Add("Date_Departure", "departure"); //2
            dataGridView1.Columns.Add("Payment", "payment"); //3
            dataGridView1.Columns.Add("ID_Room", "ID_Room"); //3
            dataGridView1.Columns.Add("ID_Client", "ID_Client");
            dataGridView1.Columns.Add("N_of_people", "N_of_people");
            dataGridView1.Columns.Add("IsNew", String.Empty);
        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0),
                record.GetDateTime(1),
                record.GetDateTime(2),
                record.GetInt32(3),
                record.GetInt32(4),
                record.GetInt32(5),
                record.GetInt32(6),
                RowState.ModifiedNew);
        }


        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"Select * from Check_in";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());

            hotel.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }
            reader.Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            int index = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows[index].Visible = false;
            dataGridView1.Rows[index].Cells[rowStateIndex].Value = RowState.Deleted;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                var rowState = dataGridView1.Rows[i].Cells[rowStateIndex].Value;
                switch (rowState)
                {
                    case RowState.Deleted:
                        {
                            int id = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value);
                            string deleteQuery = $"delete from Check_in where ID_Checkin = {id}";
                            string deleteServiceQuery = $"delete from GetService where ID_Checkin = {id}";
                            SqlCommand command = new SqlCommand(deleteQuery, hotel.GetConnection());
                            SqlCommand commandService = new SqlCommand(deleteServiceQuery, hotel.GetConnection());
                            hotel.openConnection();
                            commandService.ExecuteNonQuery();
                            command.ExecuteNonQuery();
                            break;
                        }
                    case RowState.Existed:
                        {
                            continue;
                        }
                }
                
            }
            MessageBox.Show("Information saved");
            hotel.closeConnection();
            
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
