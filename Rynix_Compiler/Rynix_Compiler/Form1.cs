using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rynix_Compiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            Scanner scanner = new Scanner();

            scanner.StartScanning(txtSource.Text);

            dgvTokens.Columns.Clear();
            dgvTokens.Rows.Clear();

            dgvTokens.Columns.Add("Lexeme", "Lexeme");
            dgvTokens.Columns.Add("TokenType", "Token Type");

            foreach (Token token in scanner.Tokens)
            {
                dgvTokens.Rows.Add(token.lex, token.token_type.ToString());
            }

        }

        private void txtSource_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
