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
using System.Windows.Forms.DataVisualization.Charting;

namespace CourseProject
{
    public partial class Statistics : Form
    {
        private Hotel hotel = new Hotel();
        public Statistics()
        {
            InitializeComponent();
        }


        private void GetMaxIncomePerMonth()
        {
            string query = $"select distinct month(Date_CheckIn) as Month, sum(Payment) over(partition by month(Date_CheckIn)) from Check_in";
            GetInfoFromDataBase(query);

        }

        private void GetInfoFromDataBase(string query)
        {
            SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader reader = cmd.ExecuteReader();
            Dictionary<string, int> data = new Dictionary<string, int>();
            while (reader.Read())
            {
                data.Add(reader[0].ToString(), (int)reader[1]);
            }
            reader.Close();
            hotel.closeConnection();

            chart1.Series["Data"].Points.DataBindXY(data.Keys, data.Values);
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, count(*) over(partition by Room.ID_Room) from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room where Type = 'Suite'";
            GetInfoFromDataBase(query);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, count(*) over(partition by Room.ID_Room) from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room where Type = 'Junior_Suite'";
            GetInfoFromDataBase(query);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, count(*) over(partition by Room.ID_Room) from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room where Type = 'Multi_bed'";
            GetInfoFromDataBase(query);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, case when Type = 'Suite' or Type = 'Junior_Suite' then count(*) over(partition by Room.ID_Room) else count(*) over(partition by Room.ID_Room) / (Select TotalN_of_Bed from Multi_bed where Multi_bed.ID_Room = Room.ID_Room) " +
                $"end from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room";
            GetInfoFromDataBase(query);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, sum(Payment) over(partition by Room.ID_Room) from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room";
            GetInfoFromDataBase(query);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string query = $"select distinct Room.ID_Room, avg(Payment) over(partition by Room.ID_Room) from Check_in inner join Room on Room.ID_Room = Check_in.ID_Room";
            GetInfoFromDataBase(query);
        }

        private void btn_Services_Click(object sender, EventArgs e)
        {
            string query = $"select distinct NameService, count(*) over(partition by GetService.ID_Service) from GetService inner join Service on Service.ID_Service = GetService.ID_Service";
            GetInfoFromDataBase(query);
        }
    }
}
