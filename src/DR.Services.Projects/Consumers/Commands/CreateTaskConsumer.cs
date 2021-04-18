using DR.Components.Projects.Events;
using DR.Components.Projects.Commands;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DR.Services.Projects.Consumers.Commands
{
    public class CreateTaskConsumer : IConsumer<CreateTask>
    {
        private readonly ILogger<CreateTaskConsumer> logger;
        private readonly IMongoRepository<Event> eventRepository;

        public CreateTaskConsumer(
            ILogger<CreateTaskConsumer> logger,
            IMongoRepository<Event> eventRepository)
        {
            this.logger = logger;
            this.eventRepository = eventRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<CreateTask> context)
        {
            try
            {
                var taskCreated = new
                {
                    context.Message.Id,
                    context.Message.ProjectId,
                    context.Message.CreatorUserId,
                    context.Message.AssignedToUserId,
                    context.Message.State,
                    context.Message.Name,
                    context.Message.Description,
                    context.Message.Priority,
                    context.Message.DueDateUtc,
                    context.Message.DependsOnTaskId
                };

                await eventRepository.AddAsync(new Event(NewId.NextGuid(), typeof(TaskCreated).FullName, JsonConvert.SerializeObject(taskCreated)));

                await context.Publish<TaskCreated>(taskCreated);

                logger.LogInformation($"task created with id: {context.Message.Id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
