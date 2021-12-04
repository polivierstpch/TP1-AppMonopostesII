
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Entites
{
    public class Publication : BaseEntity
    {
       
        [Required(ErrorMessage = "Ce champ est obligatoire")]
        public string Titre { get; set; }

        [Required(ErrorMessage = "Ce champ est obligatoire")]
        [Display(Name ="Resumé")]
        public string Resume { get; set; }

     
        [Required(ErrorMessage = "Ce champ est obligatoire")]
        public string Contenu { get; set; }

        [Required(ErrorMessage = "Ce champ est obligatoire")]
        [Display(Name = "Date de publication")]
        public DateTime DatePublication { get; set; }


        [RegularExpression(@"^\S*$", ErrorMessage = "Ce champ ne peut pas contenir d'espaces")]
        [Required(ErrorMessage = "Ce champ est obligatoire")]
        [Display(Name = "URL SEO")]
        public string UrlSlug { get; set; }

        public string NomImage { get; set; }


        [Required(ErrorMessage = "Ce champ est obligatoire")]
        public int AuteurId { get; set; }

        [Required(ErrorMessage = "Ce champ est obligatoire")]
        public int CategorieId { get; set; }


        public virtual Auteur Auteur { get; set; }
        public virtual Categorie Categorie { get; set; }

    }
}
