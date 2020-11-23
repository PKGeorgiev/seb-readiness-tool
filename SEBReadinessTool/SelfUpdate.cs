using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEBReadinessTool
{
    public class SelfUpdate
    {
        private GitHubClient _client;

        public SelfUpdate()
        {
            _client = new GitHubClient(new ProductHeaderValue("SEB-Readines-Tool"));

        }

        public async Task<GitTag> GetLatestTagAsync(string owner, string repoName)
        {
            GitTag latestTag = null;

            var repos = await _client.Repository.GetAllForUser(owner);

            if (repos == null)
                return null;

            var repo = repos.Where((r) => r.Name == repoName).FirstOrDefault();

            if (repo == null)
                return null;

            latestTag = (await _client
                .Repository.GetAllTags(repo.Id, new ApiOptions()
                {
                    PageCount = 1,
                    PageSize = 5,
                    StartPage = 1
                }))
                .Select(tag =>
                {
                    return new GitTag()
                    {
                        Version = Version.Parse(tag.Name),
                        Tag = tag
                    };
                })
                .OrderByDescending(od => od.Version)
                .FirstOrDefault();


            return latestTag;
        }

        public async Task CheckForUpdate(string owner, string repoName)
        {
            var latestTag = await GetLatestTagAsync(owner, repoName);
        }
    }
}
