using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Datos
{
    public class ClaseArticulos
    {
        public Int32 IdArticulo { get; set; }
        public String Desc { get; set; }
        public Decimal Precio { get; set; }
        public Int32 Cantidad { get; set; }
        public String Rubro { get; set; }
        public DateTime FechaAlta { get; set; }
        public Int16  IdUnidadMedida { get; set; }
        public DateTime? FechaBaja { get; set; }

        private ArticulosPruebaEntities db = new ArticulosPruebaEntities();

        public Int32 InsertarArticulo()
        {
            Int32 Id = 0;
            Articulos art = new Articulos()
            {
                Descripcion = Desc,
                Precio = Precio,
                Cantidad = Cantidad,
                Rubro = Rubro,
                FechaAlta = FechaAlta,
                IdUnidadMedida = IdUnidadMedida,
                FechaBaja = null
            };
            db.Articulos.Add(art);
            db.SaveChanges();
            Id = art.IdArt;
            return Id;
        }

        public List<ConsultaArticulos> ObtenerArticulos()
        {
            List<Articulos> lart = db.Articulos.ToList();
            List<ConsultaArticulos> lconsart = new List<ConsultaArticulos>();
            foreach (Articulos art in lart)
            {
                ConsultaArticulos ca = new ConsultaArticulos();
                ca.Descripcion = art.Descripcion;
                ca.Cantidad = art.Cantidad;
                ca.FechaAlta = art.FechaAlta;
                ca.Precio = art.Precio;
                ca.NroArt = art.IdArt;
                ca.Rubro = art.Rubro;
                ca.UMedida = art.UnidadesMedida.Descripcion;
                ca.Baja = art.FechaBaja == null ? false : true;
                lconsart.Add(ca);
            }
            return lconsart;
        }

        public List<ConsultaArticulos> ObtenerArticulosxDesc(String textobusqueda)
        {
            var resultado = from a in db.Articulos
                            where a.Descripcion.Contains(textobusqueda)
                            select new ConsultaArticulos 
                            {
                                NroArt = a.IdArt,
                                Descripcion = a.Descripcion,
                                Cantidad = a.Cantidad,
                                FechaAlta = a.FechaAlta,
                                Rubro = a.Rubro,
                                Precio = a.Precio,
                                UMedida = a.UnidadesMedida.Descripcion,
                                Baja = a.FechaBaja == null ? false : true
                            };
            return resultado.ToList();
        }

        public Boolean DardeBajaArticulo(Int32 IdArticulo)
        {
            Articulos art = new Articulos(); 
            art = db.Articulos.SingleOrDefault(c => c.IdArt == IdArticulo);
            art.FechaBaja = DateTime.Now;
            db.SaveChanges();
            return true;

        }

        public Boolean EditarArticulo()
        {
            Articulos art = new Articulos();
            art = db.Articulos.SingleOrDefault(c => c.IdArt == IdArticulo);
            art.Descripcion = Desc;
            art.Cantidad = Cantidad;
            art.IdUnidadMedida = IdUnidadMedida;
            art.Precio = Precio;
            art.Rubro = Rubro;
            db.SaveChanges();
            return true;

        }

        public Articulos ObtenerArticulo(Int32 IdArticulo)
        {
            //return db.Articulos.SingleOrDefault(c => c.IdArt == IdArticulo);
            Articulos art = new Articulos();
            art = db.Articulos.SingleOrDefault(c => c.IdArt == IdArticulo);
            return art;
        }
    }

    public class ConsultaArticulos
    {
        [Display(Name= "Número Artículo")]
        public Int32 NroArt { get; set; }

        public String Descripcion { get; set; }
        public Decimal Precio { get; set; }
        public Int32 Cantidad { get; set; }
        public String Rubro { get; set; }
        public String UMedida { get; set; }
        public DateTime FechaAlta { get; set; }

        public Boolean Baja { get; set; }
    }
}
