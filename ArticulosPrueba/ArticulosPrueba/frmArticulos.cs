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
    public partial class frmArticulos : Form
    {
        public Int32 IdArticulo;

        public frmArticulos()
        {
            InitializeComponent();
        }

        private void frmArticulos_Load(object sender, EventArgs e)
        {
            UnidadMedidaClass umc = new UnidadMedidaClass();
            this.cmbUMedida.DataSource = umc.ObtenerUMedidas();
            this.cmbUMedida.DisplayMember = "Descripcion";
            this.cmbUMedida.ValueMember = "IdUnidadMedida";
            if (IdArticulo > 0)
            {
                ClaseArticulos ca = new ClaseArticulos();
                Articulos art = new Articulos();
                art = ca.ObtenerArticulo(IdArticulo);
                this.txtDesc.Text = art.Descripcion;
                this.txtCantidad.Text = art.Cantidad.ToString();
                this.txtPrecio.Text = art.Precio.ToString();
                this.txtRubro.Text = art.Rubro;
                this.lblIdArticulo.Text = IdArticulo.ToString();
                this.dtpFechaAlta.Value = art.FechaAlta;
                this.cmbUMedida.SelectedValue = art.IdUnidadMedida;
            }

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            ClaseArticulos ca = new ClaseArticulos();
            ca.Desc = this.txtDesc.Text;
            ca.Cantidad = Convert.ToInt32(this.txtCantidad.Text);
            ca.Precio = Convert.ToDecimal(this.txtPrecio.Text);
            ca.Rubro = this.txtRubro.Text;
            ca.FechaAlta = this.dtpFechaAlta.Value;
            ca.IdUnidadMedida = Convert.ToInt16(this.cmbUMedida.SelectedValue);
            if (IdArticulo > 0)
            {
                ca.IdArticulo = IdArticulo;
                if (ca.EditarArticulo())
                {
                    MessageBox.Show("Editar piola");
                    this.DialogResult = DialogResult.OK;
                    
                    this.Close();
                }
            }
            else
            {
                Int32 IDres = ca.InsertarArticulo();
                this.lblIdArticulo.Text = IDres.ToString();
                MessageBox.Show("Alta piola");
            }
            
           
        }


    }
}
