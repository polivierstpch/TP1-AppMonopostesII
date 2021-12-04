using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Entites
{
    public class Categorie : BaseEntity
    { 
        public string Libelle { get; set; }

        public virtual ICollection<Publication> Publications { get; set; }
    }
}
