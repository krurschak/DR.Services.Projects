using DR.Components.Users.Events;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Repository;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DR.Services.Projects.Consumers.Events
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IMongoRepository<User> userRepository;

        public UserCreatedConsumer(IMongoRepository<User> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async System.Threading.Tasks.Task Consume(ConsumeContext<UserCreated> context)
        {
            await userRepository.AddAsync(new User(context.Message.Id, context.Message.Email));
        }
    }
}
