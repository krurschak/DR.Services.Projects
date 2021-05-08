using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DR.Services.Projects.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger<ProjectsController> logger;
        private readonly IMongoRepository<Project> projectRepository;
        private readonly IMongoRepository<ProjectUser> projectUserRepository;

        public ProjectsController(
            ILogger<ProjectsController> logger,
            IMongoRepository<Project> projectRepository,
            IMongoRepository<ProjectUser> projectUserRepository)
        {
            this.logger = logger;
            this.projectRepository = projectRepository;
            this.projectUserRepository = projectUserRepository;
        }

        [HttpGet("{id}")]
        public async Task<Components.Projects.Dto.Project> GetSingle([FromRoute] Guid id)
        {
            try
            {
                var project = await projectRepository.GetAsync(id);
                var projectUsers = await projectUserRepository.FindAsync(x => x.ProjectId == id);

                if (project is null)
                {
                    logger.LogWarning($"project not found with id: {id}");
                    return null;
                }

                logger.LogInformation($"project found with id: {id}");
                return new Components.Projects.Dto.Project
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    IconClass = project.IconClass,
                    IconColor = project.IconColor,
                    IconBackgroundColor = project.IconBackgroundColor,
                    Priority = project.Priority,
                    UserIds = projectUsers.Select(x => x.UserId)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return null;
            }
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Components.Projects.Dto.ProjectListItem>> Get([FromQuery] Guid userId, [FromQuery] bool accepted = true)
        {
            try
            {
                var userProjects = await projectUserRepository.FindAsync(x => x.UserId == userId && x.Accepted == accepted);
                var userProjectIds = userProjects.Select(x => x.Id);

                var projects = await projectRepository.FindAsync(x => x.CreatorUserId == userId || userProjectIds.Contains(x.Id));

                logger.LogInformation($"{projects.Count()} projects found with userId: {userId}");
                return projects.Select(x => new Components.Projects.Dto.ProjectListItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    IconClass = x.IconClass,
                    IconColor = x.IconColor,
                    IconBackgroundColor = x.IconBackgroundColor,
                    Priority = x.Priority
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return null;
            }
        }
    }
}
