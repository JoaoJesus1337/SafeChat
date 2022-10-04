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
    public partial class Login : Form
    {
        Container GereRestauranteContainer = new Container();
        bool mouseDown;
        private Point offset;
        public Login()
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

        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //Connecxão a Base de Dados usando a SQL CONNECTIO
            SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\barba\OneDrive - IPLeiria\Documents\testlogin.mdf;Integrated Security=True;Connect Timeout=30");
            //select a base de dados onde username = textbox Username e a Password = textbox Username
            SqlDataAdapter sda = new SqlDataAdapter("select count(*) from  login where username='" + textBox1.Text + "' and password ='" + textBox2.Text + "'", conn);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            //se o count for 1 é porque o username e a passwrod existem ent entra no chat
            if(dt.Rows[0][0].ToString() == "1")
            {
                this.Hide();
                Chat chat = new Chat();
                chat.Show();
            }
            // else password ou username está errado
            else
            {
                MessageBox.Show("Password ou Username Incorretos");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //butao para abrir o menu de registar
            this.Hide();
            Registar registar = new Registar();
            registar.Closed += (s, args) => this.Show();
            registar.Show();
        }
    }
}
