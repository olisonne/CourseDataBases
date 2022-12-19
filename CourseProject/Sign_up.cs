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
    public partial class Sign_up : Form
    {
        Hotel hotel = new Hotel();
        public Sign_up()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            string login = textBox_Login.Text;
            string password = textBox_Password.Text;
            if (login == "" || password == "")
            {
                MessageBox.Show("Information not complete");
                return;
            }

            if (checkUser())
            {
                return;
            }
            

            string queryString = $"insert into register(login_user, password_user, is_admin) values ('{login}', '{password}', 0)";

            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());
            hotel.openConnection();
            if(command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Аккаунт успешно создан");
                Reginster reginster = new Reginster();
                this.Hide();
                reginster.ShowDialog();
            }
            else
            {
                MessageBox.Show("Аккаунт не создан");
            }
            hotel.closeConnection();
        }

        private bool checkUser()
        {
            string login = textBox_Login.Text;
            string password = textBox_Password.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();
            string queryString = $"select * from register where login_user = '{login}' and password_user = '{password}'";

            SqlCommand command = new SqlCommand(queryString, hotel.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table);

            if(table.Rows.Count > 0)
            {

                MessageBox.Show("Аккаунт уже существует!");
                return true;
            }
            return false;
        }

        private void Sign_up_Load(object sender, EventArgs e)
        {
            textBox_Password.PasswordChar = '*';
            textBox_Login.MaxLength = 50;
            textBox_Password.MaxLength = 50;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }

}
