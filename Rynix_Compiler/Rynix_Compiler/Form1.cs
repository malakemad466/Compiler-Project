using System;
using System.Windows.Forms;

namespace Rynix_Compiler
{
    public partial class Form1 : Form
    {
        private Scanner scanner = new Scanner();

        public Form1()
        {
            InitializeComponent();

            dgvTokens.AllowUserToAddRows = false;
            dgvTokens.ReadOnly = true;
            dgvTokens.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            Errorslist.Multiline = true;
            Errorslist.ScrollBars = ScrollBars.Both;
            Errorslist.ReadOnly = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            scanner.StartScanning(txtSource.Text);

            dgvTokens.Rows.Clear();
            dgvTokens.Columns.Clear();

            dgvTokens.Columns.Add("Lexeme", "Lexeme");
            dgvTokens.Columns.Add("TokenType", "Token Class");

            foreach (Token token in scanner.Tokens)
            {
                string tokenType = token.token_type.ToString();

                switch (token.token_type)
                {
                    case Token_Class.Int:
                    case Token_Class.Float:
                    case Token_Class.String:
                    case Token_Class.Bool:
                    case Token_Class.Char:
                    case Token_Class.Array:
                    case Token_Class.Void:
                        tokenType = "Data Type (" + tokenType + ")";
                        break;
                }

                dgvTokens.Rows.Add(token.lex, tokenType);
            }

            Errorslist.Clear();

            if (Errors.Error_List.Count == 0)
            {
                Errorslist.Text = "No errors found.";
            }
            else
            {
                Errorslist.Lines = Errors.Error_List.ToArray();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSource.Clear();
            dgvTokens.Rows.Clear();
            dgvTokens.Columns.Clear();
            Errorslist.Clear();

            scanner.Tokens.Clear();
            Errors.Error_List.Clear();
        }

        private void Errorslist_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
