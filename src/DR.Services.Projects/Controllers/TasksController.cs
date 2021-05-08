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
    public class TasksController : ControllerBase
    {
        private readonly ILogger<TasksController> logger;
        private readonly IMongoRepository<Frameworks.Projects.Models.Task> taskRepository;

        public TasksController(
            ILogger<TasksController> logger,
            IMongoRepository<Frameworks.Projects.Models.Task> taskRepository)
        {
            this.logger = logger;
            this.taskRepository = taskRepository;
        }

        [HttpGet("{id}")]
        public async Task<Components.Projects.Dto.Task> GetSingle([FromRoute] Guid id)
        {
            try
            {
                var task = await taskRepository.GetAsync(id);

                if (task is null)
                {
                    logger.LogWarning($"task not found with id: {id}");
                    return null;
                }

                logger.LogInformation($"task found with id: {id}");
                return new Components.Projects.Dto.Task
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    DueDateUtc = task.DueToUtc,
                    Priority = task.Priority,
                    Tags = task.Tags.Select(x => new Components.Projects.Dto.Tag
                    {
                        Class = x.Class,
                        Key = x.Key,
                        Value = x.Value
                    })
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return null;
            }
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Components.Projects.Dto.TaskListItem>> Get([FromQuery] Guid? projectId = null, [FromQuery] Guid? userId = null)
        {
            try
            {
                if (!projectId.HasValue && !userId.HasValue)
                {
                    logger.LogWarning("projectId and userId cannot both be null");
                    return null;
                }

                IEnumerable<Frameworks.Projects.Models.Task> tasks = null;

                if (userId.HasValue)
                {
                    tasks = await taskRepository.FindAsync(x => x.AssignedToUserId == userId);
                }
                else
                {
                    tasks = await taskRepository.FindAsync(x => x.ProjectId == projectId);
                }

                logger.LogInformation($"{tasks.Count()} tasks found with userId: {userId}");
                return tasks.Select(x => new Components.Projects.Dto.TaskListItem
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    CreatorUserId = x.CreatorUserId,
                    AssignedToUserId = x.AssignedToUserId,
                    Name = x.Name,
                    DueDateUtc = x.DueToUtc,
                    Priority = x.Priority,
                    Tags = x.Tags.Select(x => new Components.Projects.Dto.Tag
                    {
                        Class = x.Class,
                        Key = x.Key,
                        Value = x.Value
                    })
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
