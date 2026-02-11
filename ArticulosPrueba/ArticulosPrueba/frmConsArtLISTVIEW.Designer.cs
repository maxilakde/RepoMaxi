namespace ArticulosPrueba
{
    partial class frmConsArtLISTVIEW
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lvwArticulos = new System.Windows.Forms.ListView();
            this.popConsulta = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemNuevo = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemEditar = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemBaja = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFiltro = new System.Windows.Forms.TextBox();
            this.popConsulta.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwArticulos
            // 
            this.lvwArticulos.ContextMenuStrip = this.popConsulta;
            this.lvwArticulos.FullRowSelect = true;
            this.lvwArticulos.GridLines = true;
            this.lvwArticulos.Location = new System.Drawing.Point(12, 69);
            this.lvwArticulos.MultiSelect = false;
            this.lvwArticulos.Name = "lvwArticulos";
            this.lvwArticulos.Size = new System.Drawing.Size(733, 238);
            this.lvwArticulos.TabIndex = 0;
            this.lvwArticulos.UseCompatibleStateImageBehavior = false;
            this.lvwArticulos.View = System.Windows.Forms.View.Details;
            // 
            // popConsulta
            // 
            this.popConsulta.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemNuevo,
            this.ItemEditar,
            this.ItemBaja});
            this.popConsulta.Name = "popConsulta";
            this.popConsulta.Size = new System.Drawing.Size(134, 70);
            this.popConsulta.Opening += new System.ComponentModel.CancelEventHandler(this.popConsulta_Opening);
            // 
            // ItemNuevo
            // 
            this.ItemNuevo.Name = "ItemNuevo";
            this.ItemNuevo.Size = new System.Drawing.Size(133, 22);
            this.ItemNuevo.Text = "Nuevo";
            this.ItemNuevo.Click += new System.EventHandler(this.ItemNuevo_Click);
            // 
            // ItemEditar
            // 
            this.ItemEditar.Name = "ItemEditar";
            this.ItemEditar.Size = new System.Drawing.Size(133, 22);
            this.ItemEditar.Text = "Editar";
            this.ItemEditar.Click += new System.EventHandler(this.ItemEditar_Click);
            // 
            // ItemBaja
            // 
            this.ItemBaja.Name = "ItemBaja";
            this.ItemBaja.Size = new System.Drawing.Size(133, 22);
            this.ItemBaja.Text = "Dar de Baja";
            this.ItemBaja.Click += new System.EventHandler(this.ItemBaja_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filtrar x nombre de artículo:";
            // 
            // txtFiltro
            // 
            this.txtFiltro.Location = new System.Drawing.Point(167, 22);
            this.txtFiltro.Name = "txtFiltro";
            this.txtFiltro.Size = new System.Drawing.Size(215, 20);
            this.txtFiltro.TabIndex = 3;
            this.txtFiltro.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // frmConsArtLISTVIEW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 352);
            this.Controls.Add(this.txtFiltro);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lvwArticulos);
            this.Name = "frmConsArtLISTVIEW";
            this.Text = "frmConsArtLISTVIEW";
            this.Load += new System.EventHandler(this.frmConsArtLISTVIEW_Load);
            this.popConsulta.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvwArticulos;
        private System.Windows.Forms.ContextMenuStrip popConsulta;
        private System.Windows.Forms.ToolStripMenuItem ItemNuevo;
        private System.Windows.Forms.ToolStripMenuItem ItemEditar;
        private System.Windows.Forms.ToolStripMenuItem ItemBaja;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFiltro;
    }
}