using DR.Components.Projects.Commands;
using DR.Components.Projects.Events;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace DR.Services.Projects.Consumers.Commands
{
    public class AddProjectConsumer : IConsumer<CreateProject>
    {
        private readonly ILogger<AddProjectConsumer> logger;
        private readonly IMongoRepository<Event> eventRepository;
        private readonly IMongoRepository<User> userRepository;

        public AddProjectConsumer(
            ILogger<AddProjectConsumer> logger,
            IMongoRepository<Event> eventRepository,
            IMongoRepository<User> userRepository)
        {
            this.logger = logger;
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<CreateProject> context)
        {
            try
            {
                foreach (var userEmail in context.Message.AddedUserEmails)
                {
                    var user = await userRepository.GetAsync(x => x.Email.ToLower() == userEmail.ToLower());

                    var projectUserAdded = new
                    {
                        UserId = user?.Id ?? NewId.NextGuid(),
                        ProjectId = context.Message.Id,
                        UserEmail = userEmail,
                        context.Message.Priority
                    };

                    if (user is null)
                    {
                        await context.Publish<ProjectUserCreated>(projectUserAdded);
                        await eventRepository.AddAsync(new Event(NewId.NextGuid(), typeof(ProjectUserCreated).FullName, JsonConvert.SerializeObject(projectUserAdded)));
                    }

                    await eventRepository.AddAsync(new Event(NewId.NextGuid(), typeof(ProjectUserAdded).FullName, JsonConvert.SerializeObject(projectUserAdded)));
                    await context.Publish<ProjectUserAdded>(projectUserAdded);
                }

                var projectCreated = new
                {
                    context.Message.Id,
                    context.Message.CreatorUserId,
                    context.Message.Name,
                    context.Message.Description,
                    context.Message.IconClass,
                    context.Message.IconColor,
                    context.Message.IconBackgroundColor,
                    context.Message.Priority,
                    context.Message.Tags
                };

                await eventRepository.AddAsync(new Event()
                {
                    Id = NewId.NextGuid(),
                    ConsumedAtUtc = DateTime.UtcNow,
                    MessageType = typeof(ProjectCreated).FullName,
                    Message = JsonConvert.SerializeObject(projectCreated)
                });

                if (context.RequestId.HasValue)
                {
                    await context.RespondAsync<ProjectCreated>(context.Message);
                }

                await context.Publish<ProjectCreated>(context.Message);

                logger.LogInformation($"project created with id: {context.Message.Id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
