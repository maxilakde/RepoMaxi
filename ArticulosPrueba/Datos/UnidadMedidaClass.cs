using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class UnidadMedidaClass
    {
        //public Int16 IdUnidadMedida { get; set; }
        //public String Desc { get; set; }

        private ArticulosPruebaEntities db = new ArticulosPruebaEntities();

        public List<UnidadesMedida> ObtenerUMedidas()
        {
            return db.UnidadesMedida.ToList();
        }

    }
}
