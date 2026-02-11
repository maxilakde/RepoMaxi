namespace ArticulosPrueba
{
    partial class frmArticulos
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblIdArticulo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRubro = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPrecio = new System.Windows.Forms.TextBox();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpFechaAlta = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbUMedida = new System.Windows.Forms.ComboBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblIdArticulo
            // 
            this.lblIdArticulo.AutoSize = true;
            this.lblIdArticulo.Location = new System.Drawing.Point(33, 23);
            this.lblIdArticulo.Name = "lblIdArticulo";
            this.lblIdArticulo.Size = new System.Drawing.Size(0, 13);
            this.lblIdArticulo.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Descripción:";
            // 
            // txtDesc
            // 
            this.txtDesc.Location = new System.Drawing.Point(84, 50);
            this.txtDesc.Multiline = true;
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.Size = new System.Drawing.Size(312, 41);
            this.txtDesc.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Rubro:";
            // 
            // txtRubro
            // 
            this.txtRubro.Location = new System.Drawing.Point(84, 101);
            this.txtRubro.Name = "txtRubro";
            this.txtRubro.Size = new System.Drawing.Size(260, 20);
            this.txtRubro.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 136);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Precio:";
            // 
            // txtPrecio
            // 
            this.txtPrecio.Location = new System.Drawing.Point(85, 133);
            this.txtPrecio.Name = "txtPrecio";
            this.txtPrecio.Size = new System.Drawing.Size(62, 20);
            this.txtPrecio.TabIndex = 6;
            // 
            // txtCantidad
            // 
            this.txtCantidad.Location = new System.Drawing.Point(85, 162);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(62, 20);
            this.txtCantidad.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 165);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Cantidad:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 193);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Fecha Alta:";
            // 
            // dtpFechaAlta
            // 
            this.dtpFechaAlta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaAlta.Location = new System.Drawing.Point(85, 188);
            this.dtpFechaAlta.Name = "dtpFechaAlta";
            this.dtpFechaAlta.Size = new System.Drawing.Size(109, 20);
            this.dtpFechaAlta.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 220);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "U.Medida:";
            // 
            // cmbUMedida
            // 
            this.cmbUMedida.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUMedida.FormattingEnabled = true;
            this.cmbUMedida.Location = new System.Drawing.Point(84, 217);
            this.cmbUMedida.Name = "cmbUMedida";
            this.cmbUMedida.Size = new System.Drawing.Size(121, 21);
            this.cmbUMedida.TabIndex = 12;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(321, 248);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(75, 23);
            this.btnGuardar.TabIndex = 13;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // frmArticulos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 283);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.cmbUMedida);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dtpFechaAlta);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtCantidad);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPrecio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtRubro);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblIdArticulo);
            this.Name = "frmArticulos";
            this.Text = "Artículos";
            this.Load += new System.EventHandler(this.frmArticulos_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIdArticulo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRubro;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPrecio;
        private System.Windows.Forms.TextBox txtCantidad;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpFechaAlta;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbUMedida;
        private System.Windows.Forms.Button btnGuardar;

    }
}

