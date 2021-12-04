using Blog.ApplicationCore.Entites;
using Blog.ApplicationCore.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Services
{
    public class AsyncBlogService : IAsyncBlogService
    {
        private readonly IAsyncRepository<Publication> _publicationRepository;

        private readonly IAsyncRepository<Auteur> _auteurRepository;

        private readonly IAsyncRepository<Categorie> _categorieRepository;

        public AsyncBlogService(
            IAsyncRepository<Publication> publicationRepository,
            IAsyncRepository<Auteur> auteurRepository, 
            IAsyncRepository<Categorie> categorieRepository
            )
        {
            _publicationRepository = publicationRepository;
            _auteurRepository = auteurRepository;
            _categorieRepository = categorieRepository;
        }
        public async Task AjouterPublicationAsync(Publication publication)
        {
           await _publicationRepository.AddAsync(publication);
        }

        public async Task<Publication> ObtenirPublicationParIdAsync(int id)
        {
            return await _publicationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Publication>> ObtenirPublicationsAsync()
        {
            return await _publicationRepository.ListAsync();
        }

        public async Task SupprimerPublicationAsync(Publication publication)
        {
            await _publicationRepository.DeleteAsync(publication);
        }

        public async Task ModifierPublicationAsync(Publication publication)
        {
            await _publicationRepository.EditAsync(publication);
        }
        
        public async Task<IEnumerable<Auteur>> ObtenirAuteursAsync()
        {
            return await _auteurRepository.ListAsync();
        }

        public async Task<IEnumerable<Categorie>> ObtenirCategoriesAsync()
        {
            return await _categorieRepository.ListAsync();
        }

        public async Task<Auteur> ObtenirAuteurParIdAsync(int id)
        {
            return await _auteurRepository.GetByIdAsync(id);
        }
    }
}
