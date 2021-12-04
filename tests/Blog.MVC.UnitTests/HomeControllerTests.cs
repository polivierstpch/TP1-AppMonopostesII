using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Blog.ApplicationCore.Entites;
using Blog.ApplicationCore.Interfaces;
using Blog.MVC.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blog.MVC.UnitTests
{
    public class HomeControllerTests
    {
        private readonly Fixture _fixture;
        private readonly ILogger<HomeController> _mockLogger;
        private readonly IConfiguration _mockConfiguration;
        
        private IAsyncBlogService _mockBlogService;
        private IAsyncImagesService _mockImageService;
        
        public HomeControllerTests()
        {
            _mockLogger = Mock.Of<ILogger<HomeController>>();
            _mockConfiguration = Mock.Of<IConfiguration>();
            _mockBlogService = Mock.Of<IAsyncBlogService>();
            _mockImageService = Mock.Of<IAsyncImagesService>();
            
            _fixture = new Fixture();
            
            // Pour empêcher les exceptions de récursion quand on crée des listes 
            // avec des objets avec références circulaires
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task EtantDonneIndex_QuandCategorieOuNon_AlorsRetourneViewResult()
        {
            // Étant donné...
            var publications = CreerCollectionDepuisFixture<Publication>(3);
            var mockBlogService = SetupMockBlogServicePourObtenirPublications(publications);
            var controller = CreerHomeController();
            
            // Quand...
            var viewResult = await controller.Index(string.Empty) as ViewResult;
            
            // Alors...
            viewResult.Should().BeOfType(typeof(ViewResult));
            mockBlogService.Verify(s => s.ObtenirPublicationsAsync(), Times.Once);
        }
        
        [Fact]
        public async Task EtantDonneIndex_QuandCategorieOuNon_AlorsAppelleServicePourObtenirPublications()
        {
            // Étant donné...
            var publications = CreerCollectionDepuisFixture<Publication>(3);
            var mockBlogService = SetupMockBlogServicePourObtenirPublications(publications);
            var controller = CreerHomeController();
            
            // Quand...
            await controller.Index(string.Empty);
            
            // Alors...
            mockBlogService.Verify(s => s.ObtenirPublicationsAsync(), Times.Once);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task EtantDonneIndex_QuandCategorieNonValide_AlorsRetourneViewResultAvecToutesLesPublications(string categorie)
        {
            // Étant donné...
            var publications = CreerCollectionDepuisFixture<Publication>(3);
            
            SetupMockBlogServicePourObtenirPublications(publications);
            var controller = CreerHomeController();
            
            // Quand...
            var viewResult = await controller.Index(categorie) as ViewResult;
            
            var listResult = viewResult?.Model as List<Publication>;
            listResult.Should().BeOfType(typeof(List<Publication>));
            listResult.Should().BeEquivalentTo(publications.OrderByDescending(p => p.DatePublication), options => options.WithStrictOrdering());
            listResult.Should().HaveCount(publications.Count);
        }

        [Fact]
        public async Task EtantDonneIndex_QuandCategorieValideEtContenueDansPublication_AlorsRetourneViewResultAvecPublicationsFiltrees()
        {
            // Étant donné...
            const string categorie = "Web";
            var publications = CreerCollectionDepuisFixture<Publication>(3);

            _fixture.Customize<Categorie>(composer => composer
                .With(c => c.Libelle, categorie));

            var publicationsCategorie = CreerCollectionDepuisFixture<Publication>(3);
     
            publications.AddRange(publicationsCategorie);

            SetupMockBlogServicePourObtenirPublications(publications);

            var controller = CreerHomeController();

            // Quand...
            var viewResult = await controller.Index(categorie) as ViewResult;
             
            // Alors...
            var listResult = viewResult?.Model as List<Publication>;
            listResult.Should().BeOfType(typeof(List<Publication>));
            listResult.Should().BeEquivalentTo(publicationsCategorie.OrderByDescending(p => p.DatePublication), options => options.WithStrictOrdering());
            listResult.Should().HaveCount(publicationsCategorie.Count);
        }

        [Fact]
        public async Task EtantDonnePost_QuandIdNul_AlorsRetourneNotFoundResult()
        {
            // Étant donné...
            var controller = CreerHomeController();
            
            // Quand...
            var resultat = await controller.Post(null, null);
            
            // Alors...
            resultat.Should().BeOfType(typeof(NotFoundResult));
        }

        [Fact] public async Task EtantDonnePost_QuandIdQuelconqueNonNulEtUrlSlugQuelconque_AlorsAppelleServicePourObtenirPublicationParId()
        {
            // Étant donné...
            var mockBlogService = new Mock<IAsyncBlogService>();

            _mockBlogService = mockBlogService.Object;
            
            var controller = CreerHomeController();
            
            // Quand...
            await controller.Post(1, null);
            
            // Alors...
            mockBlogService.Verify(s => s.ObtenirPublicationParIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task EtantDonnePost_QuandIdInvalide_AlorsRetourneNotFoundResult()
        {
            // Étant donné...
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirPublicationParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() => null);
            
            _mockBlogService = mockBlogService.Object;
            
            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Post(1, null);
            
            // Alors...
            result.Should().BeOfType(typeof(NotFoundResult));
            
        }

        [Fact]
        public async Task EtantDonnePost_QuandIdValideEtSlugNul_AlorsRetourneRedirectToActionResultAvecParametresDeRoute()
        {
            // Étant donné
            var publication = _fixture.Create<Publication>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirPublicationParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() => publication);

            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();

            // Quand ...
            var result = await controller.Post(1, null) as RedirectToActionResult;
            
            // Alors...
            result.Should().NotBeNull();
            result?.ActionName.Should().Be(nameof(controller.Post));
            result?.RouteValues.Should().ContainKeys("id", "urlslug");
            result?.RouteValues.Should().ContainValues(publication.Id, publication.UrlSlug);
        }

        [Fact]
        public async Task QuandPost_QuandIdValideEtSlugValide_AlorsRetourneViewResult()
        {
            // Étant donné
            var publication = _fixture.Create<Publication>();
            
            SetupMockBlogServicePourObtenirPublicationParId(publication);

            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Post(1, publication.UrlSlug) as ViewResult;
            
            // Alors...
            result.Should().BeOfType(typeof(ViewResult));
            result?.Model.Should().Be(publication);
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequeteGet_AlorsRetourneViewResult()
        {
            // Étant donné
            var controller = CreerHomeController();
            
            // Alors...
            var result = await controller.Nouveau() as ViewResult;

            result.Should().BeOfType(typeof(ViewResult));
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequeteGet_AlorsViewResultViewBagContientAuteursEtCategories()
        {
            // Étant donné
            var auteurs = CreerCollectionDepuisFixture<Auteur>(3);
            var categories = CreerCollectionDepuisFixture<Categorie>(3);

            var mockBlogService = SetupMockBlogServicePourObtenirAuteursEtCategories(auteurs, categories);

            var auteursExpected = auteurs.Select(a => new SelectListItem
            {
                Text = a.Nom,
                Value = a.Id.ToString()
            });
            
            var categoriesExpected = categories.Select(a => new SelectListItem
            {
                Text = a.Libelle,
                Value = a.Id.ToString()
            });
            
            var controller = CreerHomeController();
            
            // Alors...
            var result = await controller.Nouveau() as ViewResult;

            result.Should().NotBeNull();
            result?.ViewData.Should().NotBeNull();
            result?.ViewData["Auteurs"].Should().BeEquivalentTo(auteursExpected);
            result?.ViewData["Categories"].Should().BeEquivalentTo(categoriesExpected);
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublicationAvecDateInferieureAujourdhui_AlorsRetourneViewResult()
        {
            // Étant donné
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now.AddDays(-1)));
            
            var publication = _fixture.Create<Publication>();

            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Nouveau(publication, null) as ViewResult;
            
            // Alors...
            result.Should().BeOfType(typeof(ViewResult));
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublicationAvecAuteurInactif_AlorsRetourneViewResult()
        {
            // Étant donné
            _fixture.Customize<Auteur>(composer => composer
                .With(a => a.Actif, false));

            var auteur = _fixture.Create<Auteur>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(auteur);

            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now));
            
            var publication = _fixture.Create<Publication>();

            _mockBlogService = mockBlogService.Object;
            
            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Nouveau(publication, null) as ViewResult;
            
            // Alors...
            result.Should().BeOfType(typeof(ViewResult));
        }
        
        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublication_AlorsAppelleServicePourObtenirAuteurParId()
        {
            // Étant donné
            _fixture.Customize<Auteur>(composer => composer
                .With(a => a.Actif, false));

            var auteur = _fixture.Create<Auteur>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(auteur);

            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now));
            
            var publication = _fixture.Create<Publication>();

            _mockBlogService = mockBlogService.Object;
            
            var controller = CreerHomeController();
            
            // Quand...
            await controller.Nouveau(publication, null);
            
            // Alors...
            mockBlogService.Verify(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublicationValide_AlorsAppelleServiceImages()
        {
            // Étant donné
            var mockImageService = new Mock<IAsyncImagesService>();

            _fixture.Customize<Auteur>(composer => composer
                .With(a => a.Actif, true));
            var auteur = _fixture.Create<Auteur>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(auteur);

            var formFile = Mock.Of<IFormFile>();
            
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now));
            
            var publication = _fixture.Create<Publication>();

            _mockImageService = mockImageService.Object;
            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Nouveau(publication, formFile);
            
            // Alors...
            mockImageService.Verify(s => s.SauvegarderImageAsync(It.IsAny<IFormFile>()), Times.Once);
        }

        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublicationValide_AlorsRetourneRedirectToActionPourPost()
        {
            // Étant donné
            _fixture.Customize<Auteur>(composer => composer
                .With(a => a.Actif, true));
            var auteur = _fixture.Create<Auteur>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(auteur);
            
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now));
            
            var publication = _fixture.Create<Publication>();

            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Nouveau(publication, null) as RedirectToActionResult;
            
            // Alors...
            result.Should().BeOfType(typeof(RedirectToActionResult));
            result?.ActionName.Should().Be(nameof(controller.Post));
            result?.RouteValues["id"].Should().Be(publication.Id);
            result?.RouteValues["urlslug"].Should().Be(publication.UrlSlug);
        }
        
        [Fact]
        public async Task EtantDonneNouveau_QuandRequetePostAvecPublicationValide_AlorsAppelleServicePourAjouterPublication()
        {
            // Étant donné
            _fixture.Customize<Auteur>(composer => composer
                .With(a => a.Actif, true));
            var auteur = _fixture.Create<Auteur>();
            
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirAuteurParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(auteur);
            
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now));
            
            var publication = _fixture.Create<Publication>();

            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();
            
            // Quand...
            await controller.Nouveau(publication, null);
            
            mockBlogService.Verify(s => s.AjouterPublicationAsync(publication), Times.Once);
        }

        [Fact]
        public async Task EtantDonneSupprimer_QuandIdRepresentePublicationAvecDateUlterieureAAujourdhui_AlorsRetourneRedirectToActionVersIndex()
        {
            // Étant donné
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now.AddDays(1)));

            var publication = _fixture.Create<Publication>();

            SetupMockBlogServicePourObtenirPublicationParId(publication);

            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Supprimer(1) as RedirectToActionResult;
            
            // Alors...
            result.Should().BeOfType(typeof(RedirectToActionResult));
            result?.ActionName.Should().Be(nameof(controller.Index));
        }

        [Fact]
        public async Task EtantDonneSupprimer_QuandAppeleAvecIdRepresentantPostAvecDateInferieureOuEgalAAujourdhui_AlorsRetourneRedirectToActionVersPostAvecParametresDeRoute()
        {
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Today));
            
            var publication = _fixture.Create<Publication>();

            SetupMockBlogServicePourObtenirPublicationParId(publication);
            
            var controller = CreerHomeController();
            
            // Quand...
            var result = await controller.Supprimer(1) as RedirectToActionResult;
            
            // Alors...
            result.Should().BeOfType(typeof(RedirectToActionResult));
            result?.ActionName.Should().Be(nameof(controller.Post));
            result?.RouteValues["id"].Should().Be(publication.Id);
            result?.RouteValues["urlslug"].Should().Be(publication.UrlSlug);
        }

        [Fact]
        public async Task EtantDonneSupprimer_QuandAppeleAvecIdQuelconque_AlorsAppelleServiceBlogPourObtenirPublication()
        {
            var publication = _fixture.Create<Publication>();

            var mockBlogService = SetupMockBlogServicePourObtenirPublicationParId(publication);

            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();
            
            // Quand...
            await controller.Supprimer(1);
            
            // Alors...
            mockBlogService.Verify(s => s.ObtenirPublicationParIdAsync(It.IsAny<int>()), Times.Once);
        }
        
        [Fact]
        public async Task EtantDonneSupprimer_QuandIdRepresentantPublicationAvecDateUlterieureAAujourdhui_AlorsAppelleServiceBlogPourSupprimerPublication()
        {
            _fixture.Customize<Publication>(composer => composer
                .With(p => p.DatePublication, DateTime.Now.AddDays(1)));
            
            var publication = _fixture.Create<Publication>();

            var mockBlogService = SetupMockBlogServicePourObtenirPublicationParId(publication);

            _mockBlogService = mockBlogService.Object;

            var controller = CreerHomeController();
            
            // Quand...
            await controller.Supprimer(1);
            
            // Alors...
            mockBlogService.Verify(s => s.SupprimerPublicationAsync(publication), Times.Once);
        }
        
        /// <summary>
        /// Créer une liste avec le champ _fixture, puis retourne celle-ci.
        /// </summary>
        /// <param name="nombreItems">Nombre d'items à générer.</param>
        /// <typeparam name="T">Type de l'objet à faire une liste de.</typeparam>
        /// <returns>Liste de type T.</returns>
        private List<T> CreerCollectionDepuisFixture<T>(int nombreItems)
        {
            var liste = new List<T>();
            _fixture.AddManyTo(liste, nombreItems);

            return liste;
        }
        
        /// <summary>
        /// Change la subsitution de _mockBlogService pour lui ajouter un setup qui retournera la liste passée en paramètre.
        /// Retourn aussi l'objet de substitution pour des vérifications d'appel de méthodes ou autre.
        /// </summary>
        /// <param name="publicationsARetourner">La liste de publications à retourner par la méthode <see cref="IAsyncBlogService.ObtenirPublicationsAsync()"/>.</param>
        /// <returns>L'objet de substitution <see cref="IAsyncBlogService"/> qui a été assigné au champ <see cref="_mockBlogService"/>.</returns>
        private Mock<IAsyncBlogService> SetupMockBlogServicePourObtenirPublications(IEnumerable<Publication> publicationsARetourner)
        {
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService
                .Setup(s => s.ObtenirPublicationsAsync())
                .ReturnsAsync(publicationsARetourner);

            _mockBlogService = mockBlogService.Object;

            return mockBlogService;
        }
        
        /// <summary>
        /// Change la subsitution de _mockBlogService pour lui ajouter des setups qui retourneront la liste d'auteurs et la liste de catégories passées en paramètre.
        /// Retourne aussi l'objet de substitution pour des vérifications d'appel de méthodes ou autre.
        /// </summary>
        /// <param name="auteursARetourner">La liste d'auteur à retourner par la méthode <see cref="IAsyncBlogService.ObtenirAuteursAsync()"/></param>
        /// <param name="categoriesARetourner">La liste de catégories à retourner par la méthode <see cref="IAsyncBlogService.ObtenirCategoriesAsync()"/></param>
        /// <returns>L'objet de substitutuion <see cref="IAsyncBlogService"/> qui a été assigné au champ <see cref="_mockBlogService"/>.</returns>
        private Mock<IAsyncBlogService> SetupMockBlogServicePourObtenirAuteursEtCategories(IEnumerable<Auteur> auteursARetourner, IEnumerable<Categorie> categoriesARetourner)
        {
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService
                .Setup(s => s.ObtenirAuteursAsync())
                .ReturnsAsync(auteursARetourner);
            mockBlogService
                .Setup(s => s.ObtenirCategoriesAsync())
                .ReturnsAsync(categoriesARetourner);

            _mockBlogService = mockBlogService.Object;

            return mockBlogService;
        }
        /// <summary>
        /// Change la subsitution de _mockBlogService pour lui ajouter un setup qui retournera la publication passée en paramètre.
        /// Retourne aussi l'objet de substitution pour des vérifications d'appel de méthodes ou autre.
        /// </summary>
        /// <param name="publicationARetourner">La publication à retourner par la méthode <see cref="IAsyncBlogService.ObtenirPublicationParIdAsync(int)"/></param>
        /// <returns>L'objet de substitutuion <see cref="IAsyncBlogService"/> qui a été assigné au champ <see cref="_mockBlogService"/>.</returns>
        private Mock<IAsyncBlogService> SetupMockBlogServicePourObtenirPublicationParId(Publication publicationARetourner)
        {
            var mockBlogService = new Mock<IAsyncBlogService>();
            mockBlogService.Setup(s => s.ObtenirPublicationParIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() => publicationARetourner);

            _mockBlogService = mockBlogService.Object;
            return mockBlogService;
        }

        private HomeController CreerHomeController()
        {
            return new HomeController(
                _mockLogger,
                _mockConfiguration,
                _mockBlogService,
                _mockImageService
            );
        }
    }
}
