using Blog.ApplicationCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Interfaces
{
    public interface IAsyncBlogService
    {
        public Task AjouterPublicationAsync(Publication publication);

        public Task<Publication> ObtenirPublicationParIdAsync(int id);

        public Task<IEnumerable<Publication>> ObtenirPublicationsAsync();

        public Task SupprimerPublicationAsync(Publication publication);

        public Task ModifierPublicationAsync(Publication publication);
        
        public Task<IEnumerable<Auteur>> ObtenirAuteursAsync();

        public Task<IEnumerable<Categorie>> ObtenirCategoriesAsync();

        public Task<Auteur> ObtenirAuteurParIdAsync(int id);

    }
}
