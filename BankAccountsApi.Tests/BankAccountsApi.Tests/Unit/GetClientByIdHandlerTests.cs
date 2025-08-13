using BankAccountsApi.Features.Clients.Handlers;
using BankAccountsApi.Features.Clients.Queries;
using BankAccountsApi.Models;
using BankAccountsApi.Storage.Interfaces;
using Moq;

namespace BankAccountsApi.Tests.Unit
{
    [TestFixture]
    public class GetClientByIdHandlerTests
    {
        private Mock<IClientsRepository>? _clientRepoMock;
        private GetClientByIdHandler? _handler;

        [SetUp]
        public void Setup()
        {
            _clientRepoMock = new Mock<IClientsRepository>();
            _handler = new GetClientByIdHandler(_clientRepoMock.Object);
        }

        [Test]
        public async Task Handle_ClientExists_ReturnsSuccessWithClient()
        {
            var clientId = Guid.NewGuid();
            var client = new Client { Id = clientId, Name = "Имя Имя" };

            _clientRepoMock?.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(client);

            var query = new GetClientByIdQuery(clientId);
            var result = await _handler?.Handle(query, CancellationToken.None)!;

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value?.Id, Is.EqualTo(clientId));
        }

        [Test]
        public async Task Handle_ClientNotFound_ReturnsNotFound()
        {
            var clientId = Guid.NewGuid();

            _clientRepoMock?.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);

            var query = new GetClientByIdQuery(clientId);
            var result = await _handler?.Handle(query, CancellationToken.None)!;

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Code, Is.EqualTo("not_found"));
        }
    }
}
