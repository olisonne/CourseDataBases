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
    public partial class Booking : Form
    {

        private Hotel hotel = new Hotel();
        private int roomNumber;
        private DateTime checkin;
        private DateTime departure;
        private string roomType;
        private int selectedRow;
        private CheckUser user_;
        public Booking(int roomNumber_, DateTime checkin_, DateTime departure_, string roomTypes_, CheckUser user)
        {
            checkin = checkin_;
            departure = departure_;
            roomType = roomTypes_;
            roomNumber = roomNumber_;
            user_ = user;
            InitializeComponent();
        }

        private void Booking_Load(object sender, EventArgs e)
        {
            CreateColumnsClient();
            RefreshDataGridClient(dataGridView1);
            CreateColumnsServices();
            textBox_RoomNumber.Text = roomNumber.ToString();
            dateTimePicker2.Enabled = false;
            dateTimePicker1.Enabled = false;
            textBox_RoomNumber.ReadOnly = true;
            textBox_ClientNumber.ReadOnly = true;
            dateTimePicker1.Value = checkin;
            dateTimePicker2.Value = departure;

            if(roomType == "Multi_bed")
            {
                textBox_People.Text = "1";
                textBox_People.ReadOnly = true;
            }

            FillComboBox_Services();
            
        }

        private void FillComboBox_Services()
        {
            string queryString = $"select NameService from Service";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                comboBox_Services.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        private void CreateColumnsServices()
        {
            dataGridView2.Columns.Add("ID_Service", "id");
            dataGridView2.Columns.Add("Price", "price");
            dataGridView2.Columns.Add("NameService", "name");
            dataGridView2.Columns.Add("Amount", "amount");
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDataGridClient(dataGridView1);
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBox_ClientNumber.Text = row.Cells[0].Value.ToString();
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {

            string queryString = $"Select * from Service where NameService = '{comboBox_Services.Text}'";
            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader reader = command.ExecuteReader();
            string amount = textBox_Amount.Text;
            if(!int.TryParse(amount, out int n))
            {
                MessageBox.Show("Invalid Info");
            }
            if(amount == "")
            {
                amount = "1";
            }
            while (reader.Read())
            {
                dataGridView2.Rows.Add(reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                amount);
            }
            
            reader.Close();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            int index = dataGridView2.CurrentCell.RowIndex;
            dataGridView2.Rows.RemoveAt(index);
        }

        private void btn_NewClient_Click(object sender, EventArgs e)
        {
            AddClient addClient = new AddClient(user_);
            addClient.Show();
        }

        private void btn_Select_Click(object sender, EventArgs e)
        {

            AddCheckIn();
            AddGetService();
            AddPrice();
        }

        private void AddPrice()
        {
            int id = GetMaxCheckIn();
            int servicePayment = FindServicePayment(id);
            int roomPayment = FindRoomPayment(id);
            textBox_ServicePrice.Text = servicePayment.ToString();
            textBox_RoomCost.Text = roomPayment.ToString();
            textBox_Total.Text = (servicePayment + roomPayment).ToString();
        }

        private int FindRoomPayment(int id)
        {
            string query = $"select Payment from Check_in where ID_Checkin = {id}";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            int payment = -1;
            if (dataReader.HasRows)
            {
                payment = ((int)dataReader[0]);
            }
            dataReader.Close();
            hotel.closeConnection();
            return payment;
        }

        private int FindServicePayment(int id)
        {
            string query = $"select Payment from GetService where ID_Checkin = {id}";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
            SqlDataReader dataReader = cmd.ExecuteReader();
            //dataReader.Read();
            int payment = 0;

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    payment += (int)dataReader[0];
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }

            dataReader.Close();
            hotel.closeConnection();
            return payment;
        }
        private void AddGetService()
        {
            int numberDays = (int)((dateTimePicker2.Value - dateTimePicker1.Value).Days);
           
            for (int i = 0; i < dataGridView2.Rows.Count; ++i)
            {
                int amount = int.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                SqlParameter checkin = new SqlParameter("@checkin", GetMaxCheckIn());
                SqlParameter service = new SqlParameter("@service", dataGridView2.Rows[i].Cells[0].Value);
                SqlParameter amountP = new SqlParameter("@amount", amount);
                SqlParameter days = new SqlParameter("@days", numberDays);
                SqlParameter payment = new SqlParameter("@payment", GetServiceCost((int)dataGridView2.Rows[i].Cells[0].Value) * (numberDays * amount));
                hotel.openConnection();
                string query = $"insert into GetService values(@service, @checkin, @amount, @days, @payment)";
                SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
                cmd.Parameters.Add(checkin);
                cmd.Parameters.Add(service);
                cmd.Parameters.Add(amountP);
                cmd.Parameters.Add(days);
                cmd.Parameters.Add(payment);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Booking completed successfully!");
            }
        }
        private int GetServiceCost(int id_service)
        {
            string query = $"select Price from Service where ID_Service = {id_service}";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            int cost = 1;
            if (dataReader.HasRows)
            {
                cost = ((int)dataReader[0]);
            }
            dataReader.Close();
            hotel.closeConnection();
            
            return cost;
        }

        public int GetMaxCheckIn()
        {
            string getCheckIn = $"select ID_Checkin from Check_in where ID_Checkin = (select max(ID_Checkin) from Check_in)";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(getCheckIn, hotel.GetConnection());
            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            int id = 1;
            if (dataReader.HasRows)
            {
                id = ((int)dataReader[0]);
            }
            dataReader.Close();
            hotel.closeConnection();
            return id;
        }
        
        private void AddCheckIn()
        {
            int cost = GetRoomCost(int.Parse(textBox_RoomNumber.Text));
            
            if (textBox_ClientNumber.Text == "")
            {
                MessageBox.Show("Select Client");
                return;
            }
            if(textBox_People.Text == "")
            {
                MessageBox.Show("Select amount of people");
                return;
            }
            if(!int.TryParse(textBox_People.Text, out int n))
            {
                MessageBox.Show("Invalid info");
                return;
            }

            int roomPayment = (int)((dateTimePicker2.Value - dateTimePicker1.Value).Days) * cost;
            SqlParameter checkin = new SqlParameter("@checkin", dateTimePicker1.Value);
            SqlParameter departure = new SqlParameter("@departure", dateTimePicker2.Value);
            SqlParameter payment = new SqlParameter("@payment", roomPayment);
            SqlParameter room = new SqlParameter("@room", textBox_RoomNumber.Text);
            SqlParameter client = new SqlParameter("@client", textBox_ClientNumber.Text);
            SqlParameter people = new SqlParameter("@people", textBox_People.Text);

            string query = $"insert into Check_in values (@checkin, @departure, @payment, @room, @client, @people)";
            hotel.openConnection();
            SqlCommand cmd = new SqlCommand(query, hotel.GetConnection());
            hotel.openConnection();
            cmd.Parameters.Add(checkin);
            cmd.Parameters.Add(departure);
            cmd.Parameters.Add(payment);
            cmd.Parameters.Add(room);
            cmd.Parameters.Add(client);
            cmd.Parameters.Add(people);
            cmd.ExecuteNonQuery();


            hotel.closeConnection();
        }
        private int GetRoomCost(int number)
        {
            string queryPrice = $"select Cost from Room where ID_Room = {number}";
            SqlCommand cmd = new SqlCommand(queryPrice, hotel.GetConnection());
            hotel.openConnection();
            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            int cost = 1;
            if (dataReader.HasRows)
            {
                cost = ((int)dataReader[0]);
            }
            dataReader.Close();
            hotel.closeConnection();
            return cost;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            MainMenu mainMenu = new MainMenu(user_);
            this.Hide();
            mainMenu.Show();
            
        }
    }
}
