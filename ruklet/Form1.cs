using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxWMPLib;
using WMPLib;

namespace ruklet
{

    public partial class Form1 : Form
    {
        private Size originalPanelSize;
        private Dictionary<Control, Rectangle> originalBounds = new Dictionary<Control, Rectangle>();

        Image dvadeset;
        Image pedeset;
        Image sto;

        private Dictionary<PictureBox, Image> prethodnaRuka = new Dictionary<PictureBox, Image>();
        private List<PictureBox> sektor1Brojevi = new List<PictureBox>();
        private List<PictureBox> sektor2Brojevi = new List<PictureBox>();
        private List<PictureBox> sektor3Brojevi = new List<PictureBox>();
        List<PictureBox> pictureBoxes = new List<PictureBox>();
        List<PictureBox> pictureBoxes2 = new List<PictureBox>();
        List<PictureBox> historyBoxes = new List<PictureBox>();
        private List<PictureBox> crvenoBrojevi = new List<PictureBox>();
        private bool rundaUToku = false;
        int zbir = 0;
        int ulog = 0;
        private int sektor1Klikova = 0;
        private int sektor2Klikova = 0;
        private int sektor3Klikova = 0;

        public Form1()
        {
            InitializeComponent();
            dvadeset = Image.FromFile(@"slike\c20.png");
            pedeset = Image.FromFile(@"slike\c50.png");
            sto = Image.FromFile(@"slike\c100.png");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void Odigraj(int[] brojevi)
        {
            int trenutnoStanje = 0;
            int ukupnaDoplata = 0;

            if (!int.TryParse(richTextBox1.Text, out trenutnoStanje))
                trenutnoStanje = 0;

            // Povećavanje postojećih čipova (20 → 50 → 100)
            foreach (int broj in brojevi)
            {
                foreach (PictureBox pb in pictureBoxes)
                {
                    if (pb.Tag != null && pb.Tag.ToString() == broj.ToString())
                    {
                        if (pb.Image == dvadeset && trenutnoStanje >= 30)
                        {
                            pb.Image = pedeset;
                            pb.SizeMode = PictureBoxSizeMode.StretchImage;
                            trenutnoStanje -= 30;
                            ukupnaDoplata += 30;
                        }
                        else if (pb.Image == pedeset && trenutnoStanje >= 50)
                        {
                            pb.Image = sto;
                            pb.SizeMode = PictureBoxSizeMode.StretchImage;
                            trenutnoStanje -= 50;
                            ukupnaDoplata += 50;
                        }
                    }
                }
            }

            // Postavljanje novih čipova (20) gde ih nema
            foreach (int broj in brojevi)
            {
                foreach (PictureBox pb in pictureBoxes)
                {
                    if (pb.Tag != null && pb.Tag.ToString() == broj.ToString())
                    {
                        if (pb.Image == null && trenutnoStanje >= 20)
                        {
                            pb.Image = dvadeset;
                            pb.SizeMode = PictureBoxSizeMode.StretchImage;
                            trenutnoStanje -= 20;
                            ukupnaDoplata += 20;
                        }
                    }
                }
            }

            // Ažuriraj prikaz stanja i ukupnog uloga
            richTextBox1.Text = trenutnoStanje.ToString();

            int prethodniUlog = 0;
            int.TryParse(richTextBox2.Text, out prethodniUlog);
            int noviUlog = prethodniUlog + ukupnaDoplata;
            richTextBox2.Text = noviUlog.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            originalPanelSize = panel1.Size;

            foreach (Control ctrl in panel1.Controls)
            {
                originalBounds[ctrl] = ctrl.Bounds;
            }
            for (int i = 1; i <= 10; i++)
            {
                PictureBox pb = (PictureBox)this.Controls.Find("i" + i, true).FirstOrDefault();
                if (pb != null) historyBoxes.Add(pb);
            }

            foreach (PictureBox pb in panel1.Controls.OfType<PictureBox>())
            {
                pb.MouseClick += MyHandler;
                pictureBoxes.Add(pb);
            }
            foreach (Object element in panel2.Controls)
            {
                if (element is PictureBox)
                {
                    PictureBox pb2 = (PictureBox)element;                    
                    pictureBoxes2.Add(pb2);
                }
            }
            richTextBox1.Enabled = false;
            richTextBox2.Enabled = false;
            richTextBox3.Enabled = false;
            var kontroleZaIskljucivanje = new List<Control> {
                         panel1, button1, sektor1, sektor2, sektor3,
                        crveno, crno, red1, red2, red3, buttonReset
                        };

            foreach (var ctrl in kontroleZaIskljucivanje)
                ctrl.Enabled = false;

            timer1.Enabled = false;
            timer1.Start();
            timer1.Interval = 1000;
            progressBar1.Maximum = 30;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        private void MyHandler(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            PictureBox pb = (PictureBox)sender;

            int kasa = 0;
            int ulog = 0;

            int.TryParse(richTextBox1.Text, out kasa);
            int.TryParse(richTextBox2.Text, out ulog);

            // Desni klik: SKIDANJE čipa
            if (me.Button == MouseButtons.Right)
            {
                int vraceniIznos = 0;

                if (pb.Image == dvadeset)
                    vraceniIznos = 20;
                else if (pb.Image == pedeset)
                    vraceniIznos = 50;
                else if (pb.Image == sto)
                    vraceniIznos = 100;

                if (vraceniIznos > 0)
                {
                    kasa += vraceniIznos;
                    ulog -= vraceniIznos;

                    pb.Image = null;
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;

                    richTextBox1.Text = kasa.ToString();
                    richTextBox2.Text = ulog.ToString();
                }

                return;
            }

            // Levi klik: DODAVANJE čipa
            int doplata = 0;

            if (pb.Image == null)
            {
                doplata = 20;
                if (kasa >= doplata)
                {
                    pb.Image = dvadeset;
                    kasa -= doplata;
                    ulog += doplata;
                }
                else
                {
                    MessageBox.Show("Nemate dovoljno novca za ovu opkladu!");
                    return;
                }
            }
            else if (pb.Image == dvadeset)
            {
                doplata = 30;
                if (kasa >= doplata)
                {
                    pb.Image = pedeset;
                    kasa -= doplata;
                    ulog += doplata;
                }
                else
                {
                    MessageBox.Show("Nemate dovoljno novca za ovu opkladu!");
                    return;
                }
            }
            else if (pb.Image == pedeset)
            {
                doplata = 50;
                if (kasa >= doplata)
                {
                    pb.Image = sto;
                    kasa -= doplata;
                    ulog += doplata;
                }
                else
                {
                    MessageBox.Show("Nemate dovoljno novca za ovu opkladu!");
                    return;
                }
            }
            else if (pb.Image == sto)
            {
                // Maksimalni čip, ne radi ništa
                return;
            }

            pb.SizeMode = PictureBoxSizeMode.StretchImage;

            // Ažuriranje vrednosti
            richTextBox1.Text = kasa.ToString();
            richTextBox2.Text = ulog.ToString();
        }

        private void SetujAktivnostKontrola(bool aktivno)
        {
            foreach (Control kontrola in this.Controls)
            {
                if (kontrola is Button || kontrola is TextBox || kontrola is RichTextBox || kontrola is Panel)
                {
                    kontrola.Enabled = aktivno;

                    if (kontrola is Panel)
                    {
                        foreach (Control uPanelu in kontrola.Controls)
                        {
                            if (uPanelu is Button || uPanelu is PictureBox)
                                uPanelu.Enabled = aktivno;
                        }
                    }
                }
            }
        }
        private async Task PrikaziSnimak(int broj)
        {
            string folder = Path.Combine(Application.StartupPath, "slike");
            string imeFajla = $"br{broj}.mp4";
            string putanja = Path.Combine(folder, imeFajla);

            if (File.Exists(putanja))
            {
                SetujAktivnostKontrola(false); // onemogući klikove

                playerVideo.Size = new Size(640, 360);
                playerVideo.Location = new Point(
                    (this.Width - playerVideo.Width) / 2,
                    (this.Height - playerVideo.Height) / 2 );

                playerVideo.URL = putanja;
                playerVideo.Visible = true;
                playerVideo.Ctlcontrols.play();
                playerVideo.uiMode = "none";
                await Task.Delay(8000); // pauza dok ide video

                playerVideo.Ctlcontrols.stop();
                playerVideo.Visible = false;

                SetujAktivnostKontrola(true); // vrati kontrole
            }
            else
            {
                MessageBox.Show($"Video fajl za broj {broj} nije pronađen.");
            }
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {           
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                    (e.KeyChar != '.'))
                {
                    e.Handled = true;
                }           
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty )
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {

            if (rundaUToku) return;
            rundaUToku = true;

            if (progressBar1.Value != 30)
            {
                progressBar1.Value++;
                rundaUToku = false;
                return;
            }

            timer1.Stop();
            panel1.Enabled = false;

            Random rnd = new Random();
            int izvucen = rnd.Next(37);

            await PrikaziSnimak(izvucen);

            for (int i = 0; i < pictureBoxes.Count; i++)
            {
                if (pictureBoxes[i].Name.Substring(1) == izvucen.ToString())
                {
                    for (int k = historyBoxes.Count - 1; k > 0; k--)
                    {
                        historyBoxes[k].BackgroundImage = historyBoxes[k - 1].BackgroundImage;
                    }
                    historyBoxes[0].BackgroundImage = pictureBoxes[i].BackgroundImage;
                    break;
                }
            }

            int ukupnoDobijeno = 0;
            foreach (PictureBox pb in pictureBoxes)
            {
                if (pb.Image != null)
                {
                    string broj = pb.Name.Substring(1);
                    if (broj == izvucen.ToString())
                    {
                        int ulogNaBroj = pb.Image == dvadeset ? 20 :
                                         pb.Image == pedeset ? 50 :
                                         pb.Image == sto ? 100 : 0;

                        ukupnoDobijeno += ulogNaBroj * 36;
                    }
                }
            }

            if (ukupnoDobijeno > 0)
            {
                zbir = int.Parse(richTextBox1.Text) + ukupnoDobijeno;
                richTextBox1.Text = zbir.ToString();
                richTextBox3.Text = ukupnoDobijeno.ToString();
            }
            else
            {
                richTextBox3.Text = "0";
            }

            prethodnaRuka.Clear();
            foreach (var pb in pictureBoxes)
            {
                if (pb.Image != null)
                {
                    prethodnaRuka[pb] = pb.Image;
                    pb.Image = null;
                }
            }

            timer1.Enabled = true;
            panel1.Enabled = true;
            progressBar1.Value = 0;
            ulog = 0;
            richTextBox2.Text = "0";
            ponovi.Enabled = true;
            sektor1Klikova = 0;

            rundaUToku = false;
            timer1.Start();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int p;
            int p2;
            if (richTextBox1.Text != string.Empty)
            {
                p = int.Parse(richTextBox1.Text);
                p2 = int.Parse(textBox1.Text);
                richTextBox1.Text = (p2 + p).ToString();
            }
            else
            richTextBox1.Text = textBox1.Text;
            textBox1.Text = "";
            panel1.Enabled = true;
            sektor1.Enabled = true;
            sektor2.Enabled = true;
            sektor3.Enabled = true;
            crveno.Enabled = true;
            crno.Enabled = true;
            red1.Enabled = true;
            red2.Enabled = true;
            red3.Enabled = true;
            buttonReset.Enabled = true; 
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            int ukupnoNaTabli = 0;

            // 1. Saberi sve trenutno postavljene čipove
            foreach (PictureBox pb in pictureBoxes)
            {
                if (pb.Image != null)
                {
                    if (pb.Image == dvadeset)
                        ukupnoNaTabli += 20;
                    else if (pb.Image == pedeset)
                        ukupnoNaTabli += 50;
                    else if (pb.Image == sto)
                        ukupnoNaTabli += 100;

                    pb.Image = null; // Očisti čip
                }
            }

            // 2. Vratiti sabrani iznos u kasu
            int trenutnoStanje = 0;
            if (!int.TryParse(richTextBox1.Text, out trenutnoStanje))
            {
                trenutnoStanje = 0;
            }

            trenutnoStanje += ukupnoNaTabli;
            richTextBox1.Text = trenutnoStanje.ToString();

            // 3. Resetuj sve brojače i prikaze
            ulog = 0;
            richTextBox2.Text = "0";
            richTextBox3.Text = "0";

            sektor1Klikova = 0;
            sektor1Brojevi.Clear();
            sektor1.Enabled = true;

            sektor2Klikova = 0;
            sektor2Brojevi.Clear();
            sektor2.Enabled = true;

            sektor3Klikova = 0;
            sektor3Brojevi.Clear();
            sektor3.Enabled = true;

            prethodnaRuka.Clear();
            ponovi.Enabled = false;

            crvenoBrojevi.Clear();
            crveno.Enabled = true;

            MessageBox.Show("Reset uspešno izvršen!", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ponovi_Click(object sender, EventArgs e)
        {
            if (prethodnaRuka.Count == 0)
            {
                MessageBox.Show("Nema prethodne ruke za ponavljanje!");
                return;
            }

            if (ulog != 0)
            {
                MessageBox.Show("Već ste postavili opklade! Pokušajte ponovo u sledećoj rundi.");
                return;
            }

            int trenutnoStanje = int.Parse(richTextBox1.Text);
            int potrebanIznos = 0;

            // Prvo izračunaj koliko para treba
            foreach (var par in prethodnaRuka)
            {
                Image img = par.Value;
                if (img == dvadeset)
                    potrebanIznos += 20;
                else if (img == pedeset)
                    potrebanIznos += 50;
                else if (img == sto)
                    potrebanIznos += 100;
            }

            if (trenutnoStanje < potrebanIznos)
            {
                MessageBox.Show("Nemate dovoljno novca da ponovite prethodnu opkladu!");
                return;
            }

            // Očisti sve čipove
            foreach (PictureBox pb in pictureBoxes)
            {
                pb.Image = null;
            }

            // Postavi sve čipove ponovo
            foreach (var par in prethodnaRuka)
            {
                PictureBox pb = par.Key;
                Image img = par.Value;

                pb.Image = img;
            }

            
            zbir = trenutnoStanje - potrebanIznos;
            ulog = potrebanIznos;

            richTextBox1.Text = zbir.ToString();
            richTextBox2.Text = ulog.ToString();
            richTextBox3.Text = "0";

            ponovi.Enabled = false;
        }
        private void sektor1_Click(object sender, EventArgs e)
        {
            int[] sektor1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            Odigraj(sektor1);

        }

        private void sektor2_Click(object sender, EventArgs e)
        {
            int[] sektor2 = { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            Odigraj(sektor2);
        }

        private void sektor3_Click(object sender, EventArgs e)
        {
            int[] sektor3 = { 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
            Odigraj(sektor3);
            
        }

        private void crveno_Click(object sender, EventArgs e)
        {
            int[] crveniBrojevi = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            Odigraj(crveniBrojevi);
           
        }

        private void crno_Click(object sender, EventArgs e)
        {
            int[] crniBrojevi = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };
            Odigraj(crniBrojevi);
        }

        private void red1_Click(object sender, EventArgs e)
        {
            int[] red1Brojevi = { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34 };
            Odigraj(red1Brojevi);
        }

        private void red2_Click(object sender, EventArgs e)
        {
            int[] red2Brojevi = { 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35 };
            Odigraj(red2Brojevi);
        }

        private void red3_Click(object sender, EventArgs e)
        {
            int[] red3Brojevi = { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36 };
            Odigraj(red3Brojevi);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            float xRatio = (float)panel1.Width / originalPanelSize.Width;
            float yRatio = (float)panel1.Height / originalPanelSize.Height;

            foreach (Control ctrl in panel1.Controls)
            {
                if (originalBounds.ContainsKey(ctrl))
                {
                    Rectangle orig = originalBounds[ctrl];
                    ctrl.Left = (int)(orig.Left * xRatio);
                    ctrl.Top = (int)(orig.Top * yRatio);
                    ctrl.Width = (int)(orig.Width * xRatio);
                    ctrl.Height = (int)(orig.Height * yRatio);
                }
            }
        }
    }
}
