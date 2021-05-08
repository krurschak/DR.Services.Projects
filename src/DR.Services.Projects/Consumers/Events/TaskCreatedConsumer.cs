using DR.Components.Projects.Events;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using System.Linq;

namespace DR.Services.Projects.Consumers.Events
{
    public class TaskCreatedConsumer : IConsumer<TaskCreated>
    {
        private readonly IMongoRepository<Task> taskRepository;

        public TaskCreatedConsumer(IMongoRepository<Task> taskRepository)
        {
            this.taskRepository = taskRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<TaskCreated> context)
        {
            await taskRepository.AddAsync(new Task(
                context.Message.Id,
                context.Message.ProjectId,
                context.Message.CreatorUserId,
                context.Message.AssignedToUserId,
                context.Message.State,
                context.Message.Name,
                context.Message.Description,
                context.Message.Priority,
                context.Message.DueToUtc,
                context.Message.DependsOnTaskId,
                context.Message.Tags.Select(x => new Tag(x.Class, x.Key, x.Value))));
        }
    }
}
