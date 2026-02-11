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
    public partial class frmConsArtDATAGRID : Form
    {
        public frmConsArtDATAGRID()
        {
            InitializeComponent();
        }

        private void frmConsArtDATAGRID_Load(object sender, EventArgs e)
        {
            ClaseArticulos art = new ClaseArticulos();
            this.dgvArticulos.DataSource = art.ObtenerArticulos();
        }
    }
}
