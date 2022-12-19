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
    public partial class Reginster : Form
    {
        private Hotel hotel = new Hotel();
        public Reginster()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void Reginster_Load(object sender, EventArgs e)
        {
            textBox_Password.PasswordChar = '*';
            textBox_Login.MaxLength = 50;
            textBox_Password.MaxLength = 50;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            
            string loginUser = textBox_Login.Text;
            string passUser = textBox_Password.Text;

            if(loginUser == "" || passUser == "")
            {
                MessageBox.Show("Information not complete");
                return;
            }
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string queryString = $"select * from register " +
                $"where login_user = '{loginUser}' and password_user = '{passUser}'";

            SqlCommand sqlCommand = new SqlCommand(queryString, hotel.GetConnection());

            adapter.SelectCommand = sqlCommand;
            adapter.Fill(table);

            if(table.Rows.Count == 1)
            {
                CheckUser user = new CheckUser(table.Rows[0].ItemArray[1].ToString(), Convert.ToBoolean(table.Rows[0].ItemArray[3]));
                MessageBox.Show("Вы успешно вошли!");
                MainMenu form1 = new MainMenu(user);
                this.Hide();
                form1.ShowDialog();
                
            }
            else
            {
                MessageBox.Show("Такого аккаунта не существует");
            }
        }

        private void btn_NewUser_Click(object sender, EventArgs e)
        {
            Sign_up sign_Up = new Sign_up();
            sign_Up.Show();
            this.Hide();
        }
    }
}
