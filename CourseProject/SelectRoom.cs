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
    public partial class SelectRoom : Form
    {
        private Hotel hotel = new Hotel();
        private int selectedRow;
        private CheckUser user_;
        public SelectRoom(CheckUser user)
        {
            user_ = user;
            InitializeComponent();
        }

        private void Booking_Load(object sender, EventArgs e)
        {
            CreateColums();
            RefreshDataGrid(dataGridView1);
            dataGridView1.Enabled = false;
        }

        private void CreateColums()
        {
            dataGridView1.Columns.Add("ID_Room", "id"); //0
            dataGridView1.Columns.Add("Floor", "floor"); //1
            dataGridView1.Columns.Add("Phone", "phone"); //2
            dataGridView1.Columns.Add("Cost", "cost"); //3
            dataGridView1.Columns.Add("Type", "type"); //3
            dataGridView1.Columns.Add("Places", "places");
            dataGridView1.Columns.Add("FreePlaces", "free_places");
        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0),
                record.GetInt32(1),
                record.GetString(2),
                record.GetInt32(3),
                record.GetString(4),
                record.GetInt32(6), 
                record.GetInt32(7));
        }


        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"Select *, case when Type = 'Multi_bed' then(select TotalN_of_Bed from Multi_bed where ID_Room = Room.ID_Room) " +
                $"else (Select TotalN_of_Places from Suite_JuniorSuite where ID_Room = Room.ID_Room) end, " +
                $"case when Type = 'Multi_bed' then(select TotalN_of_Bed from Multi_bed where ID_Room = Room.ID_Room) else (Select TotalN_of_Places from Suite_JuniorSuite where ID_Room = Room.ID_Room)  end from Room";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());

            hotel.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }
            reader.Close();
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            dataGridView1.Enabled = true;
            Search(dataGridView1);
        }

        private void Search(DataGridView dgw)
        {
            if(comboBox_Type.Text == "")
            {
                MessageBox.Show("Choose room type!");
                return;
            }
            dgw.Rows.Clear();
            SqlParameter type = new SqlParameter("@type", comboBox_Type.SelectedItem.ToString());
            SqlParameter checkin = new SqlParameter("@checkin", dateTimePicker1.Value);
            SqlParameter departure = new SqlParameter("@departure", dateTimePicker2.Value);

            string searchString;

            if (comboBox_Type.SelectedItem.ToString() == "Multi_bed"){

                searchString = $"Select *, (select TotalN_of_Bed from Multi_bed where ID_Room = Room.ID_Room), COALESCE((select(TotalN_of_Bed - count(*)) from(Select Multi_bed.ID_Room as ID, TotalN_of_Bed From Check_in inner join Multi_bed on Multi_bed.ID_Room = Check_in.ID_Room where ID_Checkin not in (Select ID_Checkin from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room " +
                    $"where Date_Departure < @checkin or Date_CheckIn > @departure)) as T group by ID, TotalN_of_Bed), (select TotalN_of_Bed from Multi_bed where Multi_bed.ID_Room = Room.ID_Room)) as FreePlaces from Room " +
                    $"where Type = 'Multi_bed' and ID_Room not in (select ID from(Select Multi_bed.ID_Room as ID, TotalN_of_Bed From Check_in inner join Multi_bed on Multi_bed.ID_Room = Check_in.ID_Room where ID_Checkin not in (Select ID_Checkin from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room where Date_Departure < @checkin or Date_CheckIn > @departure)) as T group by ID, TotalN_of_Bed having(TotalN_of_Bed - count(*)) <= 0)";
            }
            else
            {
                searchString = $"select *, (select TotalN_of_Places from Suite_JuniorSuite where ID_Room = Room.ID_Room), " +
                    $"(select TotalN_of_Places from Suite_JuniorSuite where ID_Room = Room.ID_Room) from Room where Type = @type and ID_Room not in(Select ID_Room From Check_in where ID_Checkin not in " +
                    $"(Select ID_Checkin from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room where Date_Departure<@checkin or Date_CheckIn > @departure))";
            }
            
            
            SqlCommand command = new SqlCommand(searchString, hotel.GetConnection());
            command.Parameters.Add(type);
            command.Parameters.Add(checkin);
            command.Parameters.Add(departure);
            hotel.openConnection();
            SqlDataReader read = command.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRow(dgw, read);
            }

            read.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBox_RoomNumber.Text = row.Cells[0].Value.ToString();
                pictureBox1.Image = GetImage(int.Parse(row.Cells[0].Value.ToString()));
            }
               
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

        private void btn_Select_Click(object sender, EventArgs e)
        {
            if(dateTimePicker2.Value < dateTimePicker1.Value)
            {
                MessageBox.Show("Invalid date. Choose another one!");
                return;
            }
            if(comboBox_Type.Text == "")
            {
                MessageBox.Show("Invalid type. Choose another one!");
                return;
            }
            if(textBox_RoomNumber.Text == "")
            {
                MessageBox.Show("Invalid room. Choose another one!");
                return;
            }
            Booking booking = new Booking(int.Parse(textBox_RoomNumber.Text), dateTimePicker1.Value, dateTimePicker2.Value, comboBox_Type.Text, user_);
            this.Hide();
            booking.Show();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            MainMenu main = new MainMenu(user_);
            this.Hide();
            main.Show();
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            comboBox_Type.SelectedIndex = -1;
        }
    }
}
