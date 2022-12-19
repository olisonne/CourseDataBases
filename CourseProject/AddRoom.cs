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
    
    enum RoomTypes
    {
        Suite,
        JuniorSuite,
        Multi_bed
    }
    public partial class AddRoom : Form
    {
        private CheckUser user_;
        private Hotel hotel = new Hotel();
        public AddRoom(CheckUser user)
        {
            user_ = user;
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void AddRoom_Load(object sender, EventArgs e)
        {
            
        }

        private byte[] GetImage()
        {
            MemoryStream stream = new MemoryStream();
            pictureBox1.Image.Save(stream, pictureBox1.Image.RawFormat);

            return stream.GetBuffer();
        }

        private void btn_Upload_Click(object sender, EventArgs e)
        {
            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.Filter = "Image Files (*.jpg;*.jpeg;.*.gif;)|*.jpg;*.jpeg;.*.gif";
            if (opnfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(opnfd.FileName);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox_Type.SelectedIndex == ((int)RoomTypes.Suite) || comboBox_Type.SelectedIndex == ((int)RoomTypes.JuniorSuite))
            {
                textBox_Beds.BackColor = Color.Gray;
                textBox_Rooms.BackColor = Color.White;
                textBox_Places.BackColor = Color.White;
                textBox_Beds.Enabled = false;
                textBox_Rooms.Enabled = true;
                textBox_Places.Enabled = true;
            }
            else
            {
                textBox_Beds.BackColor = Color.White;
                textBox_Rooms.BackColor = Color.Gray;
                textBox_Places.BackColor = Color.Gray;
                textBox_Beds.Enabled = true;
                textBox_Rooms.Enabled = false;
                textBox_Places.Enabled = false;
            }
        }

        private bool CheckInfoFull()
        {
            if (textBox_Number.Text == "" || textBox_Cost.Text == "" || textBox_Floor.Text == "" || textBox_Phone.Text == "" || comboBox_Type.Text == "")
            {
                return true;
            }

            if (comboBox_Type.SelectedIndex == ((int)RoomTypes.Suite) || comboBox_Type.SelectedIndex == ((int)RoomTypes.JuniorSuite))
            {
                if (textBox_Places.Text == "" || textBox_Rooms.Text == "")
                {
                    return true;
                }

            }
            else if (textBox_Beds.Text == "" && comboBox_Type.SelectedIndex == ((int)RoomTypes.Multi_bed))
            {
                return true;
            }
            return false;
        }

        private bool CheckInfoType()
        {
            if(!int.TryParse(textBox_Number.Text, out int n))
            {
                return true;
            }
            if(!int.TryParse(textBox_Floor.Text, out int p))
            {
                return true;
            }
            if (!int.TryParse(textBox_Cost.Text, out int k))
            {
                return true;
            }
            return false;
        }

        private bool CheckInfoSuiteType()
        {
            if (!int.TryParse(textBox_Rooms.Text, out int k))
            {
                return true;
            }
            if (!int.TryParse(textBox_Places.Text, out int n))
            {
                return true;
            }
            return false;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            
            if(CheckInfoFull())
            {
                MessageBox.Show("Information not complete");
                return;
            }
            if(CheckInfoType())
            {
                MessageBox.Show("Invalid Info");
                return;
            }
            SqlParameter number = new SqlParameter("@number", int.Parse(textBox_Number.Text));
            SqlParameter floor = new SqlParameter("@floor", int.Parse(textBox_Floor.Text));
            SqlParameter phone = new SqlParameter("@phone", textBox_Phone.Text);

            SqlParameter cost = new SqlParameter("@cost", int.Parse(textBox_Cost.Text));
            SqlParameter photo = new SqlParameter("@photo", GetImage());
            hotel.openConnection();


            if (CheckIfExist(int.Parse(textBox_Number.Text)))
            {
                return;
            }

            if (comboBox_Type.SelectedIndex == ((int)RoomTypes.Suite) || comboBox_Type.SelectedIndex == ((int)RoomTypes.JuniorSuite))
            {
                if(CheckInfoSuiteType())
                {
                    MessageBox.Show("Invalid Info");
                    return;
                }
                SqlParameter rooms = new SqlParameter("@rooms", int.Parse(textBox_Rooms.Text));
                SqlParameter places = new SqlParameter("@places", int.Parse(textBox_Places.Text));
                string TypeRoom = (comboBox_Type.SelectedIndex == ((int)RoomTypes.Suite)) ? "Suite" : "Junior_Suite";
                string addQuery = $"insert into Room values (@number, @floor, @phone, @cost, '{TypeRoom}', @photo)";
                string addSubQuery = $"insert into Suite_JuniorSuite values" +
                    $"((select ID_Room from Room where ID_Room not in (Select ID_Room from Suite_JuniorSuite) and Type = '{TypeRoom}'), " +
                    $"'{TypeRoom}', @rooms, @places)";


                SqlCommand command = new SqlCommand(addQuery, hotel.GetConnection());
                command.Parameters.Add(number);
                command.Parameters.Add(floor);
                command.Parameters.Add(phone);
                command.Parameters.Add(cost);
                command.Parameters.Add(photo);

                

                command.ExecuteNonQuery();

                SqlCommand subCommand = new SqlCommand(addSubQuery, hotel.GetConnection());
                subCommand.Parameters.Add(rooms);
                subCommand.Parameters.Add(places);
                subCommand.ExecuteNonQuery();
            }
            else
            {
                if(!int.TryParse(textBox_Beds.Text, out int n))
                {
                    MessageBox.Show("Invalid Info");
                    return;
                }
                SqlParameter beds = new SqlParameter("@beds", int.Parse(textBox_Beds.Text));
                string addQuery = $"insert into Room values (@number, @floor, @phone, @cost, 'Multi_bed', @photo)";
                string addSubQuery = $"insert into Multi_bed values(" +
                    $"(select ID_Room from Room where ID_Room not in (Select ID_Room from Multi_bed) and Type = 'Multi_bed'), @beds)";
                SqlCommand command = new SqlCommand(addQuery, hotel.GetConnection());
                SqlCommand subCommand = new SqlCommand(addSubQuery, hotel.GetConnection());

                command.Parameters.Add(number);
                command.Parameters.Add(floor);
                command.Parameters.Add(phone);
                command.Parameters.Add(cost);
                command.Parameters.Add(photo);
                command.ExecuteNonQuery();
               
                subCommand.Parameters.Add(beds);
                subCommand.ExecuteNonQuery();
            }
            MessageBox.Show("Information saved");
            RoomManagement room = new RoomManagement(user_);
            this.Hide();
            room.Show();
            hotel.closeConnection();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private bool CheckIfExist(int number)
        {
            string findRoom = $"select ID_Room from Room where ID_Room = {number}";
            SqlCommand findCommand = new SqlCommand(findRoom, hotel.GetConnection());
            SqlDataReader reader = findCommand.ExecuteReader();
            if (reader.HasRows)
            {
                MessageBox.Show("This room already exists!");
                reader.Close();
                return true;
            }
            reader.Close();
            return false;
        }
    }
}
