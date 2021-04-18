using DR.Components.Projects.Events;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DR.Services.Projects.Consumers.Events
{
    public class ProjectUserAddedConsumer : IConsumer<ProjectUserAdded>
    {
        private readonly IMongoRepository<ProjectUser> projectUserRepository;

        public ProjectUserAddedConsumer(IMongoRepository<ProjectUser> projectUserRepository)
        {
            this.projectUserRepository = projectUserRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<ProjectUserAdded> context)
        {
            await projectUserRepository.AddAsync(new ProjectUser(
                NewId.NextGuid(),
                context.Message.ProjectId,
                context.Message.UserId));
        }
    }
}
