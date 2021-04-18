using DR.Components.Projects.Events;
using DR.Frameworks.Projects.Dto;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DR.Services.Projects.Consumers.Events
{
    public class ProjectAddedConsumer : IConsumer<ProjectCreated>
    {
        private readonly IMongoRepository<Project> projectRepository;

        public ProjectAddedConsumer(IMongoRepository<Project> projectRepository)
        {
            this.projectRepository = projectRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<ProjectCreated> context)
        {
            await projectRepository.AddAsync(new Project(
                context.Message.Id,
                context.Message.CreatorUserId,
                context.Message.Name,
                context.Message.Description,
                context.Message.IconClass,
                context.Message.IconColor,
                context.Message.IconBackgroundColor,
                context.Message.Priority,
                context.Message.Tags.Select(x => new Tag(x.Class, x.Key, x.Value))));
        }
    }
}
