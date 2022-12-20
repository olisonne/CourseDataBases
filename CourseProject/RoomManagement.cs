using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseProject
{

    public partial class RoomManagement : Form
    {
        private Hotel hotel = new Hotel();
        private int selectedRow;
        private int rowStateIndex = 6;
        private CheckUser user_;
        public RoomManagement(CheckUser user)
        {
            user_ = user;
            InitializeComponent();
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            AddRoom addRoom = new AddRoom(user_);
            addRoom.Show();
        }

        private void CreateColums()
        {
            dataGridView1.Columns.Add("ID_Room", "id"); //0
            dataGridView1.Columns.Add("Floor", "floor"); //1
            dataGridView1.Columns.Add("Phone", "phone"); //2
            dataGridView1.Columns.Add("Cost", "cost"); //3
            dataGridView1.Columns.Add("Type", "type"); //3
            dataGridView1.Columns.Add("Places", "places");
            dataGridView1.Columns.Add("IsNew", String.Empty);
        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0),
                record.GetInt32(1),
                record.GetString(2),
                record.GetInt32(3),
                record.GetString(4),
                record.GetInt32(6),
                RowState.ModifiedNew);
        }

        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"Select *, case when Type = 'Multi_bed' " +
                $"then(select TotalN_of_Bed from Multi_bed where ID_Room = Room.ID_Room) " +
                $"else (Select TotalN_of_Places from Suite_JuniorSuite where ID_Room = Room.ID_Room) " +
                $"end from Room";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());

            hotel.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }
            reader.Close();
        }

        private void AddRoom_Load(object sender, EventArgs e)
        {
            CreateColums();
            RefreshDataGrid(dataGridView1);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBox_Number.Text = row.Cells[0].Value.ToString();
                textBox_Floor.Text = row.Cells[1].Value.ToString();
                textBox_Phone.Text = row.Cells[2].Value.ToString();
                textBox_Cost.Text = row.Cells[3].Value.ToString();
                textBox_Type.Text = row.Cells[4].Value.ToString();
                pictureBox1.Image = null;
                pictureBox1.Image = GetImage(int.Parse(textBox_Number.Text));

                if(textBox_Type.Text == "Suite" || textBox_Type.Text == "Junior_Suite")
                {
                    GetMoreInfo(true, int.Parse(textBox_Number.Text));
                }
                else
                {
                    GetMoreInfo(false, int.Parse(textBox_Number.Text));
                }
            }
        }

        private void GetMoreInfo(bool isSuite, int ID)
        {
            textBox_Rooms.Text = "";
            textBox_Places.Text = "";
            textBox_Living.Text = "";
            textBox_Beds.Text = "";
            string query = null;
            hotel.openConnection();
            
            

            if (isSuite)
            {
                query = $"select N_of_Room, TotalN_of_Places from Suite_JuniorSuite where ID_Room = {ID}";
                SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
                SqlDataReader dataReader = cmd.ExecuteReader();
                dataReader.Read();
                if (dataReader.HasRows)
                {
                    textBox_Rooms.Text = dataReader[0].ToString();
                    textBox_Places.Text = dataReader[1].ToString();
                    
                }
                dataReader.Close();
            }
            else
            {
                query = $"select TotalN_of_Bed from Multi_bed where ID_Room = {ID}";
                SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
                SqlDataReader dataReader = cmd.ExecuteReader();
                dataReader.Read();
                if (dataReader.HasRows)
                {
                    textBox_Beds.Text = dataReader[0].ToString();
                
                }
                dataReader.Close();
            }
            
            hotel.closeConnection();

        }

        private Image GetImage(int ID)
        {
            string imgQuery = $"select Image from Room where ID_Room = {ID}";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(imgQuery, hotel.GetConnection());
            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            byte[] img;
            if (dataReader.HasRows)
            {
                img = (byte[])dataReader[0];
                if (img.Length > 0)
                {
                    dataReader.Close();
                    MemoryStream ms = new MemoryStream(img);
                    hotel.closeConnection();
                    return Image.FromStream(ms);
                }
            }
            dataReader.Close();
            hotel.closeConnection();
            return null;
            
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            ClearField();
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();
            string searchString = $"select * from Room where Type like '%" + textBox4.Text + "%'";

            SqlCommand command = new SqlCommand(searchString, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader read = command.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRow(dgw, read);
            }

            read.Close();
        }
        private void ClearField()
        {
            textBox_Beds.Text = "";
            textBox_Cost.Text = "";
            textBox_Floor.Text = "";
            textBox_Living.Text = "";
            textBox_Number.Text = "";
            textBox_Phone.Text = "";
            textBox_Places.Text = "";
            textBox_Rooms.Text = "";
            textBox_Type.Text = "";
            textBox4.Text = "";
        }

        private void deleteRow()
        {
            int index = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows[index].Visible = false;
            dataGridView1.Rows[index].Cells[rowStateIndex].Value = RowState.Deleted;
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            deleteRow();
        }

        private void update()
        {
            hotel.openConnection();
            for(int i = 0; i< dataGridView1.Rows.Count; ++i)
            {
                var rowState = dataGridView1.Rows[i].Cells[rowStateIndex].Value;
                
                switch(rowState)
                {
                    case RowState.Existed:
                        continue;
                    case RowState.Deleted:
                        {
                            int id = Convert.ToInt32(dataGridView1.Rows[i].Cells[0].Value);
                            string type = Convert.ToString(dataGridView1.Rows[i].Cells[4].Value);
                            string deleteSubQuery;
                            string deleteQuery = $"delete from Room where ID_Room = {id}";
                            string deleteService = $"delete from GetService where ID_Checkin in (Select ID_Checkin from Check_in where ID_Room = {id})";
                            string deleteCheckin = $"delete from Check_in where ID_Room = {id}";

                            SqlCommand cmdService = new SqlCommand(deleteService, hotel.GetConnection());
                            SqlCommand cmdCheckIn = new SqlCommand(deleteCheckin, hotel.GetConnection());
                            cmdService.ExecuteNonQuery();
                            cmdCheckIn.ExecuteNonQuery();

                            if(type == "Multi_bed")
                            {
                                deleteSubQuery = $"delete from Multi_bed where ID_Room = {id}"; 
                            }
                            else
                            {
                                deleteSubQuery = $"delete from Suite_JuniorSuite where ID_Room = {id}";
                            }
                            SqlCommand cmdQuery = new SqlCommand(deleteQuery, hotel.GetConnection());
                            SqlCommand cmdSubQuery = new SqlCommand(deleteSubQuery, hotel.GetConnection());
                            cmdSubQuery.ExecuteNonQuery();
                            cmdQuery.ExecuteNonQuery();
                            
                            break;
                        }
                    case RowState.Modified:
                        {
                            string id = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            SqlParameter floor = new SqlParameter("@floor", dataGridView1.Rows[i].Cells[1].Value.ToString());
                            SqlParameter phone = new SqlParameter("@phone", dataGridView1.Rows[i].Cells[2].Value.ToString());
                            SqlParameter cost = new SqlParameter("@cost", dataGridView1.Rows[i].Cells[3].Value.ToString());

                            string changeQuery = $"update Room set " +
                                $"Floor = @floor, " +
                                $"Phone = @phone, " +
                                $"Cost = @cost " +
                                $"where ID_Room = {id};";

                            SqlCommand command = new SqlCommand(changeQuery, hotel.GetConnection());

                            command.Parameters.Add(floor);
                            command.Parameters.Add(phone);
                            command.Parameters.Add(cost);

                            command.ExecuteNonQuery();
                            break;
                        }
                }
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            update();
        }

        private void btn_Change_Click(object sender, EventArgs e)
        {
            Change();   
        }

        private bool CheckInfoType()
        {
            if(!int.TryParse(textBox_Floor.Text, out int n) ||
                 !int.TryParse(textBox_Cost.Text, out int p))
            {
                return true;
            }
            return false;
        }

        private void Change()
        {
            if(CheckInfoType())
            {
                MessageBox.Show("Invalid info");
                return;
            }
            int selectedRowIndex = dataGridView1.CurrentCell.RowIndex;
            string id_s = dataGridView1.Rows[selectedRow].Cells[0].Value.ToString();
            int id = int.Parse(id_s);
            string floor = textBox_Floor.Text;
            string type = textBox_Type.Text;
            string cost = textBox_Cost.Text;
            string phone = textBox_Phone.Text;
            string places;
            if (type == "Multi_bed")
            {
                places = textBox_Beds.Text;
            }
            else
            {
                places = textBox_Places.Text;
            }

            dataGridView1.Rows[selectedRow].SetValues(id, floor, phone, cost, type, places);
            dataGridView1.Rows[selectedRowIndex].Cells[rowStateIndex].Value = RowState.Modified;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            MainMenu mainMenu = new MainMenu(user_);
            this.Hide();
            mainMenu.Show();
        }
    }

}
