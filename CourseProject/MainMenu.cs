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
    public partial class MainMenu : Form
    {
        private readonly CheckUser _user;

        public MainMenu(CheckUser user)
        {
            _user = user;
            InitializeComponent();
        }

        private void IsAdmin()
        {
            btnAddRoom.Enabled = _user.IsAdmin;
            btnAddRoom.Visible = _user.IsAdmin;
            btn_AddService.Enabled = _user.IsAdmin;
            btn_AddService.Visible = _user.IsAdmin;
            pictureBox6.Visible = _user.IsAdmin;
            pictureBox7.Visible = _user.IsAdmin;
            pictureBox8.Visible = _user.IsAdmin;
            btn_Statistics.Enabled = _user.IsAdmin;
            btn_Statistics.Visible = _user.IsAdmin;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            AddClient addClient = new AddClient(_user);
            addClient.Show();
            this.Hide();
        }

        private void btnAddRoom_Click(object sender, EventArgs e)
        {
            RoomManagement addRoom = new RoomManagement(_user);
            addRoom.Show();
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_Status.Text = $"{_user.Login} : {_user.Status}";
            IsAdmin();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectRoom booking = new SelectRoom(_user);
            this.Hide();
            booking.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddService addService = new AddService(_user);
            this.Hide();
            addService.Show();
        }

        private void btn_CancelBooking_Click(object sender, EventArgs e)
        {
            BookingCancel bookingCancel = new BookingCancel(_user);
            bookingCancel.Show();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_Statistics_Click(object sender, EventArgs e)
        {
            Statistics statistics = new Statistics();
            statistics.Show();
        }
    }
}
