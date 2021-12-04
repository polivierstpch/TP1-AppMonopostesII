using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Entites
{
    public class Auteur : BaseEntity
    {
        public string Nom { get; set; }

        public virtual ICollection<Publication> Publications { get; set; }

        public bool Actif { get; set; }

    }
}
