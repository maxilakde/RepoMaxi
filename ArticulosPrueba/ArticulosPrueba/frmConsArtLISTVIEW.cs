using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Datos;

namespace ArticulosPrueba
{
    public partial class frmConsArtLISTVIEW : Form
    {
        public frmConsArtLISTVIEW()
        {
            InitializeComponent();
        }

        private void frmConsArtLISTVIEW_Load(object sender, EventArgs e)
        {
            CrearColumnas();
            LlenarListView();

        }

        private void CrearColumnas()
        {
            this.lvwArticulos.Columns.Add("Nro.Artículo", 80, HorizontalAlignment.Center);
            this.lvwArticulos.Columns.Add("Artículo", 220, HorizontalAlignment.Left);
            this.lvwArticulos.Columns.Add("Rubro", 120, HorizontalAlignment.Left);
            this.lvwArticulos.Columns.Add("Precio", 80, HorizontalAlignment.Right);
            this.lvwArticulos.Columns.Add("Cantidad", 80, HorizontalAlignment.Right);
            this.lvwArticulos.Columns.Add("U.Medida", 100, HorizontalAlignment.Center);
            this.lvwArticulos.Columns.Add("Fecha Alta", 90, HorizontalAlignment.Center);
        }

        private void LlenarListView()
        {
            this.lvwArticulos.Items.Clear();
            ClaseArticulos art = new ClaseArticulos();
            List<ConsultaArticulos> listaconsulta = new List<ConsultaArticulos>();
            if (txtFiltro.Text == "")
            {
                listaconsulta = art.ObtenerArticulos();
            }
            else 
            {
                listaconsulta = art.ObtenerArticulosxDesc(this.txtFiltro.Text);
            }
            foreach (ConsultaArticulos ca in listaconsulta)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = ca.NroArt;
                item.Text = ca.NroArt.ToString();
                item.SubItems.Add(ca.Descripcion);
                item.SubItems.Add(ca.Rubro);
                item.SubItems.Add(ca.Precio.ToString());
                item.SubItems.Add(ca.Cantidad.ToString());
                item.SubItems.Add(ca.UMedida);
                item.SubItems.Add(ca.FechaAlta.ToShortDateString());
                if (ca.Baja)
                {
                    item.BackColor = Color.Red;
                }
                lvwArticulos.Items.Add(item);
            }
        }

        private void ItemNuevo_Click(object sender, EventArgs e)
        {
            frmArticulos frm = new frmArticulos();
            frm.ShowDialog();
            LlenarListView();
        }

        private void ItemBaja_Click(object sender, EventArgs e)
        {
            ClaseArticulos art = new ClaseArticulos();
            if (art.DardeBajaArticulo(Convert.ToInt32(this.lvwArticulos.SelectedItems[0].Tag)))
            {
                MessageBox.Show("EL ARTICULO SE DIO DE BAJA PIOLA");
                LlenarListView();
            }
        }

        private void popConsulta_Opening(object sender, CancelEventArgs e)
        {
            if (this.lvwArticulos.SelectedItems.Count == 0)
            {
                this.ItemBaja.Enabled = false;
                this.ItemEditar.Enabled = false;
            }
            else
            {
                this.ItemBaja.Enabled = true;
                this.ItemEditar.Enabled = true;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LlenarListView();
            }
        }

        private void ItemEditar_Click(object sender, EventArgs e)
        {
            frmArticulos frm = new frmArticulos();
            frm.IdArticulo = Convert.ToInt32(this.lvwArticulos.SelectedItems[0].Tag);
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                LlenarListView();
            }
        }
    }
}
