using System;
using System.Threading.Tasks;
using NServiceBus;
using Octokit;

namespace ThroughputThrottlingDemo
{
    public class GitHubSearchHandler : IHandleMessages<SearchGitHub>
    {
        private static readonly GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("IgalSpamsYou"));

        public async Task Handle(SearchGitHub message, IMessageHandlerContext context)
        {
            Console.WriteLine("received search request");
            string searchTerm = "IBus";
            SearchCodeResult result = await GitHubClient.Search.SearchCode(new SearchCodeRequest(searchTerm, "Particular", "NServiceBus"));
            Console.WriteLine($"Found {result.TotalCount} results for {searchTerm}.");
        }
    }
}