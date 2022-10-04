using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Ficha3_Cliente
{
    public partial class Registar : Form
    {
        bool mouseDown;
        private Point offset;

        public Registar()
        {
            InitializeComponent();
        }
        private void mouseDown_Event(object sender, MouseEventArgs e)
        {
            offset.X = e.X;
            offset.Y = e.Y;
            mouseDown = true;
        }

        private void mouseMove_Event(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - offset.X, currentScreenPos.Y - offset.Y);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //conexão a base de dados atraves do sql connect
            SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\barba\OneDrive - IPLeiria\Documents\testlogin.mdf;Integrated Security=True;Connect Timeout=30");
            conn.Open();
            //inserir na base de dados o username e password das respeticas textBoxes
            var sql = "INSERT INTO login(username, password) VALUES(@username, @password)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@username", textBoxRegistarUser.Text);
                cmd.Parameters.AddWithValue("@password", textBoxRegistarPassword.Text);

                cmd.ExecuteNonQuery();
            }

            this.Hide();
            Login login = new Login();
            login.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.Show();
        }

        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}
